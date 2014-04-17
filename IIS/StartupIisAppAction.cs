using System.Threading;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
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
        /// Gets or sets the name of the application pool to start.
        /// </summary>
        [Persistent]
        public string AppPool { get; set; }

        public override ActionDescription GetActionDescription()
        {
            return new ActionDescription(
                new ShortActionDescription(
                    "Start ",
                    new Hilite(this.AppPool),
                    " application pool"
                )
            );
        }
       
        protected override void Execute()
        {
            this.LogDebug("Starting application pool {0}...", this.AppPool);
            if (this.ExecuteRemoteCommand("start") == Domains.YN.Yes)
                this.LogInformation("{0} application pool started.", this.AppPool);
        }
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            try
            {
                IISUtil.Instance.StartAppPool(this.AppPool);
                Thread.Sleep(100);
                return Domains.YN.Yes;
            }
            catch (IISException ex)
            {
                this.Log(ex.LogLevel, ex.Message);
                return Domains.YN.No;
            }
        }
    }
}