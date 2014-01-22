using System;
using System.Collections.Generic;
using System.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Actions.Scripting;
using Inedo.BuildMaster.Web;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.BuildMasterExtensions.Windows.Scripting.PowerShell;

namespace Inedo.BuildMasterExtensions.Windows.Shell
{
    [ActionProperties(
        "Execute PowerShell Script",
        "Runs a PowerShell script on the target server.")]
    [Tag("windows")]
    [CustomEditor(typeof(ExecuteScriptActionEditor<PowerShellScriptType, ExecutePowerShellScriptAction>))]
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

        private static IDictionary<string, string> BuildDictionary(string values)
        {
            return Util.Persistence.DeserializeToStringArray(values)
                .Select(s => s.Split(new[] { '=' }, 2))
                .Where(p => p.Length == 2)
                .Distinct(p => p[0], StringComparer.OrdinalIgnoreCase)
                .ToDictionary(p => p[0], p => p[1], StringComparer.OrdinalIgnoreCase);
        }
    }
}
