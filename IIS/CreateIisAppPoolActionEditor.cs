﻿using System;
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
    public sealed class CreateIisAppPoolActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtName;
        private DropDownList ddlUser;
        private ValidatingTextBox txtUser;
        private PasswordTextBox txtPassword;
        private Div divUser;
        private RadioButtonList rblIntegrated;
        private DropDownList ddlManagedRuntimeVersion;

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateIisAppPoolAction)extension;

            this.txtName.Text = action.Name;
            if (new[] { "LocalSystem", "LocalService", "NetworkService" }.Contains(action.User, StringComparer.OrdinalIgnoreCase))
            {
                this.ddlUser.SelectedValue = action.User;
            }
            else
            {
                this.ddlUser.SelectedValue = "custom";
                this.txtUser.Text = action.User;
                this.txtPassword.Text = action.Password;
            }

            this.rblIntegrated.SelectedValue = action.IntegratedMode.ToString().ToLower();
            this.ddlManagedRuntimeVersion.SelectedValue = action.ManagedRuntimeVersion;
        }

        public override ActionBase CreateFromForm()
        {
            return new CreateIisAppPoolAction()
            {
                Name = this.txtName.Text,
                User = this.ddlUser.SelectedValue == "custom" ? this.txtUser.Text : this.ddlUser.SelectedValue,
                Password = this.ddlUser.SelectedValue == "custom" ? this.txtPassword.Text : "",
                IntegratedMode = bool.Parse(this.rblIntegrated.SelectedValue),
                ManagedRuntimeVersion = this.ddlManagedRuntimeVersion.SelectedValue
            };
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.Controls.Add(GetClientSideScript(this.ddlUser.ClientID, this.divUser.ClientID));

            base.OnPreRender(e);
        }

        protected override void CreateChildControls()
        {
            this.txtName = new ValidatingTextBox() { Width = 300, Required = true };
            this.ddlUser = new DropDownList()
            {
                Items =
                {
                    new ListItem("Local System", "LocalSystem"),
                    new ListItem("Local Service", "LocalService"),
                    new ListItem("Network Service", "NetworkService"),
                    new ListItem("Custom...", "custom")
                }
            };

            this.txtUser = new ValidatingTextBox() { Width = 300 };
            this.txtPassword = new PasswordTextBox() { Width = 270 };

            this.divUser = new Div() 
            { 
                Controls = 
                { 
                    new LiteralControl("<br />"),
                    new StandardFormField("User Name:", this.txtUser), 
                    new StandardFormField("Password:", this.txtPassword)
                } 
            };

            this.rblIntegrated = new RadioButtonList()
            {
                Items = 
                { 
                    new ListItem("Integrated Mode", "true"),
                    new ListItem("Classic Mode", "false") 
                }
            };

            this.ddlManagedRuntimeVersion = new DropDownList()
            {
                Items =
                {
                    new ListItem("v4.0"),
                    new ListItem("v2.0")
                }
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Name",
                    "The name of the application pool to create on the remote server.",
                    false,
                    new StandardFormField("Name:", this.txtName)
                ),
                new FormFieldGroup(
                    "User Identity",
                    "The user account that hosts the application pool.",
                    false,
                    new StandardFormField("User Identity:", this.ddlUser, this.divUser)
                ),
                new FormFieldGroup(
                    "Managed Pipeline Mode",
                    "Specify whether the application pool should run in Integrated Mode or Classic Mode.",
                    false,
                    new StandardFormField("", this.rblIntegrated)
                ),
                new FormFieldGroup(
                    "Runtime Version",
                    "Specify the version of .NET used by this application pool.",
                    false,
                    new StandardFormField("Runtime Version:", this.ddlManagedRuntimeVersion)
                )
            );
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