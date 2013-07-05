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
        "Stop IIS App Pool",
        "Stops an application pool in IIS.",
        "Windows")]
    [CustomEditor(typeof(StartStopIISAppActionEditor<ShutdownIisAppAction>))]
    public sealed class ShutdownIisAppAction : RemoteActionBase, IIISAppPoolAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShutdownIisAppAction"/> class.
        /// </summary>
        public ShutdownIisAppAction()
        {
        }

        /// <summary>
        /// Gets or sets the name of the application pool to stop.
        /// </summary>
        [Persistent]
        public string AppPool { get; set; }

        public override string ToString()
        {
            return string.Format("Stop '{0}'", this.AppPool);
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand("stop");
        }
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            this.LogInformation("Stopping {0}...", this.AppPool);
            IISUtil.Instance.StopAppPool(this.AppPool);
            Thread.Sleep(100);
            this.LogInformation("Application pool stopped.");
            return string.Empty;
        }
    }
}
