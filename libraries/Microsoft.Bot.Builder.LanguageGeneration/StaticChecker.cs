using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class StaticChecker : LGFileParserBaseVisitor<List<Diagnostic>>
    {
        private Dictionary<string, LGTemplate> templateMap = new Dictionary<string, LGTemplate>();

        public StaticChecker(List<LGTemplate> templates)
        {
            Templates = templates;
        }

        public List<LGTemplate> Templates { get; }

        /// <summary>
        /// Return error messaages list.
        /// </summary>
        /// <returns>report result.</returns>
        public List<Diagnostic> Check()
        {
            var result = new List<Diagnostic>();

            // check dup first
            var duplicatedTemplates = Templates
                                      .GroupBy(t => t.Name)
                                      .Where(g => g.Count() > 1)
                                      .ToList();

            if (duplicatedTemplates.Count > 0)
            {
                duplicatedTemplates.ForEach(g =>
                {
                    var name = g.Key;
                    var sources = string.Join(":", g.Select(x => x.Source));

                    var msg = $"Duplicated definitions found for template: {name} in {sources}";
                    result.Add(BuildBotDiagnostic(msg));
                });

                return result;
            }

            // Covert to dict should be fine after checking dup
            templateMap = Templates.ToDictionary(t => t.Name);

            if (Templates.Count == 0)
            {
                result.Add(BuildBotDiagnostic(
                    "File must have at least one template definition ",
                    DiagnosticSeverity.Warning));
            }

            Templates.ForEach(t =>
            {
                result.AddRange(Visit(t.ParseTree));
            });

            return result;
        }

        public override List<Diagnostic> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var result = new List<Diagnostic>();
            var templateName = context.templateNameLine().templateName().GetText();

            if (context.templateBody() == null)
            {
                result.Add(BuildBotDiagnostic($"There is no template body in template {templateName}", context: context.templateNameLine()));
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
                    result.Add(BuildBotDiagnostic($"parameters: {parameters.GetText()} format error", context: context.templateNameLine()));
                }

                var invalidSeperateCharacters = parameters.INVALID_SEPERATE_CHAR();
                if (invalidSeperateCharacters != null
                    && invalidSeperateCharacters.Length > 0)
                {
                    result.Add(BuildBotDiagnostic("Parameters for templates must be separated by comma.", context: context.templateNameLine()));
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new List<Diagnostic>();

            foreach (var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.AddRange(item);
            }

            return result;
        }

        public override List<Diagnostic> VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var result = new List<Diagnostic>();

            var ifRules = context.conditionalTemplateBody().ifConditionRule();
            for (var idx = 0; idx < ifRules.Length; idx++)
            {
                var conditionNode = ifRules[idx].ifCondition();
                var ifExpr = conditionNode.IF() != null;
                var elseIfExpr = conditionNode.ELSEIF() != null;
                var elseExpr = conditionNode.ELSE() != null;

                var node = ifExpr ? conditionNode.IF() :
                           elseIfExpr ? conditionNode.ELSEIF() :
                           conditionNode.ELSE();

                if (node.GetText().Count(u => u == ' ') > 1)
                {
                    result.Add(BuildBotDiagnostic($"At most 1 whitespace is allowed between IF/ELSEIF/ELSE and :. expression: '{context.conditionalTemplateBody().GetText()}", context: conditionNode));
                }

                if (idx == 0 && !ifExpr)
                {
                    result.Add(BuildBotDiagnostic($"condition is not start with if: '{context.conditionalTemplateBody().GetText()}'", DiagnosticSeverity.Warning, conditionNode));
                }

                if (idx > 0 && ifExpr)
                {
                    result.Add(BuildBotDiagnostic($"condition can't have more than one if: '{context.conditionalTemplateBody().GetText()}'", context: conditionNode));
                }

                if (idx == ifRules.Length - 1 && !elseExpr)
                {
                    result.Add(BuildBotDiagnostic($"condition is not end with else: '{context.conditionalTemplateBody().GetText()}'", DiagnosticSeverity.Warning, conditionNode));
                }

                if (idx > 0 && idx < ifRules.Length - 1 && !elseIfExpr)
                {
                    result.Add(BuildBotDiagnostic($"only elseif is allowed in middle of condition: '{context.conditionalTemplateBody().GetText()}'", context: conditionNode));
                }

                // check rule should should with one and only expression
                if (!elseExpr)
                {
                    if (ifRules[idx].ifCondition().EXPRESSION().Length != 1)
                    {
                        result.Add(BuildBotDiagnostic($"if and elseif should followed by one valid expression: '{ifRules[idx].GetText()}'", context: conditionNode));
                    }
                    else
                    {
                        result.AddRange(CheckExpression(ifRules[idx].ifCondition().EXPRESSION(0).GetText(), conditionNode));
                    }
                }
                else
                {
                    if (ifRules[idx].ifCondition().EXPRESSION().Length != 0)
                    {
                        result.Add(BuildBotDiagnostic($"else should not followed by any expression: '{ifRules[idx].GetText()}'", context: conditionNode));
                    }
                }

                if (ifRules[idx].normalTemplateBody() != null)
                {
                    result.AddRange(Visit(ifRules[idx].normalTemplateBody()));
                }
                else
                {
                    result.Add(BuildBotDiagnostic($"no normal template body in condition block: '{ifRules[idx].GetText()}'", context: conditionNode));
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<Diagnostic>();

            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.INVALID_ESCAPE:
                        result.Add(BuildBotDiagnostic($"escape character {node.GetText()} is invalid", context: context));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result.AddRange(CheckTemplateRef(node.GetText(), context));
                        break;
                    case LGFileParser.EXPRESSION:
                        result.AddRange(CheckExpression(node.GetText(), context));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result.AddRange(CheckMultiLineText(node.GetText(), context));
                        break;
                    case LGFileLexer.TEXT:
                        result.AddRange(CheckText(node.GetText(), context));
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public List<Diagnostic> CheckTemplateRef(string exp, ParserRuleContext context)
        {
            var result = new List<Diagnostic>();

            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');

            // Do have args
            if (argsStartPos > 0)
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos + 1)
                {
                    result.Add(BuildBotDiagnostic($"Not a valid template ref: {exp}", context: context));
                }
                else
                {
                    var templateName = exp.Substring(0, argsStartPos);
                    if (!templateMap.ContainsKey(templateName))
                    {
                        result.Add(BuildBotDiagnostic($"[{templateName}] template not found", context: context));
                    }
                    else
                    {
                        var argsNumber = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',').Length;
                        result.AddRange(CheckTemplateParameters(templateName, argsNumber, context));
                    }
                }
            }
            else
            {
                if (!templateMap.ContainsKey(exp))
                {
                    result.Add(BuildBotDiagnostic($"[{exp}] template not found", context: context));
                }
            }

            return result;
        }

        private List<Diagnostic> CheckMultiLineText(string exp, ParserRuleContext context)
        {
            var result = new List<Diagnostic>();

            // remove ``` ```
            exp = exp.Substring(3, exp.Length - 6);
            var reg = @"@\{[^{}]+\}";
            var mc = Regex.Matches(exp, reg);

            foreach (Match match in mc)
            {
                var newExp = match.Value.Substring(1); // remove @
                result.AddRange(CheckExpression(newExp, context));
            }

            return result;
        }

        private List<Diagnostic> CheckText(string exp, ParserRuleContext context)
        {
            var result = new List<Diagnostic>();

            if (exp.StartsWith("```"))
            {
                result.Add(BuildBotDiagnostic("Multi line variation must be enclosed in ```", context: context));
            }

            return result;
        }

        private List<Diagnostic> CheckTemplateParameters(string templateName, int argsNumber, ParserRuleContext context)
        {
            var result = new List<Diagnostic>();
            var parametersNumber = templateMap[templateName].Paramters.Count;

            if (argsNumber != parametersNumber)
            {
                result.Add(BuildBotDiagnostic($"Arguments count mismatch for template ref {templateName}, expected {parametersNumber}, actual {argsNumber}", context: context));
            }

            return result;
        }

        private List<Diagnostic> CheckExpression(string exp, ParserRuleContext context)
        {
            var result = new List<Diagnostic>();
            exp = exp.TrimStart('{').TrimEnd('}');
            try
            {
                new ExpressionEngine(new GetMethodExtensions(new Evaluator(this.Templates, null)).GetMethodX).Parse(exp);
            }
            catch (Exception e)
            {
                result.Add(BuildBotDiagnostic(e.Message + $" in expression `{exp}`", context: context));
                return result;
            }

            return result;
        }

        private Diagnostic BuildBotDiagnostic(
            string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Error,
            ParserRuleContext context = null)
        {
            var start = context == null ? new Position(0, 0) : new Position(context.Start.Line - 1, context.Start.Column);
            var stop = context == null ? new Position(0, 0) : new Position(context.Stop.Line - 1, context.Stop.Column);
            var range = new Range(start, stop);
            return new Diagnostic(range, message, severity);
        }
    }
}
