using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Documentation;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMaster.Web;
using Inedo.BuildMasterExtensions.Windows.PowerShell;
using Inedo.Diagnostics;
using Inedo.ExecutionEngine;

namespace Inedo.BuildMasterExtensions.Windows.Operations.PowerShell
{
    [DisplayName("PSCall")]
    [Description("Calls a PowerShell Script that is stored as an asset.")]
    [ScriptAlias("PSCall")]
    [Tag("powershell")]
    [ScriptNamespace("PowerShell", PreferUnqualified = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [CustomEditor(typeof(PSCallOperationEditor))]
    [Example(@"
# execute the hdars.ps1 script, passing Argument1 and Aaaaaarg2 as variables, and capturing the value of OutputArg as $MyVariable
pscall hdars (
  Argument1: hello,
  Aaaaaarg2: World,
  OutputArg => $MyVariable
);
")]
    public sealed class PSCallOperation : ExecuteOperation, ICustomArgumentMapper
    {
        public RuntimeValue DefaultArgument { get; set; }
        public IReadOnlyDictionary<string, RuntimeValue> NamedArguments { get; set; }
        public IDictionary<string, RuntimeValue> OutArguments { get; set; }

        public override Task ExecuteAsync(IOperationExecutionContext context)
        {
            if (context.Simulation)
            {
                this.LogInformation("Executing PowerShell Script...");
                return Complete;
            }

            var fullScriptName = this.DefaultArgument.AsString();
            if (fullScriptName == null)
            {
                this.LogError("Bad or missing script name.");
                return Complete;
            }

            return this.ExecuteScriptAsync(
                context: context,
                fullScriptName: fullScriptName,
                arguments: this.NamedArguments,
                outArguments: this.OutArguments,
                collectOutput: false
            );
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.DefaultArgument))
                return new ExtendedRichDescription(new RichDescription("PSCall {error parsing statement}"));

            var defaultArg = config.DefaultArgument;
            var longDesc = new RichDescription();

            bool longDescInclused = false;
            var scriptName = QualifiedName.TryParse(defaultArg);
            if (scriptName != null)
            {
                var info = PowerShellScriptInfo.TryParse(scriptName);
                if (!string.IsNullOrEmpty(info?.Description))
                {
                    longDesc.AppendContent(info.Description);
                    longDescInclused = true;
                }

                var listParams = new List<string>();
                foreach (var prop in config.NamedArguments)
                    listParams.Add($"{prop.Key}: {prop.Value}");

                foreach (var prop in config.OutArguments)
                    listParams.Add($"{prop.Key} => {prop.Value}");

                if (listParams.Count > 0)
                {
                    if (longDescInclused)
                        longDesc.AppendContent(" - ");

                    longDesc.AppendContent(new ListHilite(listParams));
                    longDescInclused = true;
                }
            }

            if (!longDescInclused)
                longDesc.AppendContent("with no parameters");

            return new ExtendedRichDescription(
                new RichDescription("PSCall ", new Hilite(defaultArg)),
                longDesc
            );
        }

        private async Task<ExecutePowerShellJob.Result> ExecuteScriptAsync(IOperationExecutionContext context, string fullScriptName, IReadOnlyDictionary<string, RuntimeValue> arguments, IDictionary<string, RuntimeValue> outArguments, bool collectOutput)
        {
            string scriptName;
            int? applicationId;
            var scriptNameParts = fullScriptName.Split(new[] { "::" }, 2, StringSplitOptions.None);
            if (scriptNameParts.Length == 2)
            {
                applicationId = DB.Applications_GetApplications(null, true).FirstOrDefault(a => string.Equals(a.Application_Name, scriptNameParts[0], StringComparison.OrdinalIgnoreCase))?.Application_Id;
                if (applicationId == null)
                {
                    this.LogError($"Invalid application name {scriptNameParts[0]}.");
                    return null;
                }

                scriptName = scriptNameParts[1];
            }
            else
            {
                applicationId = context.ApplicationId;
                scriptName = scriptNameParts[0];
            }

            string scriptText;

            if (!scriptName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                scriptName += ".ps1";

            var script = DB.ScriptAssets_GetScriptByName(scriptName, applicationId);
            if (script == null)
            {
                this.LogError($"Script {scriptName} not found.");
                return null;
            }

            using (var stream = new MemoryStream(script.Script_Text, false))
            using (var reader = new StreamReader(stream, InedoLib.UTF8Encoding))
            {
                scriptText = reader.ReadToEnd();
            }

            var variables = new Dictionary<string, string>();
            var parameters = new Dictionary<string, string>();

            PowerShellScriptInfo scriptInfo;
            if (PowerShellScriptInfo.TryParse(new StringReader(scriptText), out scriptInfo))
            {
                foreach (var var in arguments)
                {
                    var value = var.Value.AsString() ?? string.Empty;
                    var param = scriptInfo.Parameters.FirstOrDefault(p => string.Equals(p.Name, var.Key, StringComparison.OrdinalIgnoreCase));
                    if (param != null)
                        parameters[param.Name] = value;
                    else
                        variables[var.Key] = value;
                }
            }
            else
            {
                arguments.ToDictionary(v => v.Key, v => v.Value.AsString() ?? string.Empty);
            }

            var jobRunner = context.Agent.GetService<IRemoteJobExecuter>();

            var job = new ExecutePowerShellJob
            {
                ScriptText = scriptText,
                DebugLogging = true,
                VerboseLogging = true,
                CollectOutput = collectOutput,
                LogOutput = !collectOutput,
                Variables = variables,
                Parameters = parameters,
                OutVariables = outArguments.Keys.ToArray()
            };

            job.MessageLogged += (s, e) => this.Log(e.Level, e.Message);

            var result = (ExecutePowerShellJob.Result)await jobRunner.ExecuteJobAsync(job, context.CancellationToken);
            if (result.ExitCode != null)
                this.LogDebug("Script exit code: " + result.ExitCode);

            foreach (var var in result.OutVariables)
                outArguments[var.Key] = var.Value;

            return result;
        }
    }
}
