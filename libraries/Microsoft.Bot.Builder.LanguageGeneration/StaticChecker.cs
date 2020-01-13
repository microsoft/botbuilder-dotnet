// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Microsoft.Bot.Expressions;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG managed code checker.
    /// </summary>
    internal class StaticChecker : LGFileParserBaseVisitor<List<Diagnostic>>
    {
        private readonly ExpressionEngine baseExpressionEngine;
        private readonly LGFile lgFile;
        private IList<string> visitedTemplateNames;

        private IExpressionParser _expressionParser;

        public StaticChecker(LGFile lgFile, ExpressionEngine expressionEngine = null)
        {
            this.lgFile = lgFile;
            baseExpressionEngine = expressionEngine ?? new ExpressionEngine();
        }

        // Create a property because we want this to be lazy loaded
        private IExpressionParser ExpressionParser
        {
            get
            {
                if (_expressionParser == null)
                {
                    // create an evaluator to leverage it's customized function look up for checking
                    var evaluator = new Evaluator(lgFile.AllTemplates.ToList(), baseExpressionEngine);
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
            visitedTemplateNames = new List<string>();
            var result = new List<Diagnostic>();

            if (lgFile.AllTemplates.Count == 0)
            {
                result.Add(BuildLGDiagnostic(
                    LGErrors.NoTemplate,
                    DiagnosticSeverity.Warning));

                return result;
            }

            lgFile.Templates.ToList().ForEach(t =>
            {
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
                result.Add(BuildLGDiagnostic(LGErrors.InvalidTemplateName, context: errorTemplateName));
            }
            else
            {
                var templateName = context.templateNameLine().templateName().GetText();

                if (visitedTemplateNames.Contains(templateName))
                {
                    result.Add(BuildLGDiagnostic(LGErrors.DuplicatedTemplateInSameTemplate(templateName), context: templateNameLine));
                }
                else
                {
                    visitedTemplateNames.Add(templateName);
                    foreach (var reference in lgFile.References)
                    {
                        var sameTemplates = reference.Templates.Where(u => u.Name == templateName);
                        foreach (var sameTemplate in sameTemplates)
                        {
                            result.Add(BuildLGDiagnostic(LGErrors.DuplicatedTemplateInDiffTemplate(sameTemplate.Name, sameTemplate.Source), context: templateNameLine));
                        }
                    }

                    if (result.Count > 0)
                    {
                        return result;
                    }
                    else
                    {
                        if (context.templateBody() == null)
                        {
                            result.Add(BuildLGDiagnostic(LGErrors.NoTemplateBody(templateName), DiagnosticSeverity.Warning, context.templateNameLine()));
                        }
                        else
                        {
                            result.AddRange(Visit(context.templateBody()));
                        }
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
                    result.Add(BuildLGDiagnostic(LGErrors.InvalidTemplateBody, context: errorTemplateStr));
                }
                else
                {
                    result.AddRange(Visit(templateStr.normalTemplateString()));
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitStructuredTemplateBody([NotNull] LGFileParser.StructuredTemplateBodyContext context)
        {
            var result = new List<Diagnostic>();

            if (context.structuredBodyNameLine().errorStructuredName() != null)
            {
                result.Add(BuildLGDiagnostic(LGErrors.InvalidStrucName, context: context.structuredBodyNameLine()));
            }

            if (context.structuredBodyEndLine() == null)
            {
                result.Add(BuildLGDiagnostic(LGErrors.MissingStrucEnd, context: context));
            }

            var bodys = context.structuredBodyContentLine();

            if (bodys == null || bodys.Length == 0)
            {
                result.Add(BuildLGDiagnostic(LGErrors.EmptyStrucContent, context: context));
            }
            else
            {
                foreach (var body in bodys)
                {
                    if (body.errorStructureLine() != null)
                    {
                        result.Add(BuildLGDiagnostic(LGErrors.InvalidStrucBody, context: body.errorStructureLine()));
                    }
                    else if (body.objectStructureLine() != null)
                    {
                        result.AddRange(CheckExpression(body.objectStructureLine().GetText(), body.objectStructureLine()));
                    }
                    else
                    {
                        // KeyValueStructuredLine
                        var structureValues = body.keyValueStructureLine().keyValueStructureValue();
                        foreach (var structureValue in structureValues)
                        {
                            foreach (var expression in structureValue.EXPRESSION_IN_STRUCTURE_BODY())
                            {
                                result.AddRange(CheckExpression(expression.GetText(), context));
                            }
                        }
                    }
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
                    result.Add(BuildLGDiagnostic(LGErrors.InvalidWhitespaceInCondition, context: conditionNode));
                }

                if (idx == 0 && !ifExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.NotStartWithIfInCondition, DiagnosticSeverity.Warning, conditionNode));
                }

                if (idx > 0 && ifExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.MultipleIfInCondition, context: conditionNode));
                }

                if (idx == ifRules.Length - 1 && !elseExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.NotEndWithElseInCondition, DiagnosticSeverity.Warning, conditionNode));
                }

                if (idx > 0 && idx < ifRules.Length - 1 && !elseIfExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.InvalidMiddleInCondition, context: conditionNode));
                }

                // check rule should should with one and only expression
                if (!elseExpr)
                {
                    if (ifRules[idx].ifCondition().EXPRESSION().Length != 1)
                    {
                        result.Add(BuildLGDiagnostic(LGErrors.InvalidExpressionInCondition, context: conditionNode));
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
                        result.Add(BuildLGDiagnostic(LGErrors.ExtraExpressionInCondition, context: conditionNode));
                    }
                }

                if (ifRules[idx].normalTemplateBody() != null)
                {
                    result.AddRange(Visit(ifRules[idx].normalTemplateBody()));
                }
                else
                {
                    result.Add(BuildLGDiagnostic(LGErrors.MissingTemplateBodyInCondition, context: conditionNode));
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
                    result.Add(BuildLGDiagnostic(LGErrors.InvalidWhitespaceInSwitchCase, context: switchCaseNode));
                }

                if (idx == 0 && !switchExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.NotStartWithSwitchInSwitchCase, context: switchCaseNode));
                }

                if (idx > 0 && switchExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.MultipleSwithStatementInSwitchCase, context: switchCaseNode));
                }

                if (idx > 0 && idx < length - 1 && !caseExpr)
                {
                    result.Add(BuildLGDiagnostic(LGErrors.InvalidStatementInMiddlerOfSwitchCase, context: switchCaseNode));
                }

                if (idx == length - 1 && (caseExpr || defaultExpr))
                {
                    if (caseExpr)
                    {
                        result.Add(BuildLGDiagnostic(LGErrors.NotEndWithDefaultInSwitchCase, DiagnosticSeverity.Warning, switchCaseNode));
                    }
                    else
                    {
                        if (length == 2)
                        {
                            result.Add(BuildLGDiagnostic(LGErrors.MissingCaseInSwitchCase, DiagnosticSeverity.Warning, switchCaseNode));
                        }
                    }
                }

                if (switchExpr || caseExpr)
                {
                    if (switchCaseNode.EXPRESSION().Length != 1)
                    {
                        result.Add(BuildLGDiagnostic(LGErrors.InvalidExpressionInSwiathCase, context: switchCaseNode));
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
                        result.Add(BuildLGDiagnostic(LGErrors.ExtraExpressionInSwitchCase, context: switchCaseNode));
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
                        result.Add(BuildLGDiagnostic(LGErrors.MissingTemplateBodyInSwitchCase, context: switchCaseNode));
                    }
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<Diagnostic>();

            foreach (var expression in context.EXPRESSION())
            {
                result.AddRange(CheckExpression(expression.GetText(), context));
            }

            var multiLinePrefix = context.MULTILINE_PREFIX();
            var multiLineSuffix = context.MULTILINE_SUFFIX();

            if (multiLinePrefix != null && multiLineSuffix == null)
            {
                result.Add(BuildLGDiagnostic(LGErrors.NoEndingInMultiline, context: context));
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
            return new Diagnostic(range, message, severity, lgFile.Id);
        }
    }
}
