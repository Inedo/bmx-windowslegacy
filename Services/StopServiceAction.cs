using System;
using System.ServiceProcess;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    /// <summary>
    /// An action that stops a Windows service.
    /// </summary>
    [ActionProperties(
        "Stop Service",
        "Stops a Windows service.")]
    [Tag("windows")]
    [CustomEditor(typeof(StopServiceActionEditor))]
    public sealed class StopServiceAction : RemoteActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StopServiceAction"/> class.
        /// </summary>
        public StopServiceAction()
        {
            this.WaitForStop = true;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "Stop '{0}' service{1}.",
                this.ServiceName,
                this.WaitForStop ? " and wait until its status is \"Stopped\"" : string.Empty
            );
        }

        /// <summary>
        /// Gets or sets the service to stop.
        /// </summary>
        [Persistent]
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets whether the action should continue after stopping the service
        /// or wait until its status reports stopped.
        /// </summary>
        [Persistent]
        public bool WaitForStop { get; set; }

        /// <summary>
        /// Gets or sets whether the action should ignore the error generated if the service is already
        /// stopped before the action has executed.
        /// </summary>
        [Persistent]
        public bool IgnoreAlreadyStoppedError { get; set; }

        public override ActionDescription GetActionDescription()
        {
            var longDesc = new LongActionDescription();

            return new ActionDescription(
                new ShortActionDescription(
                    "Stop ",
                    this.ServiceName,
                    " Service"
                ),
                longDesc
            );
        }

        protected override void Execute()
        {
            this.LogInformation("Stopping service {0}...", this.ServiceName);
            this.ExecuteRemoteCommand("stop");
        }
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            using (var sc = new ServiceController(this.ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
                {
                    if (this.IgnoreAlreadyStoppedError)
                        this.LogInformation("Service is already stopped.");
                    else
                        this.LogError("Service is already stopped.");

                    return null;
                }

                try
                {
                    sc.Stop();
                }
                catch (Exception ex)
                {
                    this.LogError("Service could not be stopped: " + ex.Message);
                    return null;
                }

                if (this.WaitForStop)
                {
                    this.LogInformation("Waiting for service to stop...");
                    bool stopped = false;
                    while (!stopped)
                    {
                        sc.Refresh();
                        stopped = sc.Status == ServiceControllerStatus.Stopped;
                        if (stopped)
                            break;

                        this.Context.CancellationToken.WaitHandle.WaitOne(1000 * 3);
                        this.ThrowIfCanceledOrTimeoutExpired();
                    }

                    this.LogInformation("Service stopped.");
                }
                else
                {
                    this.LogInformation("Service ordered to stop.");
                }
            }

            return null;
        }
    }
}
