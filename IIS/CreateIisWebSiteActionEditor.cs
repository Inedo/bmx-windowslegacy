using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    internal sealed class CreateIisWebSiteActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtName;
        private ValidatingTextBox txtPhysicalPath;
        private ValidatingTextBox txtApplicationPoolName;
        private ValidatingTextBox txtPort;
        private ValidatingTextBox txtHostName;
        private ValidatingTextBox txtIPAddress;

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateIisWebSiteAction)extension;

            this.txtName.Text = action.Name;
            this.txtPhysicalPath.Text = action.PhysicalPath;
            this.txtApplicationPoolName.Text = action.ApplicationPool;
            this.txtPort.Text = action.Port;
            this.txtHostName.Text = action.HostName;
            this.txtIPAddress.Text = action.IPAddress;
        }

        public override ActionBase CreateFromForm()
        {
            return new CreateIisWebSiteAction()
            {
                Name = this.txtName.Text,
                PhysicalPath = this.txtPhysicalPath.Text,
                ApplicationPool = this.txtApplicationPoolName.Text,
                Port = this.txtPort.Text,
                HostName = this.txtHostName.Text,
                IPAddress = this.txtIPAddress.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtName = new ValidatingTextBox { Required = true };
            this.txtPhysicalPath = new ValidatingTextBox { Required = true };
            this.txtApplicationPoolName = new ValidatingTextBox() { Required = true };
            this.txtPort = new ValidatingTextBox { DefaultText = "80" };
            this.txtHostName = new ValidatingTextBox { DefaultText = "any" };
            this.txtIPAddress = new ValidatingTextBox { DefaultText = "All unassigned" };

            this.Controls.Add(
                new SlimFormField(
                    "Web site name:",
                    this.txtName
                ),
                new SlimFormField(
                    "Physical path:",
                    this.txtPhysicalPath
                ),
                new SlimFormField(
                    "Application pool:",
                    this.txtApplicationPoolName
                ),
                new SlimFormField(
                    "Port:",
                    this.txtPort
                ),
                new SlimFormField(
                    "Host name:",
                    this.txtHostName
                ),
                new SlimFormField(
                    "IP address:",
                    this.txtIPAddress
                )
            );
        }
    }
}