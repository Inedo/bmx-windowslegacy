using System;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    /// <summary>
    /// Represents an editor for the Execute CScript action
    /// </summary>
    public class ExecuteCScriptActionEditor : OldActionEditorBase
    {
        SourceControlFileFolderPicker ctlScriptPath = null;
        ValidatingTextBox txtArguments = null;

        public override bool DisplaySourceDirectory { get { return true; } }

        protected override void OnInit(EventArgs e)
        {
            ctlScriptPath = new SourceControlFileFolderPicker();
            ctlScriptPath.ServerId = ServerId;
            ctlScriptPath.Required = true;

            txtArguments = new ValidatingTextBox() { Width = 300 };

            CUtil.Add(
                this,
                new FormFieldGroup(
                    "Script Path",
                    "The path to a script that cscript.exe should run (generally .vbs or .js)",
                    false,
                    new StandardFormField(
                        "Script Path:",
                        ctlScriptPath
                    )
                ),
                new FormFieldGroup(
                    "CScript.exe Arguments",
                    "Arguments to pass to CScript.",
                    true,
                    new StandardFormField(
                        "Arguments:",
                        txtArguments
                    )
                )
            );


            base.OnInit(e);
        }

        public override void BindActionToForm(ActionBase action)
        {
            ExecuteCScriptAction execCScript = (ExecuteCScriptAction)action;

            ctlScriptPath.Text = execCScript.ScriptPath;
            txtArguments.Text = execCScript.Arguments;
        }

        public override ActionBase CreateActionFromForm()
        {
            ExecuteCScriptAction execCScript = new ExecuteCScriptAction();

            execCScript.ScriptPath = ctlScriptPath.Text;
            execCScript.Arguments = txtArguments.Text;

            return execCScript;
        }
    }
}
