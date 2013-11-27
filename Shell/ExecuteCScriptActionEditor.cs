using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    internal sealed class ExecuteCScriptActionEditor : ActionEditorBase
    {
        private SourceControlFileFolderPicker ctlScriptPath;
        private ValidatingTextBox txtArguments;

        public override bool DisplaySourceDirectory { get { return true; } }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var execCScript = (ExecuteCScriptAction)extension;
            this.ctlScriptPath.Text = execCScript.ScriptPath;
            this.txtArguments.Text = execCScript.Arguments;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new ExecuteCScriptAction
            {
                ScriptPath = this.ctlScriptPath.Text,
                Arguments = this.txtArguments.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.ctlScriptPath = new SourceControlFileFolderPicker
            {
                ServerId = this.ServerId,
                Required = true
            };

            this.txtArguments = new ValidatingTextBox { Width = 300 };

            this.Controls.Add(
                new FormFieldGroup(
                    "Script Path",
                    "The path to a script that cscript.exe should run (generally .vbs or .js).",
                    false,
                    new StandardFormField(
                        "Script Path:",
                        this.ctlScriptPath
                    )
                ),
                new FormFieldGroup(
                    "CScript.exe Arguments",
                    "Arguments to pass to CScript.",
                    true,
                    new StandardFormField(
                        "Arguments:",
                        this.txtArguments
                    )
                )
            );
        }
    }
}
