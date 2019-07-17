using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGBaseVisitor<TResult> : LGFileParserBaseVisitor<TResult>
    {
        public TResult VisitExpression(string exp, ParserRuleContext context = null)
        {
            exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');
            return OnVisitExpression(exp, context);
        }

        public virtual TResult OnVisitExpression(string exp, ParserRuleContext context = null)
        {
            throw new NotImplementedException();
        }

        public TResult VisitTemplateRef(string exp, ParserRuleContext context = null)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();
            return OnVisitTemplateRef(exp, context);
        }

        public virtual TResult OnVisitTemplateRef(string exp, ParserRuleContext context = null)
        {
            var expressionStr = MakeExpressionStrFromTemplateRef(exp);
            return VisitExpression(expressionStr, context);
        }

        public TResult VisitFenceBlock(string exp, ParserRuleContext context = null)
        {
            // remove ``` ```
            exp = exp.Substring(3, exp.Length - 6);
            return OnVisitFenceBlock(exp, context);
        }

        public virtual TResult OnVisitFenceBlock(string exp, ParserRuleContext context = null)
        {
            throw new NotImplementedException();
        }

        public TResult VisitEscapeCharacter(string exp, ParserRuleContext context = null)
        {
            var validCharactersDict = new Dictionary<string, string>
            {
                // Top four items :C# later render engine will treat them as escape characters, so the format is unchanged
                { @"\r", "\r" },
                { @"\n", "\n" },
                { @"\t", "\t" },
                { @"\\", "\\" },
                { @"\[", "[" },
                { @"\]", "]" },
                { @"\{", "{" },
                { @"\}", "}" },
            };

            return OnVisitEscapeCharacter(validCharactersDict[exp], context);
        }

        public virtual TResult OnVisitEscapeCharacter(string exp, ParserRuleContext context = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tranfer from template(a,b,c) -> lgTemplate('template', a, b, c).
        /// </summary>
        /// <param name="templateRef">origin template reference string.</param>
        /// <returns>return expression string of template reference.</returns>
        private string MakeExpressionStrFromTemplateRef(string templateRef)
        {
            var (templateName, paramStr) = ParseTemplateReference(templateRef);

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new Exception($"Not a valid template ref: {templateRef}");
            }

            var sb = new StringBuilder();
            sb.Append("lgTemplate(");
            sb.Append($"'{templateName}'");
            if (!string.IsNullOrWhiteSpace(paramStr))
            {
                sb.Append(", ");
                sb.Append(paramStr);
            }

            sb.Append(")");

            return sb.ToString();
        }

        private (string templateName, string paramStr) ParseTemplateReference(string templateRef)
        {
            var pattern = @"^([\w\-\.]*)(\((.*)\))?$";

            var templateName = string.Empty;
            var parameStr = string.Empty;

            var matchResults = Regex.Match(templateRef, pattern);
            if (matchResults.Success)
            {
                if (matchResults.Groups.Count == 4)
                {
                    templateName = matchResults.Groups[1].Value;
                    parameStr = matchResults.Groups[3].Value;
                }
            }

            return (templateName, parameStr);
        }
    }
}
