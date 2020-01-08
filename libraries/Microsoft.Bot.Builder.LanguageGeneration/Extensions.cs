// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
        /// <param name="expression">string expressin.</param>
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
                var commonEscapes = new List<string>() { "\\r", "\\n", "\\t" };
                if (commonEscapes.Contains(value))
                {
                    return Regex.Unescape(value);
                }

                return value.Substring(1);
            }));
        }

        /// <summary>
        /// trim expression. @{abc} => abc,  @{a == {}} => a == {}.
        /// </summary>
        /// <param name="expression">input expression string.</param>
        /// <returns>pure expression string.</returns>
        public static string TrimExpression(this string expression)
        {
            var result = expression.Trim().TrimStart('@').Trim();

            if (result.StartsWith("{") && result.EndsWith("}"))
            {
                result = result.Substring(1, result.Length - 2);
            }

            return result;
        }
    }
}
