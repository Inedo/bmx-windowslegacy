using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{
    /// <summary>
    /// Contains a method for splitting SQL scripts.
    /// </summary>
    internal static class SqlSplitter
    {
        private static readonly Regex GoRegex = new Regex(@"^\s*GO\s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Returns a collection of the SQL scripts in a string separated by canonical GO statements.
        /// </summary>
        /// <param name="script">The script to split.</param>
        /// <returns>SQL scripts separated by GO's.</returns>
        public static IEnumerable<string> SplitSqlScript(string script)
        {
            if (string.IsNullOrEmpty(script))
                yield break;

            var gos = GoRegex.Matches(script);
            if (gos.Count == 0)
            {
                yield return script;
                yield break;
            }

            var ignored = new List<Run>();

            bool inString = false;
            bool inBlockComment = false;
            bool inInlineComment = false;

            int runStart = 0;

            for (int i = 0; i < script.Length; i++)
            {
                if (inString)
                {
                    if (script[i] == '\'')
                    {
                        if (i < script.Length - 1 && script[i + 1] == '\'')
                            i++;
                        else
                        {
                            inString = false;
                            ignored.Add(new Run { Index = runStart, Length = i - runStart + 1 });
                        }
                    }
                }
                else if (inBlockComment)
                {
                    if (script[i] == '*' && (i >= script.Length - 1 || script[i + 1] == '/'))
                    {
                        i++;
                        inBlockComment = false;
                        ignored.Add(new Run { Index = runStart, Length = i - runStart + 2 });
                    }
                }
                else if (inInlineComment)
                {
                    if (script[i] == '\n')
                    {
                        inInlineComment = false;
                        ignored.Add(new Run { Index = runStart, Length = i - runStart + 1 });
                    }
                }
                else if (script[i] == '\'')
                {
                    inString = true;
                    runStart = i;
                }
                else if (script[i] == '/' && i < script.Length - 1 && script[i + 1] == '*')
                {
                    inBlockComment = true;
                    runStart = i;
                    i++;
                }
                else if (script[i] == '-' && i < script.Length - 1 && script[i + 1] == '-')
                {
                    inInlineComment = true;
                    runStart = i;
                    i++;
                }
            }

            int startPos = 0;

            foreach (Match go in gos)
            {
                if (IsInAnyRun(ignored, go.Index))
                    continue;

                yield return script.Substring(startPos, go.Index - startPos);

                startPos = go.Index + go.Length;
            }

            if (startPos < script.Length)
                yield return script.Substring(startPos);
        }

        private static bool IsInAnyRun(List<Run> runs, int index)
        {
            foreach (var run in runs)
            {
                if (index >= run.Index && index < run.Index + run.Length)
                    return true;
            }

            return false;
        }

        private struct Run
        {
            public int Index;
            public int Length;
        }
    }
}
