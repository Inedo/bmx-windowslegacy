using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    internal sealed class StartStopIISAppActionEditor<TAction> : ActionEditorBase
        where TAction : ActionBase, IIISAppPoolAction, new()
    {
        private AppPoolSelector ddlAppPool;

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (TAction)extension;
            this.ddlAppPool.Value = action.AppPool;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new TAction
            {
                AppPool = this.ddlAppPool.Value
            };
        }

        protected override void CreateChildControls()
        {
            this.ddlAppPool = new AppPoolSelector { ID = "ddlAppPool" };

            var ctlValidator = new StyledCustomValidator();
            ctlValidator.ServerValidate +=
                (s, e) =>
                {
                    e.IsValid = !string.IsNullOrWhiteSpace(this.ddlAppPool.Value);
                };

            this.Controls.Add(
                new SlimFormField("Application pool:", this.ddlAppPool, ctlValidator)
            );
        }
    }
}
