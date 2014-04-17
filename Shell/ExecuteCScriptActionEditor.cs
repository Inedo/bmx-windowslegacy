using Inedo.BuildMaster;
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

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var execCScript = (ExecuteCScriptAction)extension;
            this.ctlScriptPath.Text = Util.Path2.Combine(execCScript.OverriddenSourceDirectory ?? string.Empty, execCScript.ScriptPath ?? string.Empty);
            this.txtArguments.Text = execCScript.Arguments;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new ExecuteCScriptAction
            {
                OverriddenSourceDirectory = Util.Path2.GetDirectoryName(this.ctlScriptPath.Text),
                ScriptPath = Util.Path2.GetFileName(this.ctlScriptPath.Text),
                Arguments = this.txtArguments.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.ctlScriptPath = new SourceControlFileFolderPicker { Required = true };

            this.txtArguments = new ValidatingTextBox();

            this.Controls.Add(
                new SlimFormField("Script file path:", this.ctlScriptPath),
                new SlimFormField("CScript arguments:", this.txtArguments)
            );
        }
    }
}
