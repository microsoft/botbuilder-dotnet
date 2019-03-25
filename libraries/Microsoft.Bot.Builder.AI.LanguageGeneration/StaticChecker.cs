using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class StaticChecker : LGFileParserBaseVisitor<List<string>>
    {
        public readonly EvaluationContext Context;

        public StaticChecker(EvaluationContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Return error messaages list
        /// </summary>
        /// <returns></returns>
        public List<string> Check()
        {
            var result = new List<string>();
            foreach (var template in Context.TemplateContexts)
            {
                result.AddRange(Visit(template.Value));
            }

            return result;
        }

        public override List<string> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var result = new List<string>();
            var templateName = context.templateNameLine().templateName().GetText();

            if (context.templateBody() == null)
            {
                result.Add($"There is no template body in template {templateName}");
            }
            else
            {
                result.AddRange(Visit(context.templateBody()));
            }

            var parameters = context.templateNameLine().parameters();
            if (parameters != null)
            {
                if (parameters.CLOSE_PARENTHESIS() == null
                       || parameters.OPEN_PARENTHESIS() == null)
                {
                    result.Add($"parameters: {parameters.GetText()} format error");
                }
            }
            return result;
        }

        public override List<string> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new List<string>();

            foreach (var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.AddRange(item);
            }

            return result;
        }

        public override List<string> VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var result = new List<string>();

            var caseRules = context.conditionalTemplateBody().caseRule();
            foreach (var caseRule in caseRules)
            {
                if (caseRule.caseCondition().EXPRESSION() == null
                    || caseRule.caseCondition().EXPRESSION().Length == 0)
                {
                    result.Add($"Condition {caseRule.caseCondition().GetText()} MUST be enclosed in curly brackets.");
                }

                result.AddRange(CheckExpression(caseRule.caseCondition().EXPRESSION(0).GetText()));

                if (caseRule.normalTemplateBody() == null)
                {
                    result.Add($"Case {caseRule.GetText()} should have template body");
                }
                else
                {
                    result.AddRange(Visit(caseRule.normalTemplateBody()));
                }
            }

            var defaultRule = context?.conditionalTemplateBody()?.defaultRule();

            if (defaultRule != null)
            {
                if (defaultRule.normalTemplateBody() == null)
                    result.Add($"Default rule {defaultRule.GetText()} should have template body");
                else
                {
                    result.AddRange(Visit(defaultRule.normalTemplateBody()));
                }
            }
            else
            {
                //throw WARN
            }

            return result;
        }

        public override List<string> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<string>();

            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.ESCAPE_CHARACTER:
                        result.AddRange(CheckEscapeCharacter(node.GetText()));
                        break;
                    case LGFileParser.INVALID_ESCAPE:
                        result.Add($"escape character {node.GetText()} is invalid");
                        break;
                    case LGFileParser.EXPRESSION:
                        result.AddRange(CheckExpression(node.GetText()));
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private List<string> CheckExpression(string exp)
        {
            var result = new List<string>();
            exp = exp.TrimStart('{').TrimEnd('}');
            try
            {
                ExpressionEngine.Parse(exp);
            }
            catch(Exception e)
            {
                result.Add(e.Message);
                return result;
            }

            return result;
            
        }

        private List<string> CheckEscapeCharacter(string exp)
        {
            var result = new List<string>();
            var ValidEscapeCharacters = new List<string> {
                @"\r", @"\n", @"\t", @"\\", @"\[", @"\]", @"\{", @"\}"
            };

            if (!ValidEscapeCharacters.Contains(exp))
                result.Add($"escape character {exp} is invalid");

            return result;
        }
    }
}
