using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;
using Inedo.BuildMaster.Data;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{

    /// <summary>
    /// Represents an action used to execute scripts with the ChangeSchema
    /// </summary>
    /// <remarks>
    /// See this class's ActionProperties attribute for a full description
    /// </remarks>
    [ActionProperties(
        "Execute SQL Server Change Scripts",
        "Creates a special table (__BuildMaster_DbSchemaChanges) and stored procedure (__BuildMaster_Exec) in the target database, and executes change scripts in the source directory that are tied to a specific release.",
        "SQL Server")]
    [CustomEditor("Inedo.BuildMasterExtensions.Windows.SqlServer.ExecuteSqlChangeScriptsActionEditor.ascx")]
    public class ExecuteSqlChangeScriptsAction : SqlServerActionBase
    {
        /// <summary>
        /// See <see cref="object.ToString()"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "Execute Change Scripts ({0}) in {1}.",
                SearchPattern,
                string.IsNullOrEmpty(OverriddenSourceDirectory)
                    ? "default directory"
                    : OverriddenSourceDirectory
                );
        }

        private string getExecSproc()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            TextReader textReader = new StreamReader(
                assembly.GetManifestResourceStream(
                    "Inedo.BuildMasterExtensions.Windows.__BuildMaster_Exec.sql"));
            string result = textReader.ReadToEnd();
            textReader.Close();
            return result;
        }

        #region Properties
        private string _SearchPattern = ".sql";
        [Persistent]
        /// <summary>
        /// Gets or sets the search pattern 
        /// </summary>
        /// <remarks>
        ///     The search string to match against the names of files in path. The parameter
        ///     cannot end in two periods ("..") or contain two periods ("..") followed by
        ///     System.IO.Path.DirectorySeparatorChar or System.IO.Path.AltDirectorySeparatorChar,
        ///     nor can it contain any of the characters in System.IO.Path.InvalidPathChars.
        /// </remarks>
        public string SearchPattern
        {
            get { return _SearchPattern; }
            set { _SearchPattern = value; }
        }
        #endregion

        protected override void Execute()
        {

            LogInformation("Loading releases...");
            DataTable releases = StoredProcs.Releases_GetReleases(Context.ApplicationId,null,null).ExecuteDataTable();

            LogInformation("Initializing Schema...");
            ExecuteRemoteCommand("InitExec");

            LogInformation("Finding scripts to execute...");
            string[] filePaths = Util.Persistence.DeSerializeToStringArray(ExecuteRemoteCommand("GetFiles"));

            LogInformation("Found " + filePaths.Length.ToString() + " total script(s).");
            Array.Sort<string>(filePaths);
            
            foreach (string filePath in filePaths)
            {
                //chomp RemoteConfiguration.SourceDirectory
                string relativeFilePath = filePath.StartsWith(RemoteConfiguration.SourceDirectory)
                        ? filePath.Substring(RemoteConfiguration.SourceDirectory.Length)
                        : filePath;
                relativeFilePath = relativeFilePath.TrimStart(Path.DirectorySeparatorChar);

                //extract release number
                string releaseNumber = relativeFilePath.Contains(Path.DirectorySeparatorChar.ToString())
                    ? relativeFilePath.Substring(0, relativeFilePath.IndexOf(Path.DirectorySeparatorChar))
                    : string.Empty;
                if (string.IsNullOrEmpty(releaseNumber))
                {
                    LogInformation("Release Number not found on \"" + relativeFilePath + "\", skipping.");
                    continue;
                }

                //find release row
                DataRow[] releaseRow = releases.Select(string.Format("{0} = '{1}'",
                    TableDefs.Releases.Release_Number,
                    releaseNumber));
                if (releaseRow.Length == 0)
                {
                    LogInformation(string.Format(
                        "Release Number \"{0}\" not found, skipping file \"{1}\".",
                        releaseNumber,
                        relativeFilePath));
                    continue;
                }
                string schemaVersion =
                    releaseRow[0][TableDefs.Releases_Extended.Sortable_Release_Number]
                    .ToString()
                    .Replace(' ', '_');

                //execute against release
                LogInformation(string.Format(
                    "Executing \"{0}\" for release \"{1}\" ... ",
                    relativeFilePath,
                    releaseRow[0][TableDefs.Releases_Extended.Release_Number]
                    ));
                ExecuteRemoteCommand(
                    "ExecuteVersionedScript", 
                    filePath,
                    relativeFilePath,
                    schemaVersion);
            }

            LogInformation("Scripts complete.");

        }
        
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            if (name == "InitExec")
            {
                ExecuteNonQuery("IF OBJECT_ID('__BuildMaster_Exec') IS NOT NULL DROP PROCEDURE [__BuildMaster_Exec]");
                ExecuteNonQuery(getExecSproc());
            }
            else if (name == "GetFiles")
            {
                return Util.Persistence.SerializeStringArray(
                    Directory.GetFiles(
                        RemoteConfiguration.SourceDirectory,
                        SearchPattern,
                        SearchOption.AllDirectories));
            }
            else if (name == "ExecuteVersionedScript")
            {
                string filePath = args[0];
                string relativeFilePath = args[1];
                string schemaVer = args[2];
                if (!File.Exists(filePath)) throw new FileNotFoundException();

                if (relativeFilePath.Length > 200) relativeFilePath = relativeFilePath.Substring(0, 200);

                using (SqlCommand cmd = CreateCommand("__BuildMaster_Exec"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        cmd.Connection.Open();
                        cmd.Parameters.AddWithValue("@SchemaVersion_Id", schemaVer);
                        cmd.Parameters.AddWithValue("@Script_Name", relativeFilePath);
                        cmd.Parameters.AddWithValue("@Script_Sql", string.Empty);

                        string[] sqlCommands = SqlServerHelper.SplitScriptBatch(File.ReadAllText(filePath));
                        for (int i = 0; i < sqlCommands.Length; i++)
                        {
                            if (string.IsNullOrEmpty(sqlCommands[i])) continue;
                            if (sqlCommands.Length > 1)
                            {
                                string suffix = string.Format(" ({0})", sqlCommands.Length);
                                if (relativeFilePath.Length > 200 - suffix.Length)
                                    relativeFilePath = relativeFilePath.Substring(0, suffix.Length);

                                suffix = string.Format(" ({0})", i);
                                cmd.Parameters["@Script_Name"].Value = relativeFilePath + suffix;
                            }
                            cmd.Parameters["@Script_Sql"].Value = sqlCommands[i];

                            //Execute
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException)
                            {
                                if (!ResumeNextOnError) throw;
                            }
                        }
                    }
                    finally
                    {
                        cmd.Connection.Close();
                    }
                }

            }
            return string.Empty;
        }
    }
}
