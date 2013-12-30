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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <remarks>
        /// This should return a user-friendly string describing what the Action does
        /// and the state of its important persistent properties.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(
                "Create IIS application pool \"{0}\" (.NET {1}) in {2} mode.",
                this.Name,
                this.ManagedRuntimeVersion,
                this.IntegratedMode ? "integrated" : "classic"
            );
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            this.LogInformation("Creating application pool \"{0}\"", this.Name);

            IISUtil.Instance.CreateAppPool(this.Name, this.User, this.Password, this.IntegratedMode, this.ManagedRuntimeVersion);

            this.LogInformation("Application pool created.");

            return null;
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }
    }
}
