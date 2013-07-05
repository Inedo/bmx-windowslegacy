using System.Web.UI.WebControls;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Linq;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    internal sealed class StartStopIISAppActionEditor<TAction> : ActionEditorBase
        where TAction : ActionBase, IIISAppPoolAction, new()
    {
        private DropDownList ddlAppPool;
        private TextBox txtAppPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartStopIISAppActionEditor{TAction}"/> class.
        /// </summary>
        public StartStopIISAppActionEditor()
        {
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (TAction)extension;
            if (!string.IsNullOrEmpty(action.AppPool))
            {
                if (this.ddlAppPool.Items.Cast<ListItem>().Any(a => a.Value == action.AppPool))
                {
                    this.ddlAppPool.SelectedValue = action.AppPool;
                }
                else
                {
                    this.ddlAppPool.SelectedValue = "<|OTHER|>";
                    this.txtAppPool.Text = action.AppPool;
                }
            }
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new TAction
            {
                AppPool = this.ddlAppPool.SelectedValue != "<|OTHER|>" ? this.ddlAppPool.SelectedValue : this.txtAppPool.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtAppPool = new TextBox { Width = 300 };
            string[] names;
            try
            {
                var proxy = new ProxiedUtil((IPersistedObjectExecuter)Util.Agents.CreateAgentFromId(this.ServerId));
                names = proxy.GetAppPoolNames();
            }
            catch
            {
                names = new string[0];
            }

            this.ddlAppPool = new DropDownList { ID = "ddlAppPool" };
            this.ddlAppPool
                .Items
                .AddRange(names.Select(n => new ListItem(n, n)).ToArray());

            this.ddlAppPool.Items.Add(new ListItem("Other...", "<|OTHER|>"));

            var ctlOtherAppPool = new StandardFormField(string.Empty, this.txtAppPool) { ID = "ctlOtherAppPool" };

            this.Controls.Add(
                new FormFieldGroup(
                    "Application Pool",
                    "Select the name of the application pool.",
                    true,
                    new StandardFormField(
                        "Application Pool:",
                        this.ddlAppPool
                    ),
                    ctlOtherAppPool
                ),
                new RenderJQueryDocReadyDelegator(
                    w =>
                    {
                        w.Write("$('#{0}').change(function() {{ if($('#{0}').val() == '<|OTHER|>') $('#{1}').show(); else $('#{1}').hide(); }}); $('#{0}').change();",
                            this.ddlAppPool.ClientID,
                            ctlOtherAppPool.ClientID
                        );
                    }
                )
            );
        }
    }
}
