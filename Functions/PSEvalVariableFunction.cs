using System;
using System.ComponentModel;
using System.Linq;
using Inedo.Agents;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMaster.Extensibility.VariableFunctions;
using Inedo.BuildMasterExtensions.Windows.PowerShell;
using Inedo.Documentation;
using Inedo.ExecutionEngine;

namespace Inedo.BuildMasterExtensions.Windows.Functions
{
    [ScriptAlias("PSEval")]
    [Description("Returns the result of a PowerShell script.")]
    [Tag("PowerShell")]
    [Example(@"
# set the $NextYear variable to the value of... next year
set $PowershellScript = >>
(Get-Date).year + 1
>>;

set $NextYear = $PSEval($PowershellScript);

Log-Information $NextYear;
")]
    [VariableFunctionProperties(Category = "PowerShell")]
    public sealed class PSEvalVariableFunction : VariableFunctionBase
    {
        [DisplayName("script")]
        [VariableFunctionParameter(0)]
        [Description("The PowerShell script to execute. This should be an expression.")]
        public string ScriptText { get; set; }

        public override RuntimeValue Evaluate(IGenericBuildMasterContext context)
        {
            var execContext = context as IOperationExecutionContext;
            if (execContext == null)
                throw new NotSupportedException("This function can currently only be used within an execution.");

            var job = new ExecutePowerShellJob
            {
                CollectOutput = true,
                ScriptText = this.ScriptText,
                Variables = PowerShellScriptRunner.ExtractVariables(this.ScriptText, execContext)
            };

            var jobExecuter = execContext.Agent.GetService<IRemoteJobExecuter>();
            var result = (ExecutePowerShellJob.Result)jobExecuter.ExecuteJobAsync(job, execContext.CancellationToken).Result();

            if (result.Output.Count == 1)
                return result.Output[0];
            else
                return new RuntimeValue(result.Output.Select(o => new RuntimeValue(o)));
        }
    }
}
