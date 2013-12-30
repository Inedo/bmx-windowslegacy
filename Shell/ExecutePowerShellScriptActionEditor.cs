using System;
using System.IO;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    /// <summary>
    /// Custom editor for the execute PowerShell script action.
    /// </summary>
    internal sealed class ExecutePowerShellScriptActionEditor : ActionEditorBase
    {
        private CheckBox chkUseScriptFile;
        private SourceControlFileFolderPicker txtScriptFile;
        private TextBox txtScript;
        private TextBox txtVariables;
        private TextBox txtParameters;
        private CheckBox chkLogResults;
        private StandardFormField sffScriptFile;
        private StandardFormField sffScript;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutePowerShellScriptActionEditor"/> class.
        /// </summary>
        public ExecutePowerShellScriptActionEditor()
        {
            this.ValidateBeforeSave += this.ExecutePowerShellScriptActionEditor_ValidateBeforeSave;
        }

        public override void BindToForm(ActionBase extension)
        {
            EnsureChildControls();

            var action = (ExecutePowerShellScriptAction)extension;
            this.chkUseScriptFile.Checked = action.IsScriptFile;
            this.chkLogResults.Checked = action.LogResults;
            this.txtVariables.Text = string.Join(Environment.NewLine, action.Variables ?? new string[0]);
            this.txtParameters.Text = string.Join(Environment.NewLine, action.Parameters ?? new string[0]);

            if (action.IsScriptFile)
                this.txtScriptFile.Text = Path.Combine(action.OverriddenSourceDirectory, action.Script ?? string.Empty);
            else
                this.txtScript.Text = action.Script ?? string.Empty;
        }
        public override ActionBase CreateFromForm()
        {
            EnsureChildControls();

            var action = new ExecutePowerShellScriptAction()
            {
                IsScriptFile = this.chkUseScriptFile.Checked,
                LogResults = this.chkLogResults.Checked,
                Variables = this.txtVariables.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
                Parameters = this.txtParameters.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            };

            if (this.chkUseScriptFile.Checked)
            {
                action.OverriddenSourceDirectory = Path.GetDirectoryName(this.txtScriptFile.Text);
                action.Script = Path.GetFileName(this.txtScriptFile.Text);
            }
            else
            {
                action.Script = this.txtScript.Text;
            }

            return action;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.chkUseScriptFile = new CheckBox()
            {
                Text = "Read from File"
            };

            this.txtScriptFile = new SourceControlFileFolderPicker()
            {
                DisplayMode = SourceControlBrowser.DisplayModes.FoldersAndFiles,
                ServerId = this.ServerId
            };

            this.txtScript = new TextBox()
            {
                Width = 300,
                TextMode = TextBoxMode.MultiLine,
                Rows = 10
            };

            this.txtVariables = new TextBox()
            {
                Width = 300,
                TextMode = TextBoxMode.MultiLine,
                Rows = 5
            };

            this.txtParameters = new TextBox()
            {
                Width = 300,
                TextMode = TextBoxMode.MultiLine,
                Rows = 5
            };

            this.chkLogResults = new CheckBox()
            {
                Text = "Log Script Output",
                Checked = true
            };

            this.sffScriptFile = new StandardFormField("Script File:", this.txtScriptFile);

            this.sffScript = new StandardFormField("Script:", this.txtScript);

            this.Controls.Add(
                new FormFieldGroup(
                    "Script",
                    "Provide a script to execute either as a file on the remote server or entered directly into the action editor.",
                    false,
                    new StandardFormField(
                        string.Empty,
                        this.chkUseScriptFile
                        ),
                    this.sffScriptFile,
                    this.sffScript
                    ),
                new FormFieldGroup(
                    "Parameters",
                    "Optionally provide arguments declared in a Param() block to pass to the script in the form ParamName=ArgumentValue (one per line). " +
                    "Switch parameters can be included by their name only (e.g. SwitchName) to evaluate to $true, or omitted to evaluate to $false. ",
                    false,
                    new StandardFormField(
                        "Parameters (one per line):",
                        this.txtParameters
                        )
                    ),
                new FormFieldGroup(
                    "Variables",
                    "Optionally provide global variables to replace within the script in the form VariableName=VariableValue (one per line). " +
                    "To access custom BuildMaster variables, use surrounding percent symbols. (e.g. MyVariable=%MyCustomVariable%)",
                    false,
                    new StandardFormField(
                        "Variables (one per line):",
                        this.txtVariables
                        )
                    ),
                new FormFieldGroup(
                    "Options",
                    "Additional configuration options for running the PowerShell script.",
                    true,
                    new StandardFormField(
                        string.Empty,
                        this.chkLogResults
                        )
                    ),
                    new RenderJQueryDocReadyDelegator(w =>
                    {
                        w.WriteLine(
                         @"var onFileCheckChange = function(){
                           if($('#" + this.chkUseScriptFile.ClientID + @"').is(':checked')) {
                              $('#" + this.sffScriptFile.ClientID + @"').show();
                              $('#" + this.sffScript.ClientID + @"').hide();
                           }
                           else {
                              $('#" + this.sffScriptFile.ClientID + @"').hide();
                              $('#" + this.sffScript.ClientID + @"').show();
                           }
                           };
                           onFileCheckChange();
                           $('#" + this.chkUseScriptFile.ClientID + @"').change(onFileCheckChange);");
                    })

                );
        }

        private void ExecutePowerShellScriptActionEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ActionBase> e)
        {
            if (this.chkUseScriptFile.Checked)
            {
                if (string.IsNullOrEmpty(this.txtScriptFile.Text))
                {
                    e.ValidLevel = ValidationLevel.Error;
                    e.Message = "Script file to execute must be specified.";
                }
            }
        }
    }
}
