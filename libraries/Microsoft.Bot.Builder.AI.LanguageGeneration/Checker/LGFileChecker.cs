using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Checker
{
    public class LGFileChecker : LGFileParserBaseVisitor<int>
    {
        public readonly EvaluationContext Context;

        public LGFileChecker(EvaluationContext context)
        {
            Context = context;
        }

        public void Check()
        {
            foreach(var template in Context.TemplateContexts)
            {
                Visit(template.Value);
            }
        }

        public override int VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            Visit(context.templateBody());
            return 0;
        }

        public override int VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var caseRules = context.conditionalTemplateBody().caseRule();
            foreach (var caseRule in caseRules)
            {
                if (caseRule.caseCondition().EXPRESSION() == null
                    || caseRule.caseCondition().EXPRESSION().Length == 0)
                {
                    throw new LGParserException($"Case condition {caseRule.caseCondition().GetText()} should have expression body");
                }

                CheckExpression(caseRule.caseCondition().EXPRESSION(0).GetText());

                if (caseRule.normalTemplateBody() == null)
                {
                    throw new LGParserException($"Case {caseRule.GetText()} should have template body");
                }
                Visit(caseRule.normalTemplateBody());
            }

            var defaultRule = context?.conditionalTemplateBody()?.defaultRule();

            if(defaultRule != null)
            {
                if(defaultRule.normalTemplateBody() == null)
                    throw new LGParserException($"Default rule {defaultRule.GetText()} should have template body");
                Visit(defaultRule.normalTemplateBody());
            }
           
            
            return 0;
        }


        public override int VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.ESCAPE_CHARACTER:
                        CheckEscapeCharacter(node.GetText());
                        break;
                    case LGFileParser.INVALID_ESCAPE:
                        throw new LGParserException($"escape character {node.GetText()} is invalid");
                    case LGFileParser.TEMPLATE_REF:
                        CheckTemplateRef(node.GetText());
                        break;
                    case LGFileParser.EXPRESSION:
                        CheckExpression(node.GetText());
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        CheckMultiLineText(node.GetText());
                        break;
                    default:
                        break;
                }
            }
            return 0; ;
        }

        private void CheckExpression(string exp)
        {
            exp = exp.TrimStart('{').TrimEnd('}');
            var parseTree = ExpressionEngine.Parse(exp);
            var checker = new ExpressionChecker(Context);

            checker.Check(parseTree);
        }


        private void CheckMultiLineText(string exp)
        {
            exp = exp.Substring(3, exp.Length - 6); //remove ``` ```
            var reg = @"@\{[^{}]+\}";
            var mc = Regex.Matches(exp, reg);

            foreach (Match match in mc)
            {
                var newExp = match.Value.Substring(1); // remove @
                if (newExp.StartsWith("{[") && newExp.EndsWith("]}"))
                {
                    CheckTemplateRef(newExp.Substring(2, newExp.Length - 4));//[ ]
                }
            }
        }


        private void CheckEscapeCharacter(string exp)
        {
            var ValidEscapeCharacters = new List<string> {
                @"\r", @"\n", @"\t", @"\\", @"\[", @"\]", @"\{", @"\}"
            };
            
            if (!ValidEscapeCharacters.Contains(exp))
                throw new LGParserException($"escape character {exp} is invalid");
        }

        public void CheckTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');
            if (argsStartPos > 0) // Do have args
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos + 1)
                {
                    throw new LGParserException($"Not a valid template ref: {exp}");
                }
               
                var templateName = exp.Substring(0, argsStartPos);
                if (!Context.TemplateContexts.ContainsKey(templateName))
                {
                    throw new LGParserException($"No such template: {templateName}");
                }
                var argsNumber = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',').Length;
                CheckTemplateParameters(templateName, argsNumber);
            }
            else
            {
                if (!Context.TemplateContexts.ContainsKey(exp))
                {
                    throw new LGParserException($"No such template: {exp}");
                }
            }
        }

        private void CheckTemplateParameters(string templateName, int argsNumber)
        {
            var parametersNumber = Context.TemplateParameters.TryGetValue(templateName, out var parameters) ?
                parameters.Count : 0;

            if (argsNumber != parametersNumber)
            {
                throw new LGParserException($"Arguments count mismatch for template ref {templateName}, expected {parametersNumber}, actual {argsNumber}");
            }
        }
    }
}
