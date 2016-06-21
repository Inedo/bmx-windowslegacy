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
    [DisplayName("Start App Pool")]
    [Description("Starts an application pool.")]
    [ScriptAlias("Start-AppPool")]
    [SeeAlso(typeof(StopAppPoolOperation))]
    [ScriptNamespace("IIS")]
    [DefaultProperty(nameof(AppPool))]
    public sealed class StartAppPoolOperation : RemoteExecuteOperation
    {
        [Required]
        [ScriptAlias("Name")]
        [Description("The name of the application pool to operate on.")]
        public string AppPool { get; set; }

        protected override async Task<object> RemoteExecuteAsync(IRemoteOperationExecutionContext context)
        {
            bool started = false;
            this.LogDebug($"Starting application pool {this.AppPool}...");
            try
            {
                IISUtil.Instance.StartAppPool(this.AppPool);
                started = true;
            }
            catch (IISException ex)
            {
                this.Log(ex.LogLevel, ex.Message);
            }

            await Task.Delay(100);
            if (started)
                this.LogInformation(this.AppPool + " started.");

            return null;
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Start ",
                    new Hilite(config[nameof(AppPool)]),
                    " App Pool"
                )
            );
        }
    }
}
