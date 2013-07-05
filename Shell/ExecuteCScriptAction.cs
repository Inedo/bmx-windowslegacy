using System;
using System.IO;
using System.Text;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    /// <summary>
    /// Runs a script using cscript.exe on the target server.
    /// </summary>
    [ActionProperties(
        "Execute CScript",
        "Runs a script using cscript.exe on the target server.",
        "Windows")]
    [CustomEditor(typeof(ExecuteCScriptActionEditor))]
    public sealed class ExecuteCScriptAction : AgentBasedActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteCScriptAction"/> class.
        /// </summary>
        public ExecuteCScriptAction()
        {
        }

        /// <summary>
        /// Gets or sets the path to the script to execute.
        /// </summary>
        [Persistent]
        public string ScriptPath { get; set; }

        /// <summary>
        /// Gets or sets the arguments to pass to cscript except the script path.
        /// </summary>
        [Persistent]
        public string Arguments { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <remarks>
        /// This should return a user-friendly string describing what the Action does
        /// and the state of its important persistent properties.
        /// </remarks>
        public override string ToString()
        {
            var buffer = new StringBuilder("Run ");
            buffer.Append(this.ScriptPath);
            if(!string.IsNullOrWhiteSpace(this.Arguments))
            {
                buffer.Append(' ');
                buffer.Append(this.Arguments);
                buffer.Append(' ');
            }

            buffer.Append("using cscript.exe");
            return buffer.ToString();
       }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <remarks>
        /// This method is always called on the BuildMaster server.
        /// </remarks>
        protected override void Execute()
        {
            this.LogDebug("Executing CScript...");

            var agent = this.Context.Agent.GetService<IRemoteMethodExecuter>();
            var systemPath = agent.InvokeFunc(Environment.GetFolderPath, Environment.SpecialFolder.System);
            
            var args = this.ScriptPath;
            if (!string.IsNullOrWhiteSpace(this.Arguments))
                args += " " + this.Arguments;

            this.ExecuteCommandLine(Path.Combine(systemPath, "cscript.exe"), args, this.Context.SourceDirectory);

            this.LogDebug("CScript execution complete.");
        }
    }
}
