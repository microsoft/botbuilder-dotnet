// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Extension methods for LG.
    /// </summary>
    public static partial class Extensions
    {
        public static readonly Regex EscapeRegex = new Regex(@"\\[^\r\n]?");

        /// <summary>
        /// If a value is pure Expression.
        /// </summary>
        /// <param name="context">Key value structure value context.</param>
        /// <param name="expression">string expression.</param>
        /// <returns>is pure expression or not.</returns>
        public static bool IsPureExpression(this LGFileParser.KeyValueStructureValueContext context, out string expression)
        {
            expression = context.GetText();

            var hasExpression = false;
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.ESCAPE_CHARACTER_IN_STRUCTURE_BODY:
                        return false;
                    case LGFileParser.EXPRESSION_IN_STRUCTURE_BODY:
                        if (hasExpression)
                        {
                            return false;
                        }

                        hasExpression = true;
                        expression = node.GetText();
                        break;
                    default:
                        if (!string.IsNullOrWhiteSpace(node.GetText()))
                        {
                            return false;
                        }

                        break;
                }
            }

            return hasExpression;
        }

        /// <summary>
        /// Escape \ from text.
        /// </summary>
        /// <param name="text">input text.</param>
        /// <returns>escaped text.</returns>
        public static string Escape(this string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            return EscapeRegex.Replace(text, new MatchEvaluator(m =>
            {
                var value = m.Value;
                var commonEscapes = new List<string>() { "\\r", "\\n", "\\t", "\\\\" };
                if (commonEscapes.Contains(value))
                {
                    return Regex.Unescape(value);
                }

                // $ -> expression escape
                if (value == "\\$")
                {
                    return value.Substring(1);
                }

                return value;
            }));
        }

        /// <summary>
        /// trim expression. ${abc} => abc,  ${a == {}} => a == {}.
        /// </summary>
        /// <param name="expression">input expression string.</param>
        /// <returns>pure expression string.</returns>
        public static string TrimExpression(this string expression)
        {
            var result = expression.Trim().TrimStart('$').Trim();

            if (result.StartsWith("{") && result.EndsWith("}"))
            {
                result = result.Substring(1, result.Length - 2);
            }

            return result;
        }

        /// <summary>
        /// Normalize authored path to OS path.
        /// </summary>
        /// <remarks>
        /// path is from authored content which doesn't know what OS it is running on.
        /// This method treats / and \ both as separators regardless of OS, for Windows that means / -> \ and for Linux/Mac \ -> /.
        /// This allows author to use ../foo.lg or ..\foo.lg as equivalents for importing.
        /// </remarks>
        /// <param name="ambigiousPath">authoredPath.</param>
        /// <returns>path expressed as OS path.</returns>
        public static string NormalizePath(this string ambigiousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Map Linux/Mac separator -> Windows
                return ambigiousPath.Replace("/", "\\");
            }
            else
            {
                // Map Windows separator -> Linux/Mac
                return ambigiousPath.Replace("\\", "/");
            }
        }

        /// <summary>
        /// Get prefix error message from normal template sting context.
        /// </summary>
        /// <param name="context">normal template sting context.</param>
        /// <returns>prefix error message.</returns>
        public static string GetPrefixErrorMessage(this LGFileParser.NormalTemplateStringContext context)
        {
            var errorPrefix = string.Empty;
            if (context.Parent?.Parent?.Parent is LGFileParser.IfConditionRuleContext conditionContext)
            {
                errorPrefix = "Condition '" + conditionContext.ifCondition()?.EXPRESSION(0)?.GetText() + "': ";
            }
            else
            {
                if (context.Parent?.Parent?.Parent is LGFileParser.SwitchCaseRuleContext switchCaseContext)
                {
                    var state = switchCaseContext.switchCaseStat();
                    if (state?.DEFAULT() != null)
                    {
                        errorPrefix = "Case 'Default':";
                    }
                    else if (state?.SWITCH() != null)
                    {
                        errorPrefix = $"Switch '{state.EXPRESSION(0)?.GetText()}':";
                    }
                    else if (state?.CASE() != null)
                    {
                        errorPrefix = $"Case '{state.EXPRESSION(0)?.GetText()}':";
                    }
                }
            }

            return errorPrefix;
        }
    }
}
