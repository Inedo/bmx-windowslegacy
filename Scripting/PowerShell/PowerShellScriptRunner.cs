using System.Management.Automation;
using System.Threading.Tasks;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Scripting;
using Inedo.Diagnostics;

namespace Inedo.BuildMasterExtensions.Windows.Scripting.PowerShell
{
    internal sealed class PowerShellScriptRunner : ActiveScriptBase
    {
        private System.Management.Automation.PowerShell powerShell;
        private PSDataCollection<PSObject> outputData = new PSDataCollection<PSObject>();
        private Task runScriptTask;
        private bool disposed;

        public PowerShellScriptRunner(System.Management.Automation.PowerShell powerShell)
        {
            this.powerShell = powerShell;
            this.powerShell.Streams.Debug.DataAdded += this.Debug_DataAdded;
            this.powerShell.Streams.Error.DataAdded += this.Error_DataAdded;
            this.powerShell.Streams.Verbose.DataAdded += this.Verbose_DataAdded;
            this.powerShell.Streams.Warning.DataAdded += this.Warning_DataAdded;
            this.outputData.DataAdded += this.Output_DataAdded;
        }

        public override void Start()
        {
            this.runScriptTask = Task.Factory.FromAsync(
                this.powerShell.BeginInvoke<object, PSObject>(null, this.outputData),
                a => { this.powerShell.EndInvoke(a); }
            ).ContinueWith(this.HandleScriptCompleted);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.powerShell.Streams.Debug.DataAdded -= this.Debug_DataAdded;
                this.powerShell.Streams.Error.DataAdded -= this.Error_DataAdded;
                this.powerShell.Streams.Verbose.DataAdded -= this.Verbose_DataAdded;
                this.powerShell.Streams.Warning.DataAdded -= this.Warning_DataAdded;
                this.outputData.DataAdded -= this.Output_DataAdded;
                this.powerShell.Dispose();
                this.disposed = true;
            }

            base.Dispose(disposing);
        }

        private void HandleScriptCompleted(Task task)
        {
            var exception = task.Exception;
            if (exception == null)
            {
                this.ScriptCompleted(true, null);
                return;
            }

            this.OnLogReceived(new LogReceivedEventArgs(exception.InnerException.ToString(), MessageLevel.Error));
            this.ScriptCompleted(false, null);
        }
        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            this.OnLogReceived(new LogReceivedEventArgs(((PSDataCollection<DebugRecord>)sender)[e.Index].ToString(), MessageLevel.Debug));
        }
        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            this.OnLogReceived(new LogReceivedEventArgs(((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString(), MessageLevel.Error));
        }
        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            this.OnLogReceived(new LogReceivedEventArgs(((PSDataCollection<VerboseRecord>)sender)[e.Index].ToString(), MessageLevel.Debug));
        }
        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            this.OnLogReceived(new LogReceivedEventArgs(((PSDataCollection<WarningRecord>)sender)[e.Index].ToString(), MessageLevel.Warning));
        }
        private void Output_DataAdded(object sender, DataAddedEventArgs e)
        {
            this.OnLogReceived(new LogReceivedEventArgs(this.outputData[e.Index].ToString(), MessageLevel.Information));
        }
    }
}
