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
        "Stops an application pool in IIS.")]
    [Tag("windows")]
    [Tag("iis")]
    [CustomEditor(typeof(StartStopIISAppActionEditor<ShutdownIisAppAction>))]
    public sealed class ShutdownIisAppAction : RemoteActionBase, IIISAppPoolAction
    {
        /// <summary>
        /// Gets or sets the name of the application pool to stop.
        /// </summary>
        [Persistent]
        public string AppPool { get; set; }

        public override ActionDescription GetActionDescription()
        {
            return new ActionDescription(
                new ShortActionDescription(
                    "Stop ",
                    new Hilite(this.AppPool),
                    " application pool"
                )
            );
        }

        protected override void Execute()
        {
            this.LogDebug("Stopping application pool {0}...", this.AppPool);
            this.ExecuteRemoteCommand("stop");
            this.LogInformation("{0} application pool stopped.", this.AppPool);
        }
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            IISUtil.Instance.StopAppPool(this.AppPool);
            Thread.Sleep(100);
            return string.Empty;
        }
    }
}
