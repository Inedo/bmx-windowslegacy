using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    /// <summary>
    /// Custom editor for the <see cref="StartServiceAction"/> class.
    /// </summary>
    internal sealed class StartServiceActionEditor : ActionEditorBase
    {
        private DropDownList ddlServices;
        private TextBox txtArgs;
        private CheckBox chkWaitForStart;
        private CheckBox chkIgnoreAlreadyStartedError;
        private CheckBox chkTreatStartErrorsAsWarnings;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartServiceActionEditor"/> class.
        /// </summary>
        public StartServiceActionEditor()
        {
            this.ValidateBeforeCreate += this.StartServiceActionEditor_ValidateBeforeCreate;
            this.ValidateBeforeSave += this.StartServiceActionEditor_ValidateBeforeSave;
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var ssa = (StartServiceAction)extension;
            this.ddlServices.SelectedValue = ssa.ServiceName;
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
                ServiceName = this.ddlServices.SelectedValue,
                StartupArgs = this.txtArgs.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                WaitForStart = this.chkWaitForStart.Checked,
                IgnoreAlreadyStartedError = this.chkIgnoreAlreadyStartedError.Checked,
                TreatUnableToStartAsWarning = chkTreatStartErrorsAsWarnings.Checked
            };
        }

        protected override void CreateChildControls()
        {
            this.ddlServices = new DropDownList { ID = "ddlServices" };
            this.ddlServices.Items.Add(new ListItem { Text = "(select)", Value = string.Empty });
            foreach (var name in this.GetServiceNames())
                this.ddlServices.Items.Add(new ListItem { Text = name, Value = name });

            this.txtArgs = new TextBox
            {
                TextMode = TextBoxMode.MultiLine,
                Rows = 4,
                Width = 250
            };

            this.chkWaitForStart = new CheckBox
            {
                Text = "Wait until the service starts",
                Checked = true
            };

            this.chkIgnoreAlreadyStartedError = new CheckBox
            {
                Text = "Do not generate error if service is already running",
                Checked = false
            };

            this.chkTreatStartErrorsAsWarnings = new CheckBox
            {
                Text = "Treat \"unable to start service\" condition as a warning",
                Checked = false
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Service Options",
                    "Startup arguments are optional, and are entered one per line.",
                    false,
                    new StandardFormField("Service:", this.ddlServices),
                    new StandardFormField("Startup Arguments:", this.txtArgs)
                ),
                new FormFieldGroup(
                    "Wait for Start",
                    "Check this box if you would like this action to verify that the service has started before continuing.",
                    false,
                    new StandardFormField(string.Empty, this.chkWaitForStart)
                ),
                new FormFieldGroup(
                    "Ignore Error if already Running",
                    "Uncheck this box if you wish to generate an error if the service is running when this action is executed.",
                    false,
                    new StandardFormField(string.Empty, this.chkIgnoreAlreadyStartedError)
                ),
                new FormFieldGroup(
                    "Unable to Start Service",
                    "Check this box if you want to log a warning instead of an error when a service fails to start.",
                    true,
                    new StandardFormField(string.Empty, this.chkTreatStartErrorsAsWarnings)
                )
            );
        }

        private string[] GetServiceNames()
        {
            try
            {
                using (var agent = Util.Agents.CreateAgentFromId(this.ServerId))
                {
                    var remote = agent.GetService<IRemoteMethodExecuter>();
                    return remote.InvokeFunc(ServicesHelper.GetServiceNames);
                }
            }
            catch
            {
                return new string[0];
            }
        }
        private void StartServiceActionEditor_ValidateBeforeCreate(object sender, ValidationEventArgs<ActionBase> e)
        {
            using (var agent = Util.Agents.CreateAgentFromId(this.ServerId))
            {
                var remote = agent.GetService<IRemoteMethodExecuter>();
                if (!remote.InvokeFunc(ServicesHelper.IsAvailable))
                {
                    e.Message =
                        "This action may not be created on the specified server because a " +
                        "connection cannot be established with the server's services.";
                    e.ValidLevel = ValidationLevel.Error;
                }
            }
        }
        private void StartServiceActionEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ActionBase> e)
        {
            if (string.IsNullOrEmpty(((StartServiceAction)e.Extension).ServiceName))
            {
                e.ValidLevel = ValidationLevel.Error;
                e.Message = "Service Name must be specified.";
            }
        }
    }
}
