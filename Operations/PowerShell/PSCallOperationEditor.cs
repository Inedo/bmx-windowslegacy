using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Web.Controls.Plans;
using Inedo.BuildMasterExtensions.Windows.PowerShell;
using Inedo.ExecutionEngine;
using Inedo.Web.Controls;
using Inedo.Web.DP;

namespace Inedo.BuildMasterExtensions.Windows.Operations.PowerShell
{
    internal sealed class PSCallOperationEditor : OperationEditor
    {
        public PSCallOperationEditor()
            : base(typeof(PSCallOperation))
        {
        }

        public class Argument
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string DefaultValue { get; set; }
            public string Value { get; set; }
            public bool IsBooleanOrSwitch { get; set; }
            public bool IsOutput { get; set; }
        }
        public class PSCallOperationModel
        {
            public string ScriptName { get; set; }
            public IEnumerable<Argument> Arguments { get; set; }
        }

        private PowerShellScriptInfo GetInfo(QualifiedName scriptName)
        {
            int? applicationId = null;
            if (!string.IsNullOrWhiteSpace(scriptName.Namespace))
            {
                applicationId = DB.Applications_GetApplications(null, true)
                    .FirstOrDefault(a => string.Equals(a.Application_Name, scriptName.Namespace, StringComparison.OrdinalIgnoreCase))
                    ?.Application_Id;
            }

            var name = scriptName.Name;
            if (!name.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                name += ".ps1";

            var script = DB.ScriptAssets_GetScriptByName(name, applicationId);
            if (script == null)
                return null;

            using (var stream = new MemoryStream(script.Script_Text, false))
            using (var reader = new StreamReader(stream, InedoLib.UTF8Encoding))
            {
                PowerShellScriptInfo info;
                PowerShellScriptInfo.TryParse(reader, out info);
                return info;
            }
        }

        public override Type ModelType => typeof(PSCallOperationModel);

        public override ISimpleControl CreateView(ActionStatement action)
        {
            if (action.PositionalArguments?.Count != 1)
                return new LiteralHtml("Cannot edit this statement; the target script name is not present.");

            var scriptName = QualifiedName.Parse(action.PositionalArguments[0]);
            var info = GetInfo(scriptName);
            if (info == null)
                return new LiteralHtml("Cannot edit this statement; script metatdata could not be parsed.");

            var argumentName = new KoElement(KoBind.text(nameof(Argument.Name)));
            var argumentDesc = new KoElement(
                KoBind.text(nameof(Argument.Description))
            );

            var field = new SlimFormField(new LiteralHtml(argumentName.ToString()));
            field.HelpText = new LiteralHtml(argumentDesc.ToString());
            field.Controls.Add(
                new Element("input",
                    new ElementAttribute("type", "text"),
                    new KoBindAttribute("planargvalue", nameof(Argument.Value))));

            return new SimpleVirtualCompositeControl(
                new SlimFormField("Script name:", info.Name ?? scriptName.ToString()),
                new KoElement(
                    KoBind.@foreach(nameof(PSCallOperationModel.Arguments)),
                    field
                ),
                new SlimFormField(
                    "Parameters:",
                    KoBind.visible($"{nameof(PSCallOperationModel.Arguments)}().length == 0"),
                    "This script does not have any input or output parameters."
                )
            );

        }
        protected override object CreateModel(ActionStatement action)
        {
            if (action.PositionalArguments?.Count != 1)
                return null;

            var scriptName = QualifiedName.Parse(action.PositionalArguments[0]);

            var info = GetInfo(scriptName);
            if (info == null)
                return null;

            return new PSCallOperationModel
            {
                ScriptName = scriptName.ToString(),
                Arguments = info.Parameters.Select(p => new Argument
                {
                    DefaultValue = p.DefaultValue,
                    Description = p.Description,
                    IsBooleanOrSwitch = p.IsBooleanOrSwitch,
                    Name = p.Name
                })
            };
        }
        public override ActionStatement CreateActionStatement(QualifiedName name, object _model)
        {
            var model = (PSCallOperationModel)_model;
            return new ActionStatement("PSCall",
                model.Arguments
                    .Where(a => !string.IsNullOrEmpty(a.Value))
                    .ToDictionary(a => a.Name, a => a.Value),
                new[] { model.ScriptName }
            );
        }
    }
}
