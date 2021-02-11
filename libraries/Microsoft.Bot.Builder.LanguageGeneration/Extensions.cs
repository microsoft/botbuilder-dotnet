// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Extension methods for LG.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Regular expression for matching escaped characters.
        /// </summary>
        public static readonly Regex EscapeRegex = new Regex(@"\\[^\r\n]?");

        /// <summary>
        /// If a value is pure Expression.
        /// </summary>
        /// <param name="context">Key value structure value context.</param>
        /// <returns>Is pure expression or not.</returns>
        public static bool IsPureExpression(this LGTemplateParser.KeyValueStructureValueContext context)
        {
            if (context.expressionInStructure() == null
                || context.expressionInStructure().Length != 1)
            {
                return false;
            }

            return context.expressionInStructure(0).GetText().Trim() == context.GetText().Trim();
        }

        /// <summary>
        /// Escape \ from text.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Escaped text.</returns>
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
                else if (value == "\\`")
                {
                    return value.Substring(1);
                }

                return value;
            }));
        }

        /// <summary>
        /// trim expression. ${abc} => abc,  ${a == {}} => a == {}.
        /// </summary>
        /// <param name="expression">Input expression string.</param>
        /// <returns>Pure expression string.</returns>
        public static string TrimExpression(this string expression)
        {
            var result = expression.Trim().TrimStart('$').Trim();

            if (result.StartsWith("{", StringComparison.Ordinal) && result.EndsWith("}", StringComparison.Ordinal))
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
        /// <param name="ambiguousPath">Authored path.</param>
        /// <returns>Path expressed as OS path.</returns>
        public static string NormalizePath(this string ambiguousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Map Linux/Mac separator -> Windows
                return ambiguousPath.Replace("/", "\\");
            }
            else
            {
                // Map Windows separator -> Linux/Mac
                return ambiguousPath.Replace("\\", "/");
            }
        }

        /// <summary>
        /// Get prefix error message from normal template sting context.
        /// </summary>
        /// <param name="context">Normal template sting context.</param>
        /// <returns>Prefix error message.</returns>
        public static string GetPrefixErrorMessage(this LGTemplateParser.NormalTemplateStringContext context)
        {
            var errorPrefix = string.Empty;
            if (context.Parent?.Parent?.Parent is LGTemplateParser.IfConditionRuleContext conditionContext)
            {
                errorPrefix = "Condition '" + conditionContext.ifCondition()?.expression(0)?.GetText() + "': ";
            }
            else
            {
                if (context.Parent?.Parent?.Parent is LGTemplateParser.SwitchCaseRuleContext switchCaseContext)
                {
                    var state = switchCaseContext.switchCaseStat();
                    if (state?.DEFAULT() != null)
                    {
                        errorPrefix = "Case 'Default':";
                    }
                    else if (state?.SWITCH() != null)
                    {
                        errorPrefix = $"Switch '{state.expression(0)?.GetText()}':";
                    }
                    else if (state?.CASE() != null)
                    {
                        errorPrefix = $"Case '{state.expression(0)?.GetText()}':";
                    }
                }
            }

            return errorPrefix;
        }

        /// <summary>
        /// Convert antlr parser into Range.
        /// </summary>
        /// <param name="context">Antlr parse context.</param>
        /// <param name="lineOffset">Line offset.</param>
        /// <returns>Range object.</returns>
        internal static Range ConvertToRange(this ParserRuleContext context, int lineOffset = 0)
        {
            if (context == null)
            {
                return Range.DefaultRange;
            }

            var startPosition = new Position(lineOffset + context.Start.Line, context.Start.Column);
            var stopPosition = new Position(lineOffset + context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
            return new Range(startPosition, stopPosition);
        }
    }
}
