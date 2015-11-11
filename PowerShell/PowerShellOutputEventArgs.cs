using System;
using System.Management.Automation;

namespace Inedo.BuildMasterExtensions.Windows.PowerShell
{
    internal sealed class PowerShellOutputEventArgs : EventArgs
    {
        public PowerShellOutputEventArgs(PSObject obj)
        {
            this.Output = obj;
        }

        public PSObject Output { get; }
    }
}
