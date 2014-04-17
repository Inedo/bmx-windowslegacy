using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    internal sealed class UninstallServiceActionEditor : ActionEditorBase
    {
        private ServiceSelector ddlServices;
        private CheckBox chkErrorIfNotInstalled;
        private CheckBox chkStopIfRunning;

        public override void BindToForm(ActionBase extension)
        {
            var action = (UninstallServiceAction)extension;
            this.ddlServices.Value = action.ServiceName;
            this.chkErrorIfNotInstalled.Checked = action.ErrorIfNotInstalled;
            this.chkStopIfRunning.Checked = action.StopIfRunning;
        }
        public override ActionBase CreateFromForm()
        {
            return new UninstallServiceAction
            {
                ServiceName = this.ddlServices.Value,
                ErrorIfNotInstalled = this.chkErrorIfNotInstalled.Checked,
                StopIfRunning = this.chkStopIfRunning.Checked
            };
        }

        protected override void CreateChildControls()
        {
            this.ddlServices = new ServiceSelector { ID = "ddlServices" };

            var ctlValidator = new StyledCustomValidator();
            ctlValidator.ServerValidate +=
                (s, e) =>
                {
                    e.IsValid = !string.IsNullOrWhiteSpace(this.ddlServices.Value);
                };

            this.chkErrorIfNotInstalled = new CheckBox
            {
                Text = "Log error if service is not found"
            };

            this.chkStopIfRunning = new CheckBox
            {
                Text = "Stop service if it is running"
            };

            this.Controls.Add(
                new SlimFormField("Service:", this.ddlServices, ctlValidator),
                new SlimFormField(
                    "Options:",
                    new Div(this.chkErrorIfNotInstalled),
                    new Div(this.chkStopIfRunning)
                )
            );
        }
    }
}