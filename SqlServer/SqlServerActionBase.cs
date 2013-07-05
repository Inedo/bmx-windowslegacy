using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{
    /// <summary>
    /// Represents the base class for all SQL Server actions
    /// </summary>
    [ActionProperties(
        "SQL Server Base Action",
        "The base action that all SQL Server actions use",
        "SQL Server")]
    [Obsolete("Use DatabaseBaseAction")]
    public abstract class SqlServerActionBase : RemoteActionBase
    {
        private string _ConnectionString;
        /// <summary>
        /// Gets or sets the database connection string
        /// </summary>
        [Obsolete("Use ProviderId", false)]
        [Persistent]
        public virtual string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }


        private int _ProviderId;
        /// <summary>
        /// Gets or sets the database provider ID
        /// </summary>
        [Persistent]
        public virtual int ProviderId
        {
            get { return _ProviderId; }
            set { _ProviderId = value; }
        }

        #region SqlHelpers
        /// <summary>
        /// Creates a <see cref="SqlConnection"/> with a connection string set
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <returns></returns>
        protected SqlConnection CreateConnection()
        {
            SqlServerDatabaseProvider sqlProv = null;
            if (string.IsNullOrEmpty(ConnectionString))
                sqlProv = (SqlServerDatabaseProvider)Util.Providers.CreateProviderFromId(ProviderId);

            SqlConnectionStringBuilder conStr = new SqlConnectionStringBuilder(sqlProv == null ? ConnectionString : sqlProv.ConnectionString);
            conStr.Pooling = false;

            SqlConnection con = new SqlConnection(conStr.ToString());
            con.InfoMessage += delegate(object sender, SqlInfoMessageEventArgs e)
            {
                LogInformation(e.Message);
            };
            return con;
        }

        /// <summary>
        /// Creates a <see cref="SqlCommand"/> with a connection string set
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <returns></returns>
        protected SqlCommand CreateCommand(string cmdText)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = base.Timeout;
            cmd.CommandText = cmdText;
            cmd.Connection = CreateConnection();
            return cmd;
        }

        /// <summary>
        /// Executes the specified command text 
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        protected void ExecuteNonQuery(string cmdText)
        {
            using (SqlCommand cmd = CreateCommand(cmdText))
            {
                try
                {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes the specified command text and returns a datatable as a result
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected DataTable ExecuteDataTable(string cmdText)
        {
            DataTable dt = new DataTable();
            using (SqlCommand cmd = CreateCommand(cmdText))
            {
                try
                {
                    cmd.Connection.Open();
                    dt.Load(cmd.ExecuteReader());
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
            return dt;
        }
       #endregion
    }
}
