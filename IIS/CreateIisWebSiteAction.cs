using System;
using Inedo.BuildMaster;
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
                "Create a new IIS web site \"{0}\" at the path \"{1}\" under the \"{2}\" application pool on port {3}.",
                this.Name,
                this.PhysicalPath,
                this.ApplicationPool,
                this.Port
            );
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            this.LogInformation("Creating Web Site in IIS: \"{0}\"", this.Name);
            this.LogDebug("Physical Path: {0}", this.PhysicalPath);
            this.LogDebug("App Pool: {0}", this.ApplicationPool);

            int? port = string.IsNullOrEmpty(this.Port) ? 80 : InedoLib.Util.Int.ParseN(this.Port);
            if (port == null || port < 1)
                throw new InvalidOperationException(string.Format("The specified port ({0}) does not resolve to an integer greater than 0.", port));

            this.LogDebug("Binding Info (IP:Port:Hostname): {0}", new IISUtil.BindingInfo(this.HostName, (int)port, this.IPAddress));

            IISUtil.Instance.CreateWebSite(
                this.Name, 
                this.PhysicalPath, 
                this.ApplicationPool, 
                (int)port == 443, 
                new IISUtil.BindingInfo(this.HostName, (int)port, this.IPAddress)
            );

            return null;
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }
    }
}
