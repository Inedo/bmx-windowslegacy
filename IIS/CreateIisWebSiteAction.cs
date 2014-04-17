using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    [ActionProperties(
       "Create IIS 7+ Web Site",
       "Creates a new web site in IIS 7 or later.")]
    [Tag(Tags.Windows)]
    [Tag("iis")]
    [CustomEditor(typeof(CreateIisWebSiteActionEditor))]
    public sealed class CreateIisWebSiteAction : RemoteActionBase
    {
        /// <summary>
        /// Gets or sets the name of the web site.
        /// </summary>
        [Persistent]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the physical path.
        /// </summary>
        [Persistent]
        public string PhysicalPath { get; set; }

        /// <summary>
        /// Gets or sets the application pool.
        /// </summary>
        [Persistent]
        public string ApplicationPool { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        [Persistent]
        public string Port { get; set; }

        /// <summary>
        /// Gets or sets the optional hostname header for the web site.
        /// </summary>
        [Persistent]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the option IP address of the web site.
        /// </summary>
        [Persistent]
        public string IPAddress { get; set; }

        public override ActionDescription GetActionDescription()
        {
            return new ActionDescription(
                new ShortActionDescription(
                    "Create ",
                    new Hilite(this.Name),
                    " IIS Web Site"
                ),
                new LongActionDescription(
                    "at ",
                    new Hilite(this.PhysicalPath),
                    "using the ",
                    new Hilite(this.ApplicationPool),
                    " application pool on port ",
                    new Hilite(this.Port)
                )
            );
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            this.LogDebug("Physical Path: {0}", this.PhysicalPath);
            this.LogDebug("App Pool: {0}", this.ApplicationPool);

            int port = string.IsNullOrEmpty(this.Port) ? 80 : InedoLib.Util.Int.ParseZ(this.Port);
            if (port < 1 || port > ushort.MaxValue)
            {
                this.LogError("The specified port ({0}) does not resolve to a valid port number.", this.Port);
                return Domains.YN.No;
            }

            var bindingInfo = new IISUtil.BindingInfo(this.HostName, port, this.IPAddress);
            this.LogDebug("Binding Info (IP:Port:Hostname): " + bindingInfo);

            IISUtil.Instance.CreateWebSite(
                this.Name, 
                this.PhysicalPath, 
                this.ApplicationPool, 
                port == 443, 
                bindingInfo
            );

            return Domains.YN.Yes;
        }

        protected override void Execute()
        {
            this.LogDebug("Creating IIS web site {0}...", this.Name);
            if (this.ExecuteRemoteCommand(null) == Domains.YN.Yes)
                this.LogInformation("{0} web site created.", this.Name);
        }
    }
}
