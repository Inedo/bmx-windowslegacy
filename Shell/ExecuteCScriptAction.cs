using System;
using System.IO;
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
        "Runs a script using cscript.exe on the target server.")]
    [Tag("windows")]
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

        public override ActionDescription GetActionDescription()
        {
            var longDesc = new LongActionDescription();
            if (!string.IsNullOrWhiteSpace(this.Arguments))
            {
                longDesc.AppendContent(
                    "with arguments: ",
                    new Hilite(this.Arguments)
                );
            }

            return new ActionDescription(
                new ShortActionDescription(
                    "Run ",
                    new DirectoryHilite(this.OverriddenSourceDirectory, this.ScriptPath),
                    " using cscript.exe"
                ),
                longDesc
            );
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <remarks>
        /// This method is always called on the BuildMaster server.
        /// </remarks>
        protected override void Execute()
        {
            this.LogDebug("Arguments: " + this.Arguments);
            this.LogInformation("Executing CScript.exe {0}...", this.ScriptPath);

            var agent = this.Context.Agent.GetService<IRemoteMethodExecuter>();
            var systemPath = agent.InvokeFunc(Environment.GetFolderPath, Environment.SpecialFolder.System);

            var args = "\"" + this.ScriptPath + "\"";
            if (!string.IsNullOrWhiteSpace(this.Arguments))
                args += " " + this.Arguments;

            this.ExecuteCommandLine(Path.Combine(systemPath, "cscript.exe"), args, this.Context.SourceDirectory);

            this.LogInformation("CScript execution complete.");
        }
    }
}
