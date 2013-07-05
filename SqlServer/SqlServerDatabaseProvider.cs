using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Diagnostics;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.Database;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{
    [ProviderProperties(
        "SQL Server",
        "Provides functionality for managing change scripts in Microsoft SQL Server databases.",
        IconResource = "Inedo.BuildMasterExtensions.Windows.SqlServer.sql-logo.png")]
    [CustomEditor(typeof(SqlServerDatabaseProviderEditor))]
    public sealed class SqlServerDatabaseProvider : DatabaseProviderBase, IRestoreProvider, IChangeScriptProvider, IDisposable
    {
        void ValidateInitialization()
        {
            int ver = int.Parse(ExecuteDataTable(@"
DECLARE @ver SQL_VARIANT
IF OBJECT_ID('__BuildMaster_DbSchemaChanges') IS NULL 
  SET @ver = 0
ELSE IF OBJECT_ID('__BuildMaster_ExecSql') IS NULL AND NOT EXISTS (SELECT * FROM ::fn_listextendedproperty ('__BuildMaster_Ver',NULL,NULL,NULL,NULL,NULL,NULL))
  SET @ver = 3
ELSE 
  SET @ver = COALESCE((SELECT [value] FROM ::fn_listextendedproperty ('__BuildMaster_Ver',NULL,NULL,NULL,NULL,NULL,NULL)),1)

SELECT @ver")
                .Rows[0][0].ToString());
            switch (ver)
            {
                case 0:
                    throw new InvalidOperationException("Database Not Initialized");
                case 1:
                    ExecuteNonQuery(ProviderScripts.GetReInitFrom1());
                    break;
                case 2:
                    ExecuteNonQuery(ProviderScripts.GetReInitFrom2());
                    break;
                case 3:
                    break;
                default:
                    throw new InvalidOperationException("Unexpected Ver:" + ver.ToString());
            }
        }

        private SqlConnection sharedConnection;

        #region IChangeScriptProvider Methods
        public void InitializeDatabase()
        {
            if (IsDatabaseInitialized()) throw new InvalidOperationException("Database Already Initialized");
            ExecuteNonQuery(ProviderScripts.GetInitialize());
        }

        public bool IsDatabaseInitialized()
        {
            ValidateConnection();

            return 
            //Verify __BuildMaster_DbSchemaChanges
            (bool)ExecuteDataTable
            (
                "SELECT CAST(CASE WHEN EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__BuildMaster_DbSchemaChanges') THEN 1 ELSE 0 END AS BIT)"
            ).Rows[0][0]

            //Verify Rows __BuildMaster_DbSchemaChanges
            && (bool)ExecuteDataTable
            (
                "SELECT CAST(COUNT(*) AS BIT) FROM [__BuildMaster_DbSchemaChanges]"
            ).Rows[0][0]
            ;
        }

        public ChangeScript[] GetChangeHistory()
        {
            ValidateInitialization();
            List<ChangeScript> changeScripts = new List<ChangeScript>();
            DataTable dt = ExecuteDataTable
                (
@"  SELECT [Numeric_Release_Number] 
        ,[Script_Id] 
        ,[Batch_Name] 
        ,MIN([Executed_Date]) [Executed_Date]
        ,MIN([Success_Indicator]) [Success_Indicator]
    FROM [__BuildMaster_DbSchemaChanges] 
GROUP BY [Script_Id], [Numeric_Release_Number], [Batch_Name]
ORDER BY [Numeric_Release_Number], MIN([Executed_Date]), [Batch_Name]"
                );
            foreach (DataRow dr in dt.Rows)
            {
                changeScripts.Add(new SqlServerChangeScript(dr));
            }
            return changeScripts.ToArray();
        }

        public long GetSchemaVersion()
        {
            ValidateInitialization();

            return (long)ExecuteDataTable(
                "SELECT COALESCE(MAX(Numeric_Release_Number),0) FROM __BuildMaster_DbSchemaChanges"
                ).Rows[0][0];
        }

        public ExecutionResult ExecuteChangeScript(long numericReleaseNumber, int scriptId, string scriptName, string scriptText)
        {
            this.ValidateInitialization();

            var tables = this.ExecuteDataTable("SELECT * FROM __BuildMaster_DbSchemaChanges");
            if (tables.Select("Script_Id=" + scriptId.ToString()).Length > 0)
                return GetExecutionResult(ExecutionResult.Results.Skipped, scriptName, null, null);

            var sqlMessageBuffer = new StringBuilder();
            bool errorOccured = false;
            EventHandler<LogReceivedEventArgs> logMessage = (s, e) =>
            {
                if (e.LogLevel == MessageLevels.Error)
                    errorOccured = true;

                sqlMessageBuffer.AppendLine(e.Message);
            };
            this.LogReceived += logMessage;
            
            try
            {
                using (var cmd = CreateCommand())
                {
                    int scriptSequence = 0;
                    foreach(string sqlCommand in SqlSplitter.SplitSqlScript(scriptText))
                    {
                        scriptSequence++;
                        try 
                        {
                            cmd.CommandText = sqlCommand;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex) 
                        {
                            InsertSchemaChange(numericReleaseNumber, scriptId, scriptName, scriptSequence, false);
                            return GetExecutionResult(ExecutionResult.Results.Failed, scriptName, ex, sqlMessageBuffer);
                        }

                        InsertSchemaChange(numericReleaseNumber, scriptId, scriptName, scriptSequence, true);

                        if (errorOccured)
                            return GetExecutionResult(ExecutionResult.Results.Failed, scriptName, null, sqlMessageBuffer);
                    }
                }

                return GetExecutionResult(ExecutionResult.Results.Success, scriptName, null, sqlMessageBuffer);
            }
            finally 
            {
                this.LogReceived -= logMessage;
            }
        }

        private static ExecutionResult GetExecutionResult(ExecutionResult.Results result, string scriptName, Exception ex, StringBuilder sqlMessageBuffer)
        {
            if (result == ExecutionResult.Results.Skipped)
                return new ExecutionResult(result, string.Format("The script \"{0}\" was already executed.", scriptName));
            
            if (result == ExecutionResult.Results.Success)
                return new ExecutionResult(result, string.Format("The script \"{0}\" executed successfully.", scriptName) + Util.ConcatNE(" SQL Output: ", sqlMessageBuffer.ToString()));

            if (ex != null)
                return new ExecutionResult(result, string.Format("The script \"{0}\" execution encountered a fatal error. Error details: {1}", scriptName, ex.Message) + Util.ConcatNE(" Additional SQL Output: ", sqlMessageBuffer.ToString()));

            return new ExecutionResult(result, string.Format("The script \"{0}\" execution failed.", scriptName) + Util.ConcatNE(" SQL Error: ", sqlMessageBuffer.ToString()));
        }

        private void InsertSchemaChange(long numericReleaseNumber, int scriptId, string scriptName, int scriptSequence, bool success)
        {
            this.ExecuteQuery(string.Format(
                "INSERT INTO __BuildMaster_DbSchemaChanges "
                + " (Numeric_Release_Number, Script_Id, Script_Sequence, Batch_Name, Executed_Date, Success_Indicator) "
                + "VALUES "
                + "({0}, {1}, {2}, '{3}', GETDATE(), '{4}')",
                numericReleaseNumber,
                scriptId,
                scriptSequence,
                scriptName.Replace("'", "''"),
                success ? "Y" : "N")
            );
        }
        #endregion

        #region IRestporeProvider Methods
        public void BackupDatabase(string databaseName, string destinationPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            ExecuteNonQuery(string.Format(
                "BACKUP DATABASE [{0}] TO DISK = N'{1}' WITH FORMAT",
                databaseName.Replace("]", "]]"), //I seriously hope someone doesn't have a ] in their DB name
                destinationPath.Replace("'", "''")));
        }

        public void RestoreDatabase(string databaseName, string sourcePath)
        {
            ExecuteNonQuery(string.Format(
                "USE master IF DB_ID('{0}') IS NULL CREATE DATABASE [{1}]",
                databaseName.Replace("'", "''"),
                databaseName.Replace("]", "]]")
                ));

            ExecuteNonQuery(string.Format(
                "ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                databaseName.Replace("]", "]]")));

            ExecuteNonQuery(string.Format(
                "USE master RESTORE DATABASE [{0}] FROM DISK = N'{1}' WITH REPLACE",
                databaseName.Replace("]", "]]"),
                sourcePath.Replace("'", "''")));

            ExecuteNonQuery(string.Format(
                "ALTER DATABASE [{0}] SET MULTI_USER",
                databaseName.Replace("]", "]]")));
        }
        #endregion

        #region DatabaseProviderBase Methods
        public override void ExecuteQueries(string[] queries)
        {
            using (SqlCommand cmd = CreateCommand())
            {
                try
                {
                    foreach (string query in queries)
                    {
                        foreach (string splitQuery in SqlSplitter.SplitSqlScript(query))
                        {
                            try
                            {
                                cmd.CommandText = splitQuery;
                                cmd.ExecuteNonQuery();
                            }
                            catch
                            {
                                // TODO: how to read action properties in this method?
                                // Error reporting?
                                //if (!ResumeNextOnError) throw;
                            }
                        }
                    }
                }
                finally
                {
                    if(this.sharedConnection == null)
                        cmd.Connection.Close();
                }
            }
        }

        public override void ExecuteQuery(string query)
        {
            ExecuteNonQuery(query);
        }

        public override void OpenConnection()
        {
            if (this.sharedConnection != null)
                this.sharedConnection = CreateConnection();
        }

        public override void CloseConnection()
        {
            if (this.sharedConnection != null)
            {
                this.sharedConnection.Dispose();
                this.sharedConnection = null;
            }
        }
        #endregion

        #region ProviderBaseMethods
        public override bool IsAvailable()
        {
            return true;
        }

        public override void ValidateConnection()
        {
            DataRow dr = ExecuteDataTable
            (
                "SELECT CAST(IS_MEMBER('db_owner') AS BIT) isDbOwner"
            ).Rows[0];

            bool db_owner = !Convert.IsDBNull(dr[0]) && (bool)dr[0];

            if (!db_owner) throw new NotAvailableException(
                "The ConnectionString credentials must have 'db_owner' privileges.");
        }
        #endregion

        public override string ToString()
        {
            try
            {
                var csb = new SqlConnectionStringBuilder(ConnectionString);
                var toString = new StringBuilder();
                if (!string.IsNullOrEmpty(csb.InitialCatalog))
                    toString.Append("SQL Server database \"" + csb.InitialCatalog + "\"");
                else
                    toString.Append("SQL Server database");

                if (!string.IsNullOrEmpty(csb.DataSource))
                    toString.Append(" on server \"" + csb.DataSource + "\"");

                return toString.ToString();
            }
            catch { /* GULP */ }
            return "SQL Server database";
        }

        public void Dispose()
        {
            CloseConnection();
        }

        #region SqlHelpers
        private SqlConnection CreateConnection()
        {
            var conStr = new SqlConnectionStringBuilder(ConnectionString) 
            {
                Pooling = false
            };

            var con = new SqlConnection(conStr.ToString())
            {
                FireInfoMessageEventOnUserErrors = true
            };
            con.InfoMessage += (s, e) =>
            {
                foreach (SqlError errorMessage in e.Errors)
                {
                    if (errorMessage.Class > 10)
                        this.LogError(errorMessage.Message);
                    else
                        this.LogInformation(errorMessage.Message);
                }
            };

            return con;
        }

        private SqlCommand CreateCommand()
        {
            var cmd = new SqlCommand()
            {
                CommandTimeout = 0
            };

            if (this.sharedConnection != null)
            {
                cmd.Connection = this.sharedConnection;
            }
            else
            {
                var con = CreateConnection();
                con.Open();
                cmd.Connection = con;
            }

            return cmd;
        }

        private void ExecuteNonQuery(string cmdText)
        {
            using (SqlCommand cmd = CreateCommand())
            {
                try
                {
                    foreach (var commandText in SqlSplitter.SplitSqlScript(cmdText))
                    {
                        try
                        {
                            cmd.CommandText = commandText;
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            // TODO: no action context; not sure whether to continue on next
                            throw;
                        }
                    }
                }
                finally
                {
                    if(this.sharedConnection == null)
                        cmd.Connection.Close();
                }
            }
        }

        private DataTable ExecuteDataTable(string cmdText)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = CreateCommand())
            {
                cmd.CommandText = cmdText;
                try
                {
                    dt.Load(cmd.ExecuteReader());
                }
                finally
                {
                    if (this.sharedConnection == null)
                        cmd.Connection.Close();
                }
            }
            return dt;
        }
        #endregion
    }
}
