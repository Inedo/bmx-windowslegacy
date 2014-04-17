using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    [ActionProperties(
       "Create IIS 7+ App Pool",
       "Creates an application pool in IIS 7 or later.")]
    [Tag(Tags.Windows)]
    [Tag("iis")]
    [CustomEditor(typeof(CreateIisAppPoolActionEditor))]
    public sealed class CreateIisAppPoolAction : RemoteActionBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Persistent]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        [Persistent]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Persistent]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the app pool should be integrated mode or classic.
        /// </summary>
        [Persistent]
        public bool IntegratedMode { get; set; }

        /// <summary>
        /// Gets or sets the managed runtime version.
        /// </summary>
        /// <remarks>Valid values are v2.0 and v4.0</remarks>
        [Persistent]
        public string ManagedRuntimeVersion { get; set; }

        public override ActionDescription GetActionDescription()
        {
            return new ActionDescription(
                new ShortActionDescription(
                    "Create ",
                    new Hilite(this.Name),
                    " IIS Application Pool"
                ),
                new LongActionDescription(
                    "for ",
                    new Hilite(".NET " + this.ManagedRuntimeVersion),
                    ", ",
                    new Hilite(this.IntegratedMode ? "integrated" : "classic"),
                    " pipeline"
                )
            );
        }

        protected override void Execute()
        {
            this.LogDebug("Creating application pool {0}...", this.Name);
            this.ExecuteRemoteCommand(null);
            this.LogInformation("{0} application pool created.", this.Name);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            this.LogDebug("User: " + this.User);
            this.LogDebug("Pipeline: {0} ({1})", this.ManagedRuntimeVersion, this.IntegratedMode ? "integrated" : "classic");

            IISUtil.Instance.CreateAppPool(this.Name, this.User, this.Password, this.IntegratedMode, this.ManagedRuntimeVersion);
            return null;
        }
    }
}
