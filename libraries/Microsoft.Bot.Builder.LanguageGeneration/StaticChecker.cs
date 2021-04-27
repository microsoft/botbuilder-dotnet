// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG managed code checker.
    /// </summary>
    internal class StaticChecker : LGTemplateParserBaseVisitor<List<Diagnostic>>
    {
        private readonly Templates _templates;
        private Template _currentTemplate;

        private IExpressionParser _expressionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticChecker"/> class.
        /// </summary>
        /// <param name="templates">Templates wihch would be checked.</param>
        public StaticChecker(Templates templates)
        {
            _templates = templates;
        }

        // Create a property because we want this to be lazy loaded
        private IExpressionParser ExpressionParser
        {
            get
            {
                if (_expressionParser == null)
                {
                    // create an evaluator to leverage it's customized function look up for checking
                    var evaluator = new Evaluator(_templates, _templates.LgOptions);
                    _expressionParser = evaluator.ExpressionParser;
                }

                return _expressionParser;
            }
        }

        /// <summary>
        /// Returns a list of Diagnostic instances.
        /// </summary>
        /// <returns>Report result.</returns>
        public List<Diagnostic> Check()
        {
            var result = new List<Diagnostic>();

            if (_templates.AllTemplates.Count == 0)
            {
                var diagnostic = new Diagnostic(Range.DefaultRange, TemplateErrors.NoTemplate, DiagnosticSeverity.Warning, _templates.Source);
                result.Add(diagnostic);
                return result;
            }

            foreach (var template in _templates)
            {
                _currentTemplate = template;
                var templateDiagnostics = new List<Diagnostic>();

                // checker duplicated in different files
                foreach (var reference in _templates.References)
                {
                    var sameTemplates = reference.Where(u => u.Name == template.Name);
                    foreach (var sameTemplate in sameTemplates)
                    {
                        var startLine = template.SourceRange.Range.Start.Line;
                        var range = new Range(startLine, 0, startLine, template.Name.Length + 1);
                        var diagnostic = new Diagnostic(range, TemplateErrors.DuplicatedTemplateInDiffTemplate(sameTemplate.Name, sameTemplate.SourceRange.Source), source: _templates.Source);
                        templateDiagnostics.Add(diagnostic);
                    }
                }

                if (templateDiagnostics.Count == 0 && template.TemplateBodyParseTree != null)
                {
                    templateDiagnostics.AddRange(Visit(template.TemplateBodyParseTree));
                }

                result.AddRange(templateDiagnostics);
            }

            return result;
        }

        public override List<Diagnostic> VisitNormalTemplateBody([NotNull] LGTemplateParser.NormalTemplateBodyContext context)
        {
            var result = new List<Diagnostic>();

            foreach (var templateStr in context.templateString())
            {
                var errorTemplateStr = templateStr.errorTemplateString();
                if (errorTemplateStr != null)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.InvalidTemplateBody, context: errorTemplateStr));
                }
                else
                {
                    result.AddRange(Visit(templateStr.normalTemplateString()));
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitStructuredTemplateBody([NotNull] LGTemplateParser.StructuredTemplateBodyContext context)
        {
            var result = new List<Diagnostic>();

            var errorName = context.structuredBodyNameLine().errorStructuredName();
            if (errorName != null)
            {
                result.Add(BuildLGDiagnostic(TemplateErrors.InvalidStrucName(errorName.GetText()), context: context.structuredBodyNameLine()));
            }

            if (context.structuredBodyEndLine() == null)
            {
                result.Add(BuildLGDiagnostic(TemplateErrors.MissingStrucEnd, context: context));
            }

            var errors = context.errorStructureLine();
            if (errors != null && errors.Length > 0)
            {
                foreach (var error in errors)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.InvalidStrucBody(error.GetText()), context: error));
                }
            }
            else
            {
                var bodys = context.structuredBodyContentLine();

                if (bodys == null || bodys.Length == 0)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.EmptyStrucContent, context: context));
                }
                else
                {
                    foreach (var body in bodys)
                    {
                        if (body.expressionInStructure() != null)
                        {
                            result.AddRange(CheckExpression(body.expressionInStructure()));
                        }
                        else
                        {
                            // KeyValueStructuredLine
                            var structureValues = body.keyValueStructureLine().keyValueStructureValue();
                            var errorPrefix = "Property '" + body.keyValueStructureLine().STRUCTURE_IDENTIFIER().GetText() + "':";
                            foreach (var structureValue in structureValues)
                            {
                                foreach (var expression in structureValue.expressionInStructure())
                                {
                                    result.AddRange(CheckExpression(expression, errorPrefix));
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context)
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
                    result.Add(BuildLGDiagnostic(TemplateErrors.InvalidWhitespaceInCondition, context: conditionNode));
                }

                if (idx == 0 && !ifExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.NotStartWithIfInCondition, DiagnosticSeverity.Warning, conditionNode));
                }

                if (idx > 0 && ifExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.MultipleIfInCondition, context: conditionNode));
                }

                if (idx == ifRules.Length - 1 && !elseExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.NotEndWithElseInCondition, DiagnosticSeverity.Warning, conditionNode));
                }

                if (idx > 0 && idx < ifRules.Length - 1 && !elseIfExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.InvalidMiddleInCondition, context: conditionNode));
                }

                // check rule should should with one and only expression
                if (!elseExpr)
                {
                    if (ifRules[idx].ifCondition().expression().Length != 1)
                    {
                        result.Add(BuildLGDiagnostic(TemplateErrors.InvalidExpressionInCondition, context: conditionNode));
                    }
                    else
                    {
                        var errorPrefix = "Condition '" + conditionNode.expression(0).GetText() + "': ";
                        result.AddRange(CheckExpression(conditionNode.expression(0), errorPrefix));
                    }
                }
                else
                {
                    if (ifRules[idx].ifCondition().expression().Length != 0)
                    {
                        result.Add(BuildLGDiagnostic(TemplateErrors.ExtraExpressionInCondition, context: conditionNode));
                    }
                }

                if (ifRules[idx].normalTemplateBody() != null)
                {
                    result.AddRange(Visit(ifRules[idx].normalTemplateBody()));
                }
                else
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.MissingTemplateBodyInCondition, context: conditionNode));
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context)
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
                    result.Add(BuildLGDiagnostic(TemplateErrors.InvalidWhitespaceInSwitchCase, context: switchCaseNode));
                }

                if (idx == 0 && !switchExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.NotStartWithSwitchInSwitchCase, context: switchCaseNode));
                }

                if (idx > 0 && switchExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.MultipleSwithStatementInSwitchCase, context: switchCaseNode));
                }

                if (idx > 0 && idx < length - 1 && !caseExpr)
                {
                    result.Add(BuildLGDiagnostic(TemplateErrors.InvalidStatementInMiddlerOfSwitchCase, context: switchCaseNode));
                }

                if (idx == length - 1 && (caseExpr || defaultExpr))
                {
                    if (caseExpr)
                    {
                        result.Add(BuildLGDiagnostic(TemplateErrors.NotEndWithDefaultInSwitchCase, DiagnosticSeverity.Warning, switchCaseNode));
                    }
                    else
                    {
                        if (length == 2)
                        {
                            result.Add(BuildLGDiagnostic(TemplateErrors.MissingCaseInSwitchCase, DiagnosticSeverity.Warning, switchCaseNode));
                        }
                    }
                }

                if (switchExpr || caseExpr)
                {
                    if (switchCaseNode.expression().Length != 1)
                    {
                        result.Add(BuildLGDiagnostic(TemplateErrors.InvalidExpressionInSwiathCase, context: switchCaseNode));
                    }
                    else
                    {
                        var errorPrefix = switchExpr ? "Switch" : "Case";
                        errorPrefix += " '" + switchCaseNode.expression(0).GetText() + "': ";
                        result.AddRange(CheckExpression(switchCaseNode.expression(0), errorPrefix));
                    }
                }
                else
                {
                    if (switchCaseNode.expression().Length != 0 || switchCaseNode.TEXT().Length != 0)
                    {
                        result.Add(BuildLGDiagnostic(TemplateErrors.ExtraExpressionInSwitchCase, context: switchCaseNode));
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
                        result.Add(BuildLGDiagnostic(TemplateErrors.MissingTemplateBodyInSwitchCase, context: switchCaseNode));
                    }
                }
            }

            return result;
        }

        public override List<Diagnostic> VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context)
        {
            var prefixErrorMsg = context.GetPrefixErrorMessage();
            var result = new List<Diagnostic>();

            foreach (var expression in context.expression())
            {
                result.AddRange(CheckExpression(expression, prefixErrorMsg));
            }

            var multiLinePrefix = context.MULTILINE_PREFIX();
            var multiLineSuffix = context.MULTILINE_SUFFIX();

            if (multiLinePrefix != null && multiLineSuffix == null)
            {
                result.Add(BuildLGDiagnostic(TemplateErrors.NoEndingInMultiline, context: context));
            }

            return result;
        }

        private List<Diagnostic> CheckExpression(ParserRuleContext expressionContext, string prefix = "")
        {
            var exp = expressionContext.GetText();
            var result = new List<Diagnostic>();
            if (!exp.EndsWith("}", StringComparison.Ordinal))
            {
                result.Add(BuildLGDiagnostic(TemplateErrors.NoCloseBracket, context: expressionContext));
            }
            else
            {
                exp = exp.TrimExpression();

                try
                {
                    ExpressionParser.Parse(exp);
                }
#pragma warning disable CA1031 // Do not catch general exception types (catch any exception and return it in the result)
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    var suffixErrorMsg = Evaluator.ConcatErrorMsg(TemplateErrors.ExpressionParseError(exp), e.Message);
                    var errorMsg = Evaluator.ConcatErrorMsg(prefix, suffixErrorMsg);

                    result.Add(BuildLGDiagnostic(errorMsg, context: expressionContext));
                    return result;
                }
            }

            return result;
        }

        private Diagnostic BuildLGDiagnostic(
            string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Error,
            ParserRuleContext context = null)
        {
            var lineOffset = _currentTemplate != null ? _currentTemplate.SourceRange.Range.Start.Line : 0;
            var templateNameInfo = string.Empty;
            if (_currentTemplate != null && _currentTemplate.Name.StartsWith(Templates.InlineTemplateIdPrefix, StringComparison.InvariantCulture))
            {
                templateNameInfo = $"[{_currentTemplate.Name}]";
            }

            var range = context == null ? new Range(1 + lineOffset, 0, 1 + lineOffset, 0) : context.ConvertToRange(lineOffset);
            return new Diagnostic(range, templateNameInfo + message, severity, _templates.Source);
        }
    }
}
