using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using Inedo.BuildMaster.Diagnostics;
using Inedo.BuildMaster.Extensibility.Actions;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    /// <summary>
    /// Used primarily to pass output data from the PowerShell UI to the BuildMaster PowerShell host.
    /// </summary>
    internal class BuildMasterPSHostUserInterface : PSHostUserInterface
    {
        internal event EventHandler<LogReceivedEventArgs> LogReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildMasterPSHostUserInterface"/> class.
        /// </summary>
        public BuildMasterPSHostUserInterface()
        {
        }

        private void OnLogReceived(string message, MessageLevels messageLevel) 
        {
            if (this.LogReceived != null)
                LogReceived(this, new LogReceivedEventArgs(message, messageLevel));
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new InvalidOperationException(String.Format("BuildMaster does not support input prompts from PowerShell. Message: {0}", message));
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new InvalidOperationException(String.Format("BuildMaster does not support input prompts from PowerShell. Message: {0}", message));
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new InvalidOperationException(String.Format("BuildMaster does not support prompts for credentials from PowerShell. Message: {0}", message));
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new InvalidOperationException(String.Format("BuildMaster does not support prompts for credentials from PowerShell. Message: {0}", message));
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return null; }
        }

        public override string ReadLine()
        {
            throw new InvalidOperationException("BuildMaster does not support reading lines from PowerShell.");
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new InvalidOperationException("BuildMaster does not support reading lines from PowerShell.");
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            OnLogReceived(value, MessageLevels.Information);
        }

        public override void Write(string value)
        {
            OnLogReceived(value, MessageLevels.Information);
        }

        public override void WriteDebugLine(string message)
        {
            OnLogReceived(message, MessageLevels.Debug);
        }

        public override void WriteErrorLine(string value)
        {
            OnLogReceived(value, MessageLevels.Error);
        }

        public override void WriteLine(string value)
        {
            OnLogReceived(value, MessageLevels.Information);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
        }

        public override void WriteVerboseLine(string message)
        {
            OnLogReceived(message, MessageLevels.Debug);
        }

        public override void WriteWarningLine(string message)
        {
            OnLogReceived(String.Format("WARNING: {0}", message), MessageLevels.Information);
        }
    }
}
