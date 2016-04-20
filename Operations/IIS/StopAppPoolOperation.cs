using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Windows.Iis;
using Inedo.Diagnostics;
using Inedo.Documentation;

namespace Inedo.BuildMasterExtensions.Windows.Operations.IIS
{
    [Serializable]
    [DisplayName("Stop App Pool")]
    [Description("Stops an application pool.")]
    [ScriptAlias("Stop-AppPool")]
    [SeeAlso(typeof(StartAppPoolOperation))]
    [ScriptNamespace("IIS")]
    [DefaultProperty(nameof(AppPool))]
    public sealed class StopAppPoolOperation : RemoteExecuteOperation
    {
        [Required]
        [ScriptAlias("Name")]
        [Description("The name of the application pool to operate on.")]
        public string AppPool { get; set; }

        protected override async Task RemoteExecuteAsync(IRemoteOperationExecutionContext context)
        {
            bool stopped = false;
            this.LogDebug($"Stopping application pool {this.AppPool}...");
            try
            {
                IISUtil.Instance.StopAppPool(this.AppPool);
                stopped = true;
            }
            catch (IISException ex)
            {
                this.Log(ex.LogLevel, ex.Message);
            }

            await Task.Delay(100);
            if (stopped)
                this.LogInformation(this.AppPool + " stopped.");
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Stop ",
                    new Hilite(config[nameof(AppPool)]),
                    " App Pool"
                )
            );
        }
    }
}
