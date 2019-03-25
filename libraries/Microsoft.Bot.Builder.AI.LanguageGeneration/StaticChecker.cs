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
                else
                {
                    result.AddRange(CheckExpression(caseRule.caseCondition().EXPRESSION(0).GetText()));
                }
                

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
                    case LGFileParser.TEMPLATE_REF:
                        result.AddRange(CheckTemplateRef(node.GetText()));
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

        public List<string> CheckTemplateRef(string exp)
        {
            var result = new List<string>();

            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');
            if (argsStartPos > 0) // Do have args
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos + 1)
                {
                    result.Add($"Not a valid template ref: {exp}");
                }
                else
                {
                    var templateName = exp.Substring(0, argsStartPos);
                    if (!Context.TemplateContexts.ContainsKey(templateName))
                    {
                        result.Add($"No such template: {templateName}");
                    }
                    else
                    {
                        var argsNumber = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',').Length;
                        result.AddRange(CheckTemplateParameters(templateName, argsNumber));
                    }
                }
            }
            else
            {
                if (!Context.TemplateContexts.ContainsKey(exp))
                {
                    result.Add($"No such template: {exp}");
                }
            }
            return result;
        }

        private List<string> CheckTemplateParameters(string templateName, int argsNumber)
        {
            var result = new List<string>();
            var parametersNumber = Context.TemplateParameters.TryGetValue(templateName, out var parameters) ?
                parameters.Count : 0;

            if (argsNumber != parametersNumber)
            {
                result.Add($"Arguments count mismatch for template ref {templateName}, expected {parametersNumber}, actual {argsNumber}");
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
