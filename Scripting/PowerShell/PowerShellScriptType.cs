using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Scripting;

namespace Inedo.BuildMasterExtensions.Windows.Scripting.PowerShell
{
    [ScriptTypeProperties(
        "Windows PowerShell",
        "Provides script library support for Windows PowerShell scripts.",
        "powershell")]
    [Tag("windows")]
    public sealed class PowerShellScriptType : ScriptTypeBase, IScriptMetadataReader
    {
        private static readonly Regex DocumentationRegex = new Regex(@"\s*\.(?<1>\S+)[ \t]*(?<2>[^\r\n]+)?\s*\n(?<3>(.(?!\n\.))+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
        private static readonly Regex SpaceCollapseRegex = new Regex(@"\s*\n\s*", RegexOptions.Singleline);
        private static readonly Regex ParameterTypeRegex = new Regex(@"^\[(?<1>.+)\]$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        public override string TrueValue
        {
            get { return "$true"; }
        }
        public override string FalseValue
        {
            get { return "$false"; }
        }

        public ScriptMetadata GetScriptMetadata(TextReader scriptText)
        {
            if (scriptText == null)
                throw new ArgumentNullException("scriptText");

            Collection<PSParseError> errors;

            var tokens = PSParser.Tokenize(scriptText.ReadToEnd(), out errors);

            int paramIndex = tokens
                .TakeWhile(t => t.Type != PSTokenType.Keyword || !string.Equals(t.Content, "param", StringComparison.OrdinalIgnoreCase))
                .Count();

            var parameters = this.ScrapeParameters(tokens.Skip(paramIndex + 1)).ToList();

            var documentationToken = tokens
                .Take(paramIndex)
                .LastOrDefault(t => t.Type == PSTokenType.Comment);

            if (documentationToken != null)
            {
                var documentation = documentationToken.Content;
                if (documentation.StartsWith("<#") && documentation.EndsWith("#>"))
                    documentation = documentation.Substring(2, documentation.Length - 4);

                var docBlocks = DocumentationRegex
                    .Matches(documentation)
                    .Cast<Match>()
                    .Select(m => new
                        {
                            Name = m.Groups[1].Value,
                            Arg = !string.IsNullOrWhiteSpace(m.Groups[2].Value) ? m.Groups[2].Value.Trim() : null,
                            Value = !string.IsNullOrWhiteSpace(m.Groups[3].Value) ? SpaceCollapseRegex.Replace(m.Groups[3].Value.Trim(), " ") : null
                        })
                    .Where(d => d.Value != null)
                    .ToLookup(
                        d => d.Name,
                        d => new { d.Arg, d.Value },
                        StringComparer.OrdinalIgnoreCase);

                return new ScriptMetadata(
                    description: docBlocks["SYNOPSIS"].Concat(docBlocks["DESCRIPTION"]).Select(d => d.Value).FirstOrDefault(),
                    parameters: parameters.GroupJoin(
                        docBlocks["PARAMETER"],
                        p => p.Name,
                        d => d.Arg,
                        (p, d) => new ScriptParameterMetadata(p.Name, d.Select(t => t.Value).FirstOrDefault()),
                        StringComparer.OrdinalIgnoreCase)
                );
            }

            return new ScriptMetadata(
                parameters: parameters.Select(p => new ScriptParameterMetadata(p.Name))
            );
        }

        public override IActiveScript ExecuteScript(ScriptExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var ps = System.Management.Automation.PowerShell.Create();

            string fullText;

            using (var reader = GetTextReader2(context))
            {
                fullText = reader.ReadToEnd();
            }

            ps.AddScript(fullText);

            Collection<PSParseError> errors;
            var tokens = PSParser.Tokenize(fullText, out errors);

            int paramIndex = tokens
                .TakeWhile(t => t.Type != PSTokenType.Keyword || !string.Equals(t.Content, "param", StringComparison.OrdinalIgnoreCase))
                .Count();

            var parameters = context
                .Arguments
                .GroupJoin(
                    this.ScrapeParameters(tokens.Skip(paramIndex + 1)),
                    a => a.Key,
                    p => p.Name,
                    (a, p) => new { Name = a.Key, a.Value, Metadata = p.FirstOrDefault() },
                    StringComparer.OrdinalIgnoreCase);

            foreach (var arg in parameters)
            {
                if (arg.Metadata != null)
                {
                    if (string.Equals(arg.Metadata.Type, "switch", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(arg.Value, this.TrueValue, StringComparison.OrdinalIgnoreCase))
                            ps.AddParameter(arg.Name);
                    }
                    else if (string.Equals(arg.Metadata.Type, "bool", StringComparison.OrdinalIgnoreCase) || string.Equals(arg.Metadata.Type, "System.Boolean", StringComparison.OrdinalIgnoreCase))
                    {
                        ps.AddParameter(arg.Name, string.Equals(arg.Value, this.TrueValue, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        ps.AddParameter(arg.Name, arg.Value);
                    }
                }
                else
                {
                    ps.AddParameter(arg.Name, arg.Value);
                }
            }

            foreach (var var in context.Variables)
                ps.Runspace.SessionStateProxy.SetVariable(var.Key, var.Value);

            return new PowerShellScriptRunner(ps);
        }

        private static TextReader GetTextReader2(ScriptExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(context.FileName))
                return context.GetTextReader();

            var correctedPath = Path.Combine(context.WorkingDirectory ?? string.Empty, context.FileName);
            return new StreamReader(new FileStream(correctedPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private IEnumerable<ParamInfo> ScrapeParameters(IEnumerable<PSToken> tokens)
        {
            int groupDepth = 0;
            var paramTokens = new List<PSToken>();

            foreach (var token in tokens)
            {
                paramTokens.Add(token);

                if (token.Type == PSTokenType.GroupStart && token.Content == "(")
                    groupDepth++;

                if (token.Type == PSTokenType.GroupEnd && token.Content == ")")
                {
                    groupDepth--;
                    if (groupDepth <= 0)
                        break;
                }
            }

            string currentType = null;

            foreach (var token in paramTokens)
            {
                if (token.Type == PSTokenType.Type)
                {
                    var match = ParameterTypeRegex.Match(token.Content ?? string.Empty);
                    if (match.Success)
                        currentType = match.Groups[1].Value;
                }

                if (token.Type == PSTokenType.Variable)
                {
                    yield return new ParamInfo
                    {
                        Name = token.Content,
                        Type = currentType
                    };

                    currentType = null;
                }
            }
        }

        private sealed class ParamInfo
        {
            public string Name;
            public string Type;

            public override string ToString()
            {
                if (this.Type != null)
                    return "[" + this.Type + "] " + this.Name;
                else
                    return this.Name;
            }
        }
    }
}
