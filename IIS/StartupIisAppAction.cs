using System.Threading;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    /// <summary>
    /// Represents an action that starts an IIS Application Pool.
    /// </summary>
    [ActionProperties(
        "Start IIS App Pool",
        "Starts an application pool in IIS.")]
    [Tag("windows")]
    [Tag("iis")]
    [CustomEditor(typeof(StartStopIISAppActionEditor<StartupIisAppAction>))]
    public sealed class StartupIisAppAction : RemoteActionBase, IIISAppPoolAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupIisAppAction"/> class.
        /// </summary>
        public StartupIisAppAction()
        {
        }

        /// <summary>
        /// Gets or sets the name of the application pool to start.
        /// </summary>
        [Persistent]
        public string AppPool { get; set; }

        public override string ToString()
        {
            return string.Format("Start '{0}'", this.AppPool);
        }
       
        protected override void Execute()
        {
            this.ExecuteRemoteCommand("start");
        }
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            this.LogInformation("Starting {0}...", this.AppPool);
            IISUtil.Instance.StartAppPool(this.AppPool);
            Thread.Sleep(100);
            this.LogInformation("Application pool started.");
            return string.Empty;
        }
    }
}
