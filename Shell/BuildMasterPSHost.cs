using System;
using System.Globalization;
using System.Management.Automation.Host;
using Inedo.BuildMaster.Extensibility.Actions;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    /// <summary>
    /// Custom host for PowerShell used to capture non-error messages so we can 
    /// log everything except errors.
    /// </summary>
    internal class BuildMasterPSHost : PSHost
    {
        /// <summary>
        /// Fires when something is written to the user interface.
        /// </summary>
        public event EventHandler<LogReceivedEventArgs> LogReceived;

        /// <summary>
        /// Gets the exit code returned from the PowerShell console.
        /// </summary>
        public int ExitCode { get; private set; }

        private Guid id = Guid.NewGuid();
        private BuildMasterPSHostUserInterface psHostUserInterface = new BuildMasterPSHostUserInterface();

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildMasterPSHost"/> class.
        /// </summary>
        public BuildMasterPSHost()
        {
            psHostUserInterface.LogReceived += new EventHandler<LogReceivedEventArgs>(psHostUserInterface_LogReceived);
        }

        private void psHostUserInterface_LogReceived(object sender, LogReceivedEventArgs e)
        {
            if (LogReceived != null)
                LogReceived(sender, e);
        }

        public override CultureInfo CurrentCulture
        {
            get { return CultureInfo.InvariantCulture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return CultureInfo.InvariantCulture; }
        }

        public override void EnterNestedPrompt()
        {
        }

        public override void ExitNestedPrompt()
        {
        }

        public override Guid InstanceId
        {
            get { return this.id; }
        }

        public override string Name
        {
            get { return "BuildMasterPowerShellHost"; }
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
            this.ExitCode = exitCode;
        }

        public override PSHostUserInterface UI
        {
            get { return this.psHostUserInterface; }
        }

        public override Version Version
        {
            get { return new Version(0, 0, 0, 0); }
        }
    }
}
