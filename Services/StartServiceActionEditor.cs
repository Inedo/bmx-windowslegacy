using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.ClientResources;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;
using Inedo.Web.Handlers;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    /// <summary>
    /// Custom editor for the <see cref="StartServiceAction"/> class.
    /// </summary>
    internal sealed class StartServiceActionEditor : ActionEditorBase
    {
        private ServiceSelector ddlServices;
        private TextBox txtArgs;
        private CheckBox chkWaitForStart;
        private CheckBox chkIgnoreAlreadyStartedError;
        private CheckBox chkTreatStartErrorsAsWarnings;

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var ssa = (StartServiceAction)extension;
            this.ddlServices.Value = ssa.ServiceName;
            this.txtArgs.Text = (ssa.StartupArgs == null)
                ? string.Empty
                : string.Join(Environment.NewLine, ssa.StartupArgs);
            this.chkWaitForStart.Checked = ssa.WaitForStart;
            this.chkIgnoreAlreadyStartedError.Checked = ssa.IgnoreAlreadyStartedError;
            this.chkTreatStartErrorsAsWarnings.Checked = ssa.TreatUnableToStartAsWarning;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new StartServiceAction
            {
                ServiceName = this.ddlServices.Value,
                StartupArgs = this.txtArgs.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                WaitForStart = this.chkWaitForStart.Checked,
                IgnoreAlreadyStartedError = this.chkIgnoreAlreadyStartedError.Checked,
                TreatUnableToStartAsWarning = chkTreatStartErrorsAsWarnings.Checked
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

            this.txtArgs = new TextBox
            {
                TextMode = TextBoxMode.MultiLine,
                Rows = 4
            };

            this.chkWaitForStart = new CheckBox
            {
                Text = "Wait until the service starts",
                Checked = true
            };

            this.chkIgnoreAlreadyStartedError = new CheckBox
            {
                Text = "Do not generate an error if service is already running",
                Checked = false
            };

            this.chkTreatStartErrorsAsWarnings = new CheckBox
            {
                Text = "Treat \"unable to start service\" condition as a warning instead of an error",
                Checked = false
            };

            this.Controls.Add(
                new SlimFormField("Service:", this.ddlServices, ctlServiceValidator),
                new SlimFormField("Startup arguments:", this.txtArgs)
                {
                    HelpText = "Enter arguments one per line."
                },
                new SlimFormField(
                    "Options:",
                    new Div(this.chkWaitForStart),
                    new Div(this.chkIgnoreAlreadyStartedError),
                    new Div(this.chkTreatStartErrorsAsWarnings)
                )
            );
        }
    }
}
