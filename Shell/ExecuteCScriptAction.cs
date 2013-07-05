using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    [ActionProperties(
        "Execute CScript",
        "Runs a script using cscript.exe on the target server.",
        "Windows")]
    [CustomEditor(typeof(ExecuteCScriptActionEditor))]
    public class ExecuteCScriptAction : CommandLineActionBase
    {
        public override string ToString()
        {
            return "Run the script '" + ScriptPath + "'" +
                (!string.IsNullOrEmpty(Arguments)
                    ? " (" + Arguments + ")"
                    : "") +
                " using cscript.exe";
        }

        #region Properties
        private string _ScriptPath;
        /// <summary>
        /// Gets or sets the path to the script to execute
        /// </summary>
        [Persistent]
        public string ScriptPath
        {
            get { return _ScriptPath; }
            set { _ScriptPath = value; }
        }

        private string _Arguments = null;
        /// <summary>
        /// Gets or sets the arguments to pass to cscript except the script path
        /// </summary>
        [Persistent]
        public string Arguments
        {
            get { return _Arguments; }
            set { _Arguments = value; }
        }
        #endregion

        protected override void Execute()
        {
            LogDebug("Executing CScript...");
            ExecuteRemoteCommand(string.Empty);
            LogDebug("CScript Execution Complete.");
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            return ExecuteCommandLine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cscript.exe"),
                string.Format("{0} {1}", ScriptPath, Arguments),
                OverriddenSourceDirectory
            ); 
        }


    }
}
