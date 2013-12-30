using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    public sealed class CreateIisWebSiteActionEditor : ActionEditorBase
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
            this.txtName = new ValidatingTextBox() { Width = 300, Required = true };
            this.txtPhysicalPath = new ValidatingTextBox() { Width = 300, Required = true };
            this.txtApplicationPoolName = new ValidatingTextBox() { Width = 300, Required = true };
            this.txtPort = new ValidatingTextBox() { Width = 125, DefaultText = "80" };
            this.txtHostName = new ValidatingTextBox() { Width = 300, DefaultText = "any" };
            this.txtIPAddress = new ValidatingTextBox() { Width = 300, DefaultText = "All unassigned" };

            this.Controls.Add(
                new FormFieldGroup(
                    "Name",
                    "The name of the web site to create on the remote server.",
                    false,
                    new StandardFormField("Name:", this.txtName)
                ),
                new FormFieldGroup(
                    "Physical Path",
                    "The physical path of the web site on the remote server.",
                    false,
                    new StandardFormField("Physical Path:", this.txtPhysicalPath)
                ),
                new FormFieldGroup(
                    "Application Pool",
                    "The name of the application pool that will host the web site.",
                    false,
                    new StandardFormField("Application Pool:", this.txtApplicationPoolName)
                ),
                new FormFieldGroup(
                    "Binding Information",
                    "Specify the port, and optionally, the hostname and IP address of the web site.",
                    false,
                    new StandardFormField("Port:", this.txtPort),
                    new StandardFormField("Host Name:", this.txtHostName),
                    new StandardFormField("IP Address:", this.txtIPAddress)
                )
            );
        }
    }
}
