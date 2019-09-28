using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class StaticChecker
    {
        private readonly ExpressionEngine expressionEngine;

        public StaticChecker(ExpressionEngine expressionEngine = null)
        {
            this.expressionEngine = expressionEngine ?? new ExpressionEngine();
        }

        public List<Diagnostic> CheckFiles(IEnumerable<string> filePaths, ImportResolverDelegate importResolver = null)
        {
            var result = new List<Diagnostic>();
            var templates = new List<LGTemplate>();
            var isParseSuccess = true;
            try
            {
                var totalLGResources = new List<LGResource>();
                foreach (var filePath in filePaths)
                {
                    importResolver = importResolver ?? ImportResolver.FileResolver;

                    var fullPath = Path.GetFullPath(ImportResolver.NormalizePath(filePath));
                    var rootResource = LGParser.Parse(File.ReadAllText(fullPath), fullPath);
                    var resources = rootResource.DiscoverDependencies(importResolver);
                    totalLGResources.AddRange(resources);
                }

                var deduplicatedLGResources = totalLGResources.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                templates = deduplicatedLGResources.SelectMany(x => x.Templates).ToList();
            }
            catch (LGException ex)
            {
                result.AddRange(ex.Diagnostics);
                isParseSuccess = false;
            }
            catch (Exception err)
            {
                result.Add(new Diagnostic(new Range(new Position(0, 0), new Position(0, 0)), err.Message));
                isParseSuccess = false;
            }

            if (isParseSuccess)
            {
                result.AddRange(CheckTemplates(templates));
            }

            return result;
        }

        public List<Diagnostic> CheckFile(string filePath, ImportResolverDelegate importResolver = null) => CheckFiles(new List<string>() { filePath }, importResolver);

        public List<Diagnostic> CheckText(string content, string id = "", ImportResolverDelegate importResolver = null)
        {
            if (importResolver == null)
            {
                var importPath = ImportResolver.NormalizePath(id);
                if (!Path.IsPathRooted(importPath))
                {
                    throw new Exception("[Error] id must be full path when importResolver is null");
                }
            }

            var result = new List<Diagnostic>();
            var templates = new List<LGTemplate>();
            var isParseSuccess = true;
            try
            {
                var rootResource = LGParser.Parse(content, id);
                var resources = rootResource.DiscoverDependencies(importResolver);
                templates = resources.SelectMany(x => x.Templates).ToList();
            }
            catch (LGException ex)
            {
                result.AddRange(ex.Diagnostics);
                isParseSuccess = false;
            }
            catch (Exception err)
            {
                result.Add(new Diagnostic(new Range(new Position(0, 0), new Position(0, 0)), err.Message));
                isParseSuccess = false;
            }

            if (isParseSuccess)
            {
                result.AddRange(CheckTemplates(templates));
            }

            return result;
        }

        public List<Diagnostic> CheckTemplates(List<LGTemplate> templates) => new StaticCheckerInner(templates, expressionEngine).Check();

        private class StaticCheckerInner : LGFileParserBaseVisitor<List<Diagnostic>>
        {
            private Dictionary<string, LGTemplate> templateMap = new Dictionary<string, LGTemplate>();

            private string currentSource = string.Empty;
            private ExpressionEngine baseExpressionEngine;

            private IExpressionParser _expressionParser;

            public StaticCheckerInner(List<LGTemplate> templates, ExpressionEngine expressionEngine)
            {
                Templates = templates;
                baseExpressionEngine = expressionEngine;
            }

            public List<LGTemplate> Templates { get; }

            // Create a property because we want this to be lazy loaded
            private IExpressionParser ExpressionParser
            {
                get
                {
                    if (_expressionParser == null)
                    {
                        // create an evaluator to leverage it's customized function look up for checking
                        var evaluator = new Evaluator(Templates, baseExpressionEngine);
                        _expressionParser = evaluator.ExpressionEngine;
                    }

                    return _expressionParser;
                }
            }

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
                        result.Add(BuildLGDiagnostic(msg));
                    });

                    return result;
                }

                // Covert to dict should be fine after checking dup
                templateMap = Templates.ToDictionary(t => t.Name);

                if (Templates.Count == 0)
                {
                    result.Add(BuildLGDiagnostic(
                        "File must have at least one template definition ",
                        DiagnosticSeverity.Warning));
                }

                Templates.ForEach(t =>
                {
                    currentSource = t.Source;
                    result.AddRange(Visit(t.ParseTree));
                });

                return result;
            }

            public override List<Diagnostic> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
            {
                var result = new List<Diagnostic>();
                var templateNameLine = context.templateNameLine();
                var errorTemplateName = templateNameLine.errorTemplateName();
                if (errorTemplateName != null)
                {
                    result.Add(BuildLGDiagnostic($"Not a valid template name line", context: errorTemplateName));
                }
                else
                {
                    var templateName = context.templateNameLine().templateName().GetText();

                    if (context.templateBody() == null)
                    {
                        result.Add(BuildLGDiagnostic($"There is no template body in template {templateName}", context: context.templateNameLine()));
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
                            result.Add(BuildLGDiagnostic($"parameters: {parameters.GetText()} format error", context: context.templateNameLine()));
                        }
                    }
                }

                return result;
            }

            public override List<Diagnostic> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
            {
                var result = new List<Diagnostic>();

                foreach (var templateStr in context.templateString())
                {
                    var errorTemplateStr = templateStr.errorTemplateString();
                    if (errorTemplateStr != null)
                    {
                        result.Add(BuildLGDiagnostic($"Invalid template body line, did you miss '-' at line begin", context: errorTemplateStr));
                    }
                    else
                    {
                        var item = Visit(templateStr.normalTemplateString());
                        result.AddRange(item);
                    }
                }

                return result;
            }

            public override List<Diagnostic> VisitIfElseBody([NotNull] LGFileParser.IfElseBodyContext context)
            {
                var result = new List<Diagnostic>();

                var ifRules = context.ifElseTemplateBody().ifConditionRule();
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
                        result.Add(BuildLGDiagnostic($"At most 1 whitespace is allowed between IF/ELSEIF/ELSE and :", context: conditionNode));
                    }

                    if (idx == 0 && !ifExpr)
                    {
                        result.Add(BuildLGDiagnostic($"condition is not start with if", DiagnosticSeverity.Warning, conditionNode));
                    }

                    if (idx > 0 && ifExpr)
                    {
                        result.Add(BuildLGDiagnostic($"condition can't have more than one if", context: conditionNode));
                    }

                    if (idx == ifRules.Length - 1 && !elseExpr)
                    {
                        result.Add(BuildLGDiagnostic($"condition is not end with else", DiagnosticSeverity.Warning, conditionNode));
                    }

                    if (idx > 0 && idx < ifRules.Length - 1 && !elseIfExpr)
                    {
                        result.Add(BuildLGDiagnostic($"only elseif is allowed in middle of condition", context: conditionNode));
                    }

                    // check rule should should with one and only expression
                    if (!elseExpr)
                    {
                        if (ifRules[idx].ifCondition().EXPRESSION().Length != 1)
                        {
                            result.Add(BuildLGDiagnostic($"if and elseif should followed by one valid expression", context: conditionNode));
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
                            result.Add(BuildLGDiagnostic($"else should not followed by any expression", context: conditionNode));
                        }
                    }

                    if (ifRules[idx].normalTemplateBody() != null)
                    {
                        result.AddRange(Visit(ifRules[idx].normalTemplateBody()));
                    }
                    else
                    {
                        result.Add(BuildLGDiagnostic($"no normal template body in condition block", context: conditionNode));
                    }
                }

                return result;
            }

            public override List<Diagnostic> VisitSwitchCaseBody([NotNull] LGFileParser.SwitchCaseBodyContext context)
            {
                var result = new List<Diagnostic>();
                var switchCaseRules = context.switchCaseTemplateBody().switchCaseRule();
                var length = switchCaseRules.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    var switchCaseNode = switchCaseRules[idx].switchCaseStat();
                    var switchExpr = switchCaseNode.SWITCH() != null;
                    var caseExpr = switchCaseNode.CASE() != null;
                    var defaultExpr = switchCaseNode.DEFAULT() != null;
                    var node = switchExpr ? switchCaseNode.SWITCH() :
                               caseExpr ? switchCaseNode.CASE() :
                               switchCaseNode.DEFAULT();

                    if (node.GetText().Count(u => u == ' ') > 1)
                    {
                        result.Add(BuildLGDiagnostic($"At most 1 whitespace is allowed between SWITCH/CASE/DEFAULT and :.", context: switchCaseNode));
                    }

                    if (idx == 0 && !switchExpr)
                    {
                        result.Add(BuildLGDiagnostic($"control flow is not start with switch", context: switchCaseNode));
                    }

                    if (idx > 0 && switchExpr)
                    {
                        result.Add(BuildLGDiagnostic($"control flow can not have more than one switch statement", context: switchCaseNode));
                    }

                    if (idx > 0 && idx < length - 1 && !caseExpr)
                    {
                        result.Add(BuildLGDiagnostic($"only case statement is allowed in the middle of control flow", context: switchCaseNode));
                    }

                    if (idx == length - 1 && (caseExpr || defaultExpr))
                    {
                        if (caseExpr)
                        {
                            result.Add(BuildLGDiagnostic($"control flow is not ending with default statement", DiagnosticSeverity.Warning, switchCaseNode));
                        }
                        else
                        {
                            if (length == 2)
                            {
                                result.Add(BuildLGDiagnostic($"control flow should have at least one case statement", DiagnosticSeverity.Warning, switchCaseNode));
                            }
                        }
                    }

                    if (switchExpr || caseExpr)
                    {
                        if (switchCaseNode.EXPRESSION().Length != 1)
                        {
                            result.Add(BuildLGDiagnostic($"switch and case should followed by one valid expression", context: switchCaseNode));
                        }
                        else
                        {
                            result.AddRange(CheckExpression(switchCaseNode.EXPRESSION(0).GetText(), switchCaseNode));
                        }
                    }
                    else
                    {
                        if (switchCaseNode.EXPRESSION().Length != 0 || switchCaseNode.TEXT().Length != 0)
                        {
                            result.Add(BuildLGDiagnostic($"default should not followed by any expression or any text", context: switchCaseNode));
                        }
                    }

                    if (caseExpr || defaultExpr)
                    {
                        if (switchCaseRules[idx].normalTemplateBody() != null)
                        {
                            result.AddRange(Visit(switchCaseRules[idx].normalTemplateBody()));
                        }
                        else
                        {
                            result.Add(BuildLGDiagnostic($"no normal template body in case or default block", context: switchCaseNode));
                        }
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
                var expression = exp;
                if (exp.IndexOf('(') < 0)
                {
                    if (this.templateMap.ContainsKey(exp))
                    {
                        expression = exp + "(" + string.Join(",", this.templateMap[exp].Parameters) + ")";
                    }
                    else
                    {
                        expression = exp + "()";
                    }
                }

                try
                {
                    ExpressionParser.Parse(expression);
                }
                catch (Exception e)
                {
                    result.Add(BuildLGDiagnostic(e.Message + $" in template reference `{exp}`", context: context));
                    return result;
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
                    result.AddRange(CheckExpression(match.Value, context));
                }

                return result;
            }

            private List<Diagnostic> CheckText(string exp, ParserRuleContext context)
            {
                var result = new List<Diagnostic>();

                if (exp.StartsWith("```"))
                {
                    result.Add(BuildLGDiagnostic("Multi line variation must be enclosed in ```", context: context));
                }

                return result;
            }

            private List<Diagnostic> CheckExpression(string exp, ParserRuleContext context)
            {
                var result = new List<Diagnostic>();
                exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');

                try
                {
                    ExpressionParser.Parse(exp);
                }
                catch (Exception e)
                {
                    result.Add(BuildLGDiagnostic(e.Message + $" in expression `{exp}`", context: context));
                    return result;
                }

                return result;
            }

            /// <summary>
            /// Build LG diagnostic with antlr tree node context.
            /// </summary>
            /// <param name="message">error/warning message. <see cref="Diagnostic.Message"/>.</param>
            /// <param name="severity">diagnostic Severity <see cref="DiagnosticSeverity"/> to get more info.</param>
            /// <param name="context">the parsed tree node context of the diagnostic.</param>
            /// <returns>new Diagnostic object.</returns>
            private Diagnostic BuildLGDiagnostic(
                string message,
                DiagnosticSeverity severity = DiagnosticSeverity.Error,
                ParserRuleContext context = null)
            {
                var startPosition = context == null ? new Position(0, 0) : new Position(context.Start.Line, context.Start.Column);
                var stopPosition = context == null ? new Position(0, 0) : new Position(context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
                var range = new Range(startPosition, stopPosition);
                message = $"source: {currentSource}. error message: {message}";
                return new Diagnostic(range, message, severity);
            }
        }
    }
}
