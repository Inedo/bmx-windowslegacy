using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    /// <summary>
    /// Custom editor for the <see cref="CreateIisAppPoolAction"/> class.
    /// </summary>
    internal sealed class CreateIisAppPoolActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtName;
        private DropDownList ddlUser;
        private ValidatingTextBox txtUser;
        private PasswordTextBox txtPassword;
        private Div divUser;
        private RadioButtonList rblIntegrated;
        private RadioButtonList rdbOmitActionIfPoolExists;
        private DropDownList ddlManagedRuntimeVersion;

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateIisAppPoolAction) extension;

            txtName.Text = action.Name;
            if (new[] {"LocalSystem", "LocalService", "NetworkService", "ApplicationPoolIdentity"}.Contains(
                action.User, StringComparer.OrdinalIgnoreCase))
            {
                ddlUser.SelectedValue = action.User;
            }
            else
            {
                ddlUser.SelectedValue = "custom";
                txtUser.Text = action.User;
                txtPassword.Text = action.Password;
            }

            rblIntegrated.SelectedValue = action.IntegratedMode.ToString().ToLower();
            ddlManagedRuntimeVersion.SelectedValue = action.ManagedRuntimeVersion;
            rdbOmitActionIfPoolExists.SelectedValue = action.OmitActionIfPoolExists.ToString().ToLower();
        }

        public override ActionBase CreateFromForm()
        {
            return new CreateIisAppPoolAction()
            {
                Name = txtName.Text,
                User = ddlUser.SelectedValue == "custom" ? txtUser.Text : ddlUser.SelectedValue,
                Password = ddlUser.SelectedValue == "custom" ? txtPassword.Text : "",
                IntegratedMode = bool.Parse(rblIntegrated.SelectedValue),
                ManagedRuntimeVersion = ddlManagedRuntimeVersion.SelectedValue
            };
        }

        protected override void OnPreRender(EventArgs e)
        {
            Controls.Add(GetClientSideScript(ddlUser.ClientID, divUser.ClientID));

            base.OnPreRender(e);
        }

        protected override void CreateChildControls()
        {
            txtName = new ValidatingTextBox {Required = true};
            ddlUser = new DropDownList
            {
                Items =
                {
                    new ListItem("Local System", "LocalSystem"),
                    new ListItem("Local Service", "LocalService"),
                    new ListItem("Network Service", "NetworkService"),
                    new ListItem("Application Pool Identity", "ApplicationPoolIdentity"),
                    new ListItem("Custom...", "custom")
                }
            };

            txtUser = new ValidatingTextBox();
            txtPassword = new PasswordTextBox();

            divUser = new Div
            {
                Controls =
                {
                    new LiteralControl("<br />"),
                    new StandardFormField("User Name:", txtUser),
                    new StandardFormField("Password:", txtPassword)
                }
            };

            rblIntegrated = new RadioButtonList
            {
                Items =
                {
                    new ListItem("Integrated Mode", "true"),
                    new ListItem("Classic Mode", "false")
                }
            };

            ddlManagedRuntimeVersion = new DropDownList
            {
                Items =
                {
                    new ListItem("v4.0"),
                    new ListItem("v2.0")
                }
            };

            Controls.Add(
                new SlimFormField(
                    "Application pool name:",
                    txtName
                    ),
                new SlimFormField(
                    "User identity:",
                    ddlUser,
                    divUser
                    ),
                new SlimFormField(
                    "Managed pipeline mode:",
                    rblIntegrated
                    ),
                new SlimFormField(
                    "Managed runtime version:",
                    ddlManagedRuntimeVersion
                    )
                );

            rdbOmitActionIfPoolExists = new RadioButtonList
            {
                Items =
                {
                    new ListItem("Omit action if pool already exists", "true"),
                }
            };
        }

        private RenderJQueryDocReadyDelegator GetClientSideScript(string ddlUserId, string divUserId)
        {
            return new RenderJQueryDocReadyDelegator(w =>
                w.Write(
                    "var onload = $('#" + ddlUserId + "').find('option').filter(':selected').val();" +
                    "if(onload == 'custom')" +
                    "{" +
                    "$('#" + divUserId + "').show();" +
                    "}" +
                    "$('#" + ddlUserId + "').change(function () {" +
                    "var selectedConfig = $(this).find('option').filter(':selected').val();" +
                    "if(selectedConfig == 'custom')" +
                    "{" +
                    "$('#" + divUserId + "').show();" +
                    "}" +
                    "else" +
                    "{" +
                    "$('#" + divUserId + "').hide();" +
                    "}" +
                    "}).change();"
                    )
                );
        }
    }
}