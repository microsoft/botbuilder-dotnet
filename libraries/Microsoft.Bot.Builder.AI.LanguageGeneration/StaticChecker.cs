using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class StaticChecker : LGFileParserBaseVisitor<List<LGReportMessage>>
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
        public List<LGReportMessage> Check()
        {
            var result = new List<LGReportMessage>();

            if(Context.TemplateContexts == null 
                || Context.TemplateContexts.Count == 0)
            {
                result.Add(new LGReportMessage("File must have at least one template definition ",
                                                LGReportMessageType.WARN));
            }
            else
            {
                foreach (var template in Context.TemplateContexts)
                {
                    result.AddRange(Visit(template.Value));
                }
            }
            

            return result;
        }

        public override List<LGReportMessage> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var result = new List<LGReportMessage>();
            var templateName = context.templateNameLine().templateName().GetText();

            if (context.templateBody() == null)
            {
                result.Add(new LGReportMessage($"There is no template body in template {templateName}"));
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
                    result.Add(new LGReportMessage($"parameters: {parameters.GetText()} format error"));
                }

                var invalidSeperateCharacters = parameters.INVALID_SEPERATE_CHAR();
                if(invalidSeperateCharacters != null 
                    && invalidSeperateCharacters.Length > 0)
                {
                    result.Add(new LGReportMessage("Parameters for templates must be separated by comma."));
                }
            }
            return result;
        }

        public override List<LGReportMessage> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new List<LGReportMessage>();

            foreach (var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.AddRange(item);
            }

            return result;
        }

        public override List<LGReportMessage> VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var result = new List<LGReportMessage>();

            var caseRules = context.conditionalTemplateBody().caseRule();
            if(caseRules == null || caseRules.Length == 0)
            {
                result.Add(new LGReportMessage($"Only default condition will result in a warning.", LGReportMessageType.WARN));
            }
            else
            {
                foreach (var caseRule in caseRules)
                {
                    if (caseRule.caseCondition().EXPRESSION() == null
                        || caseRule.caseCondition().EXPRESSION().Length == 0)
                    {
                        result.Add(new LGReportMessage($"Condition {caseRule.caseCondition().GetText()} MUST be enclosed in curly brackets."));
                    }
                    else
                    {
                        result.AddRange(CheckExpression(caseRule.caseCondition().EXPRESSION(0).GetText()));
                    }


                    if (caseRule.normalTemplateBody() == null)
                    {
                        result.Add(new LGReportMessage($"Case {caseRule.GetText()} should have template body"));
                    }
                    else
                    {
                        result.AddRange(Visit(caseRule.normalTemplateBody()));
                    }
                }
            }
            

            var defaultRule = context?.conditionalTemplateBody()?.defaultRule();

            if (defaultRule != null)
            {
                if (defaultRule.normalTemplateBody() == null)
                    result.Add(new LGReportMessage($"Default rule {defaultRule.GetText()} should have template body"));
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

        public override List<LGReportMessage> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<LGReportMessage>();

            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.ESCAPE_CHARACTER:
                        result.AddRange(CheckEscapeCharacter(node.GetText()));
                        break;
                    case LGFileParser.INVALID_ESCAPE:
                        result.Add(new LGReportMessage($"escape character {node.GetText()} is invalid"));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result.AddRange(CheckTemplateRef(node.GetText()));
                        break;
                    case LGFileParser.EXPRESSION:
                        result.AddRange(CheckExpression(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result.AddRange(CheckMultiLineText(node.GetText()));
                        break;
                    case LGFileLexer.TEXT:
                        result.AddRange(CheckText(node.GetText()));
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public List<LGReportMessage> CheckTemplateRef(string exp)
        {
            var result = new List<LGReportMessage>();

            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');
            if (argsStartPos > 0) // Do have args
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos + 1)
                {
                    result.Add(new LGReportMessage($"Not a valid template ref: {exp}"));
                }
                else
                {
                    var templateName = exp.Substring(0, argsStartPos);
                    if (!Context.TemplateContexts.ContainsKey(templateName))
                    {
                        result.Add(new LGReportMessage($"No such template: {templateName}"));
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
                    result.Add(new LGReportMessage($"No such template: {exp}"));
                }
            }
            return result;
        }

        private List<LGReportMessage> CheckMultiLineText(string exp)
        {
            var result = new List<LGReportMessage>();

            exp = exp.Substring(3, exp.Length - 6); //remove ``` ```
            var reg = @"@\{[^{}]+\}";
            var mc = Regex.Matches(exp, reg);

            foreach (Match match in mc)
            {
                var newExp = match.Value.Substring(1); // remove @
                if (newExp.StartsWith("{[") && newExp.EndsWith("]}"))
                {
                    result.AddRange(CheckTemplateRef(newExp.Substring(2, newExp.Length - 4)));//[ ]
                }
            }
            return result;
        }

        private List<LGReportMessage> CheckText(string exp)
        {
            var result = new List<LGReportMessage>();

            if (exp.StartsWith("```"))
                result.Add(new LGReportMessage("Multi line variation must be enclosed in ```"));
            return result;
        }

        private List<LGReportMessage> CheckTemplateParameters(string templateName, int argsNumber)
        {
            var result = new List<LGReportMessage>();
            var parametersNumber = Context.TemplateParameters.TryGetValue(templateName, out var parameters) ?
                parameters.Count : 0;

            if (argsNumber != parametersNumber)
            {
                result.Add(new LGReportMessage($"Arguments count mismatch for template ref {templateName}, expected {parametersNumber}, actual {argsNumber}"));
            }

            return result;
        }

        private List<LGReportMessage> CheckExpression(string exp)
        {
            var result = new List<LGReportMessage>();
            exp = exp.TrimStart('{').TrimEnd('}');
            try
            {
                ExpressionEngine.Parse(exp);
            }
            catch(Exception e)
            {
                result.Add(new LGReportMessage(e.Message));
                return result;
            }

            return result;
            
        }

        private List<LGReportMessage> CheckEscapeCharacter(string exp)
        {
            var result = new List<LGReportMessage>();
            var ValidEscapeCharacters = new List<string> {
                @"\r", @"\n", @"\t", @"\\", @"\[", @"\]", @"\{", @"\}"
            };

            if (!ValidEscapeCharacters.Contains(exp))
                result.Add(new LGReportMessage($"escape character {exp} is invalid"));

            return result;
        }
    }
}
