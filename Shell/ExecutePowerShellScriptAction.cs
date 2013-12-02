using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    /// <summary>
    /// Runs a PowerShell script on a remote server.
    /// </summary>
    [ActionProperties(
        "Execute PowerShell Script",
        "Runs a PowerShell script on the target server.")]
    [Tag("windows")]
    [CustomEditor(typeof(ExecutePowerShellScriptActionEditor))]
    public sealed class ExecutePowerShellScriptAction : RemoteActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutePowerShellScriptAction"/> class.
        /// </summary>
        public ExecutePowerShellScriptAction()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Script property is a script file name (true)
        /// or a script string (false).
        /// </summary>
        [Persistent]
        public bool IsScriptFile { get; set; }
        /// <summary>
        /// Gets or sets either the name of a script file to execute or the script itself
        /// depending on the value of the IsScriptFile property.
        /// </summary>
        [Persistent]
        public string Script { get; set; }
        /// <summary>
        /// Gets or sets a set of variable name/value pairs to pass to the script.
        /// </summary>
        /// <remarks>
        /// Each string should be of the form:
        ///   VariableName=VariableValue
        /// </remarks>
        [Persistent]
        public string[] Variables { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the results of the script should be logged.
        /// </summary>
        [Persistent]
        public bool LogResults { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Runs the PowerShell script " + GetScriptText();
        }

        protected override void Execute()
        {
            var results = ExecuteRemoteCommand("runscript");

            if (this.LogResults && !string.IsNullOrEmpty(results))
            {
                foreach (var item in Util.Persistence.DeserializeToStringArray(results))
                    this.LogInformation(item);
            }
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            var variables = ParseVariables();

            var host = new BuildMasterPSHost();
            
            if (this.LogResults)
                host.LogReceived += new EventHandler<LogReceivedEventArgs>(host_LogReceived);

            using (var runspace = RunspaceFactory.CreateRunspace(host))
            {
                runspace.Open();

                foreach (var pair in variables)
                    runspace.SessionStateProxy.SetVariable(pair.Key, pair.Value);
                
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;

                    if (this.IsScriptFile)
                    {
                        if (string.IsNullOrEmpty(this.Script))
                            throw new InvalidOperationException("No script file has been specified.");

                        string scriptFilePath = Path.Combine(this.Context.SourceDirectory, this.Script);
                        if (!File.Exists(scriptFilePath))
                        {
                            LogError(string.Format("The script file '{0}' either does not exist or BuildMaster does not have permission to access it.", this.Script));
                            return string.Empty;
                        }

                        ps.Commands.AddScript(File.ReadAllText(scriptFilePath));
                    }
                    else
                    {
                        ps.Commands.AddScript(this.Script);
                    }

                    // capture errors from "write-error" here since apparently a custom host cannot do this
                    ps.AddCommand("out-default");
                    ps.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(Error_DataAdded);

                    ps.Invoke();
                }
            }

            return string.Empty;
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var errors = (PSDataCollection<ErrorRecord>)sender;

            // must use a for loop here instead of foreach because the script 
            // execution will never end if you use foreach...
            for (int i = 0; i < errors.Count; i++)
            {
                LogPowerShellError(errors[i]);
            }
        }

        private void LogPowerShellError(ErrorRecord error)
        {
            if (error.FullyQualifiedErrorId == "HostFunctionNotImplemented,Microsoft.PowerShell.Commands.WriteHostCommand")
                LogError("PowerShell returned the error: \"Cannot invoke this function because the current host does not implement it\". BuildMaster cannot handle the \"write-host\" cmdlet, please use \"write-output\" instead.");
            else
                LogError(error.Exception.Message);
        }

        private void host_LogReceived(object sender, LogReceivedEventArgs e)
        {
            Log(e.LogLevel, e.Message);
        }

        private Dictionary<string, string> ParseVariables()
        {
            var variables = new Dictionary<string, string>();

            var rows = this.Variables ?? new string[0];
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row))
                    continue;

                var pair = row.Split(new[] { '=' }, 2, StringSplitOptions.None);
                if (pair.Length != 2)
                    throw new InvalidDataException("Unable to parse variable definition: " + row);

                string name = pair[0].TrimStart('$');
                string value = pair[1];

                if (name == String.Empty)
                    throw new InvalidDataException("Unable to parse variable definition: " + row);

                if (value == String.Empty)
                {
                    LogDebug("Variable \"{0}\" has no value, skipping.");
                    continue;
                }

                if (!variables.ContainsKey(name))
                {
                    variables.Add(name, value);
                }
                else
                {
                    LogDebug(String.Format("Variable \"{0}\" already declared, existing value will be overwritten.", name));
                    variables[name] = value;
                }
            }

            return variables;
        }

        private string GetScriptText()
        {
            if (this.IsScriptFile)
                return string.Format("file '{0}'", this.Script);
            else
            {
                var script = this.Script ?? string.Empty;
                if (script.Length > 100)
                    script = script.Substring(0, 100) + "...";

                return "'" + script.Replace(Environment.NewLine, " ") + "'";
            }
        }
    }
}
