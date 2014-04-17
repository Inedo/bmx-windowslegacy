using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    /// <summary>
    /// Custom editor for the <see cref="StopServiceAction"/> class.
    /// </summary>
    internal sealed class StopServiceActionEditor : ActionEditorBase
    {
        private ServiceSelector ddlServices;
        private CheckBox chkWaitForStop;
        private CheckBox chkIgnoreAlreadyStoppedError;

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var ssa = (StopServiceAction)extension;
            this.ddlServices.Value = ssa.ServiceName;
            this.chkWaitForStop.Checked = ssa.WaitForStop;
            this.chkIgnoreAlreadyStoppedError.Checked = ssa.IgnoreAlreadyStoppedError;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new StopServiceAction
            {
                ServiceName = this.ddlServices.Value,
                WaitForStop = this.chkWaitForStop.Checked,
                IgnoreAlreadyStoppedError = this.chkIgnoreAlreadyStoppedError.Checked
            };
        }

        protected override void CreateChildControls()
        {
            this.ddlServices = new ServiceSelector { ID = "ddlServices" };

            var ctlServiceValidator = new StyledCustomValidator();
            ctlServiceValidator.ServerValidate +=
                (s, e) =>
                {
                    e.IsValid = !string.IsNullOrWhiteSpace(this.ddlServices.Value);
                };

            this.chkWaitForStop = new CheckBox
            {
                Text = "Wait until the service stops",
                Checked = true
            };

            this.chkIgnoreAlreadyStoppedError = new CheckBox
            {
                Text = "Do not generate error if service is already stopped",
                Checked = true
            };

            this.Controls.Add(
                new SlimFormField("Service:", this.ddlServices, ctlServiceValidator),
                new SlimFormField(
                    "Options:",
                    new Div(this.chkWaitForStop),
                    new Div(this.chkIgnoreAlreadyStoppedError)
                )
            );
        }
    }
}
