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
    [CustomEditor(typeof (CreateIisAppPoolActionEditor))]
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
        /// Gets or sets a value indicating whether the action should be ignored if the App Pool aready exists.
        /// </summary>
        [Persistent]
        public bool OmitActionIfPoolExists { get; set; }

        public override ActionDescription GetActionDescription()
        {
            return new ActionDescription(
                new ShortActionDescription(
                    "Create ",
                    new Hilite(Name),
                    " IIS Application Pool"
                    ),
                new LongActionDescription(
                    "for ",
                    new Hilite(".NET " + ManagedRuntimeVersion),
                    ", ",
                    new Hilite(IntegratedMode ? "integrated" : "classic"),
                    " pipeline"
                    )
                );
        }

        protected override void Execute()
        {
            LogDebug("Creating application pool {0}...", Name);
            ExecuteRemoteCommand(null);
            LogInformation("{0} application pool created.", Name);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            LogDebug("User: " + User);
            LogDebug("Pipeline: {0} ({1})", ManagedRuntimeVersion, IntegratedMode ? "integrated" : "classic");
            if (OmitActionIfPoolExists)
            {
                if (!IISUtil.Instance.AppPoolExists(name))
                {
                    LogDebug(
                        "IIS Application Pool with name: {0} already exists. The Application Pool creation is omitted",
                        name);
                    return null;
                }
            }
            IISUtil.Instance.CreateAppPool(Name, User, Password, IntegratedMode, ManagedRuntimeVersion);

            return null;
        }
    }
}