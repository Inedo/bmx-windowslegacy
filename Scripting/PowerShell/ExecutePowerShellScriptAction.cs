using System;
using System.Collections.Generic;
using System.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Actions.Scripting;
using Inedo.BuildMaster.Extensibility.Variables;
using Inedo.BuildMaster.Web;
using Inedo.BuildMasterExtensions.Windows.Scripting.PowerShell;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    [ActionProperties(
        "Execute PowerShell Script",
        "Runs a PowerShell script on the target server.")]
    [Tag("windows")]
    [CustomEditor(typeof(ExecutePowerShellScriptActionEditor))]
    public sealed class ExecutePowerShellScriptAction : ExecuteScriptActionBase<PowerShellScriptType>, IMissingPersistentPropertyHandler
    {
        void IMissingPersistentPropertyHandler.OnDeserializedMissingProperties(IDictionary<string, string> missingProperties)
        {
            // Convert old format from before BM 4.1

            var variables = missingProperties.GetValueOrDefault("Variables") ?? string.Empty;
            var parameters = missingProperties.GetValueOrDefault("Parameters") ?? string.Empty;
            var script = missingProperties.GetValueOrDefault("Script");
            bool isScriptFile = bool.Parse(missingProperties.GetValueOrDefault("IsScriptFile", "False"));

            if (isScriptFile)
            {
                this.ScriptMode = ScriptActionMode.FileName;
                this.ScriptFileName = script;
            }
            else
            {
                this.ScriptMode = ScriptActionMode.Direct;
                this.ScriptText = script;
            }

            this.VariableValues = BuildDictionary(variables);
            this.ParameterValues = BuildDictionary(parameters);
        }

        protected override void Execute()
        {
            // Force defaults if specified in configurer
            var configurer = (WindowsExtensionConfigurer)this.GetExtensionConfigurer();
            if (configurer.OverridePowerShellDefaults)
            {
                var parameterData = StoredProcs.Scripts_GetScript(this.ScriptId)
                    .Execute()
                    .ScriptParameters
                    .Where(p => !string.IsNullOrEmpty(p.DefaultValue_Text));

                var application = StoredProcs.Applications_GetApplication(this.Context.ApplicationId)
                    .Execute()
                    .Applications_Extended
                    .FirstOrDefault();

                var evalContext = this.GetVariableEvaluationContext();

                foreach (var parameter in parameterData)
                {
                    if (string.IsNullOrEmpty(this.ParameterValues.GetValueOrDefault(parameter.Parameter_Name)))
                    {
                        try
                        {
                            var tree = VariableExpressionTree.Parse(parameter.DefaultValue_Text, application.VariableSupport_Code);
                            this.ParameterValues[parameter.Parameter_Name] = tree.Evaluate(evalContext);
                        }
                        catch
                        {
                            this.ParameterValues[parameter.Parameter_Name] = parameter.DefaultValue_Text;
                        }
                    }
                }
            }

            base.Execute();
        }

        private static IDictionary<string, string> BuildDictionary(string values)
        {
            return Util.Persistence.DeserializeToStringArray(values)
                .Select(s => s.Split(new[] { '=' }, 2))
                .Where(p => p.Length == 2)
                .Distinct(p => p[0], StringComparer.OrdinalIgnoreCase)
                .ToDictionary(p => p[0], p => p[1], StringComparer.OrdinalIgnoreCase);
        }

        private IVariableEvaluationContext GetVariableEvaluationContext()
        {
            // StandardEvaluationContext should probably be moved to SDK.
            // For now create instance using reflection.

            return (IVariableEvaluationContext)Activator.CreateInstance(
                Type.GetType("Inedo.BuildMaster.Variables.StandardVariableEvaluationContext,BuildMaster", true),
                (IGenericBuildMasterContext)this.Context,
                this.Context.Variables
            );
        }
    }
}
