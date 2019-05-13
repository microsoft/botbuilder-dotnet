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
    public enum ReportEntryType
    {
        /// <summary>
        /// Catch Error info.
        /// </summary>
        ERROR,

        /// <summary>
        /// Catch Warning info.
        /// </summary>
        WARN,
    }

    public class StaticChecker : LGFileParserBaseVisitor<List<ReportEntry>>
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
        public List<ReportEntry> Check()
        {
            var result = new List<ReportEntry>();

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
                    result.Add(new ReportEntry(msg));
                });

                return result;
            }

            // Covert to dict should be fine after checking dup
            templateMap = Templates.ToDictionary(t => t.Name);

            if (Templates.Count == 0)
            {
                result.Add(new ReportEntry(
                    "File must have at least one template definition ",
                    ReportEntryType.WARN));
            }

            Templates.ForEach(t =>
            {
                result.AddRange(Visit(t.ParseTree));
            });

            return result;
        }

        public override List<ReportEntry> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var result = new List<ReportEntry>();
            var templateName = context.templateNameLine().templateName().GetText();

            if (context.templateBody() == null)
            {
                result.Add(new ReportEntry($"There is no template body in template {templateName}", context: context.templateNameLine()));
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
                    result.Add(new ReportEntry($"parameters: {parameters.GetText()} format error", context: context.templateNameLine()));
                }

                var invalidSeperateCharacters = parameters.INVALID_SEPERATE_CHAR();
                if (invalidSeperateCharacters != null
                    && invalidSeperateCharacters.Length > 0)
                {
                    result.Add(new ReportEntry("Parameters for templates must be separated by comma.", context: context.templateNameLine()));
                }
            }

            return result;
        }

        public override List<ReportEntry> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new List<ReportEntry>();

            foreach (var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.AddRange(item);
            }

            return result;
        }

        public override List<ReportEntry> VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var result = new List<ReportEntry>();

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
                    result.Add(new ReportEntry($"At most 1 whitespace is allowed between IF/ELSEIF/ELSE and :. expression: '{context.conditionalTemplateBody().GetText()}", ReportEntryType.ERROR, conditionNode));
                }

                if (idx == 0 && !ifExpr)
                {
                    result.Add(new ReportEntry($"condition is not start with if: '{context.conditionalTemplateBody().GetText()}'", ReportEntryType.WARN, conditionNode));
                }

                if (idx > 0 && ifExpr)
                {
                    result.Add(new ReportEntry($"condition can't have more than one if: '{context.conditionalTemplateBody().GetText()}'", context: conditionNode));
                }

                if (idx == ifRules.Length - 1 && !elseExpr)
                {
                    result.Add(new ReportEntry($"condition is not end with else: '{context.conditionalTemplateBody().GetText()}'", ReportEntryType.WARN, conditionNode));
                }

                if (idx > 0 && idx < ifRules.Length - 1 && !elseIfExpr)
                {
                    result.Add(new ReportEntry($"only elseif is allowed in middle of condition: '{context.conditionalTemplateBody().GetText()}'", context: conditionNode));
                }

                // check rule should should with one and only expression
                if (!elseExpr)
                {
                    if (ifRules[idx].ifCondition().EXPRESSION().Length != 1)
                    {
                        result.Add(new ReportEntry($"if and elseif should followed by one valid expression: '{ifRules[idx].GetText()}'", context: conditionNode));
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
                        result.Add(new ReportEntry($"else should not followed by any expression: '{ifRules[idx].GetText()}'", context: conditionNode));
                    }
                }

                if (ifRules[idx].normalTemplateBody() != null)
                {
                    result.AddRange(Visit(ifRules[idx].normalTemplateBody()));
                }
                else
                {
                    result.Add(new ReportEntry($"no normal template body in condition block: '{ifRules[idx].GetText()}'", context: conditionNode));
                }
            }

            return result;
        }

        public override List<ReportEntry> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<ReportEntry>();

            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.INVALID_ESCAPE:
                        result.Add(new ReportEntry($"escape character {node.GetText()} is invalid", context: context));
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

        public List<ReportEntry> CheckTemplateRef(string exp, ParserRuleContext context)
        {
            var result = new List<ReportEntry>();

            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');

            // Do have args
            if (argsStartPos > 0)
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos + 1)
                {
                    result.Add(new ReportEntry($"Not a valid template ref: {exp}", context: context));
                }
                else
                {
                    var templateName = exp.Substring(0, argsStartPos);
                    if (!templateMap.ContainsKey(templateName))
                    {
                        result.Add(new ReportEntry($"[{templateName}] template not found", context: context));
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
                    result.Add(new ReportEntry($"[{exp}] template not found", context: context));
                }
            }

            return result;
        }

        private List<ReportEntry> CheckMultiLineText(string exp, ParserRuleContext context)
        {
            var result = new List<ReportEntry>();

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

        private List<ReportEntry> CheckText(string exp, ParserRuleContext context)
        {
            var result = new List<ReportEntry>();

            if (exp.StartsWith("```"))
            {
                result.Add(new ReportEntry("Multi line variation must be enclosed in ```", context: context));
            }

            return result;
        }

        private List<ReportEntry> CheckTemplateParameters(string templateName, int argsNumber, ParserRuleContext context)
        {
            var result = new List<ReportEntry>();
            var parametersNumber = templateMap[templateName].Paramters.Count;

            if (argsNumber != parametersNumber)
            {
                result.Add(new ReportEntry($"Arguments count mismatch for template ref {templateName}, expected {parametersNumber}, actual {argsNumber}", context: context));
            }

            return result;
        }

        private List<ReportEntry> CheckExpression(string exp, ParserRuleContext context)
        {
            var result = new List<ReportEntry>();
            exp = exp.TrimStart('{').TrimEnd('}');
            try
            {
                new ExpressionEngine(new GetMethodExtensions(new Evaluator(this.Templates, null)).GetMethodX).Parse(exp);
            }
            catch (Exception e)
            {
                result.Add(new ReportEntry(e.Message + $" in expression `{exp}`", context: context));
                return result;
            }

            return result;
        }
    }

    /// <summary>
    /// Error/Warning report when parsing/evaluating template/inlineText.
    /// </summary>
    public class ReportEntry
    {
        public ReportEntry(
            string message,
            ReportEntryType type = ReportEntryType.ERROR,
            ParserRuleContext context = null,
            Tuple<int, int> start = null,
            Tuple<int, int> stop = null)
        {
            Message = message;
            Type = type;

            if (context != null)
            {
                Start = new Tuple<int, int>(context.Start.Line - 1, context.Start.Column);
                Stop = new Tuple<int, int>(context.Stop.Line - 1, context.Stop.Column);
            }
            else
            {
                Start = start ?? new Tuple<int, int>(0, 0);
                Stop = stop ?? new Tuple<int, int>(0, 0);
            }
        }

        public Tuple<int, int> Start { get; } = new Tuple<int, int>(0, 0);

        public Tuple<int, int> Stop { get; } = new Tuple<int, int>(0, 0);

        public ReportEntryType Type { get; set; }

        public string Message { get; }

        public override string ToString()
        {
            var label = Type == ReportEntryType.ERROR ? "ERROR" : "WARNING";
            return $"{label}: {Message}";
        }
    }
}
