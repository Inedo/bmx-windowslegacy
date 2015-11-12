using System;
using System.ServiceProcess;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;
using Inedo.BuildMasterExtensions.Windows.ActionImporters;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    [ActionProperties(
        "Start Service",
        "Starts a Windows Service.")]
    [Tag("windows")]
    [CustomEditor(typeof(StartServiceActionEditor))]
    [ConvertibleToOperation(typeof(StartServiceImporter))]
    public sealed class StartServiceAction : RemoteActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartServiceAction"/> class.
        /// </summary>
        public StartServiceAction()
        {
            this.WaitForStart = true;
        }

        /// <summary>
        /// Gets or sets the service to start.
        /// </summary>
        [Persistent]
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the startup arguments for the service.
        /// </summary>
        [Persistent]
        public string[] StartupArgs  { get; set; }

        /// <summary>
        /// Determines whether the action should or should immediately continue after starting the service
        /// or wait until its status reports running.
        /// </summary>
        [Persistent]
        public bool WaitForStart  { get; set; }

        /// <summary>
        /// Gets or sets whether the action should ignore the error generated if the service is already
        /// started before the action has executed.
        /// </summary>
        [Persistent]
        public bool IgnoreAlreadyStartedError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service being unable to start should
        /// be considered a warning or an error.
        /// </summary>
        [Persistent]
        public bool TreatUnableToStartAsWarning { get; set; }

        public override ActionDescription GetActionDescription()
        {
            var longDesc = new LongActionDescription();
            if (this.StartupArgs != null && this.StartupArgs.Length > 0)
            {
                longDesc.AppendContent(
                    "with arguments: ",
                    new Hilite(string.Join(" ", this.StartupArgs))
                );
            }

            return new ActionDescription(
                new ShortActionDescription(
                    "Start ",
                    this.ServiceName,
                    " Service"
                ),
                longDesc
            );
        }

        protected override void Execute()
        {
            this.LogInformation("Starting service {0}...", this.ServiceName);
            this.ExecuteRemoteCommand("start");
        }
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            using (var sc = new ServiceController(this.ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    if (this.IgnoreAlreadyStartedError)
                        this.LogInformation("Service is already running.");
                    else
                        this.LogError("Service is already running.");

                    return null;
                }

                try
                {
                    sc.Start(this.StartupArgs ?? new string[0]);
                }
                catch (Exception ex)
                {
                    this.LogErrorWarning("Service could not be started: " + ex.Message);
                    return null;
                }

                if (this.WaitForStart)
                {
                    this.LogInformation("Waiting for service to start...");
                    bool started = false;
                    while (!started)
                    {
                        sc.Refresh();
                        started = sc.Status == ServiceControllerStatus.Running;
                        if (started)
                            break;

                        this.Context.CancellationToken.WaitHandle.WaitOne(1000 * 3);
                        this.ThrowIfCanceledOrTimeoutExpired();

                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            this.LogErrorWarning("Service stopped immediately after starting.");
                            return null;
                        }
                    }
                    
                    this.LogInformation("Service started.");
                }
                else
                {
                    this.LogInformation("Service ordered to start.");
                }
            }

            return null;
        }

        /// <summary>
        /// Invokes either log warning or log error depending on the mode.
        /// </summary>
        /// <param name="message">Message to log.</param>
        private void LogErrorWarning(string message)
        {
            if (this.TreatUnableToStartAsWarning)
                this.LogWarning(message);
            else
                this.LogError(message);
        }
    }
}