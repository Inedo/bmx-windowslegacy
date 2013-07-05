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
    /// Custom editor for the <see cref="StopServiceAction"/> class.
    /// </summary>
    internal sealed class StopServiceActionEditor : ActionEditorBase
    {
        private DropDownList ddlServices;
        private CheckBox chkWaitForStop;
        private CheckBox chkIgnoreAlreadyStoppedError;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopServiceActionEditor"/> class.
        /// </summary>
        public StopServiceActionEditor()
        {
            this.ValidateBeforeCreate += this.StopServiceActionEditor_ValidateBeforeCreate;
            this.ValidateBeforeSave += this.StopServiceActionEditor_ValidateBeforeSave;
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var ssa = (StopServiceAction)extension;
            this.ddlServices.SelectedValue = ssa.ServiceName;
            this.chkWaitForStop.Checked = ssa.WaitForStop;
            this.chkIgnoreAlreadyStoppedError.Checked = ssa.IgnoreAlreadyStoppedError;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new StopServiceAction
            {
                ServiceName = this.ddlServices.SelectedValue,
                WaitForStop = this.chkWaitForStop.Checked,
                IgnoreAlreadyStoppedError = this.chkIgnoreAlreadyStoppedError.Checked
            };
        }

        protected override void CreateChildControls()
        {
            this.ddlServices = new DropDownList { ID = "ddlServices" };
            this.ddlServices.Items.Add(new ListItem { Text = "(select)", Value = string.Empty });
            foreach (var name in this.GetServiceNames())
                this.ddlServices.Items.Add(new ListItem { Text = name, Value = name });

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
                new FormFieldGroup(
                    "Service",
                    "Specify the name of the service to stop.",
                    false,
                    new StandardFormField("Service:", this.ddlServices)
                ),
                new FormFieldGroup(
                    "Wait for Stop",
                    "Check this box if you would like this action to verify that the service has stopped before continuing.",
                    false,
                    new StandardFormField(string.Empty, this.chkWaitForStop)
                ),
                new FormFieldGroup(
                    "Ignore Error if already Stopped",
                    "Uncheck this box if you wish to generate an error if the service was stopped prior to executing this action.",
                    true,
                    new StandardFormField(string.Empty, this.chkIgnoreAlreadyStoppedError)
                )
            );
        }

        private string[] GetServiceNames()
        {
            return new ServicesHelper((IPersistedObjectExecuter)Util.Agents.CreateAgentFromId(this.ServerId)).GetServiceNames();
        }
        private void StopServiceActionEditor_ValidateBeforeCreate(object sender, ValidationEventArgs<ActionBase> e)
        {
            ServicesHelper sc;
            try
            {
                sc = new ServicesHelper((IPersistedObjectExecuter)Util.Agents.CreateAgentFromId(this.ServerId));
            }
            catch
            {
                e.Message =
                    "There was an error communicating with the selected server.";
                e.ValidLevel = ValidationLevels.Error;
                return;
            }

            if (!sc.IsAvailable())
            {
                e.Message =
                    "This action may not be created on the specified server because a " +
                    "connection cannot be established with the server's services.";
                e.ValidLevel = ValidationLevels.Error;
            }
        }
        private void StopServiceActionEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ActionBase> e)
        {
            if (string.IsNullOrEmpty(((StopServiceAction)e.Extension).ServiceName))
            {
                e.ValidLevel = ValidationLevels.Error;
                e.Message = "Service Name must be specified.";
            }
        }
    }
}
