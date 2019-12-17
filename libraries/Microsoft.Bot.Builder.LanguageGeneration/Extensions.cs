// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Extension methods for LG.
    /// </summary>
    public static partial class Extensions
    {
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

        public static string Escape(this string text)
        {

        }
    }
}
