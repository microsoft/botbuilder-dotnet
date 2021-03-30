// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG template analyzer.
    /// </summary>
    internal class Analyzer : LGTemplateParserBaseVisitor<AnalyzerResult>
    {
        private readonly Dictionary<string, Template> _templateMap;

        private readonly IExpressionParser _expressionParser;

        private readonly Stack<EvaluationTarget> _evaluationTargetStack = new Stack<EvaluationTarget>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyzer"/> class.
        /// </summary>
        /// <param name="templates">Templates.</param>
        /// <param name="opt">Options for LG. </param>
        public Analyzer(Templates templates, EvaluationOptions opt = null)
        {
            Templates = templates;
            _templateMap = templates.ToDictionary(t => t.Name);

            // create an evaluator to leverage it's customized function look up for checking
            var evaluator = new Evaluator(Templates, opt);
            _expressionParser = evaluator.ExpressionParser;
        }

        /// <summary>
        /// Gets templates.
        /// </summary>
        /// <value>
        /// Templates.
        /// </value>
        public Templates Templates { get; }

        /// <summary>
        /// Analyzes a template to get the static analyzer results. 
        /// Throws errors if certain errors detected <see cref="TemplateErrors"/>.
        /// </summary>
        /// <param name="templateName">Template name.</param>
        /// <returns>Analyze result including variables and template references.</returns>
        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            if (!_templateMap.ContainsKey(templateName))
            {
                throw new ArgumentException(TemplateErrors.TemplateNotExist(templateName));
            }

            if (_evaluationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new InvalidOperationException($"{TemplateErrors.LoopDetected} {string.Join(" => ", _evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evaluation trace
            _evaluationTargetStack.Push(new EvaluationTarget(templateName, null));

            // we don't exclude parameters any more
            // because given we don't track down for templates have parameters
            // the only scenario that we are still analyzing an parameterized template is
            // this template is root template to analyze, in this we also don't have exclude parameters
            var dependencies = Visit(_templateMap[templateName].TemplateBodyParseTree);
            _evaluationTargetStack.Pop();

            return dependencies;
        }

        /// <inheritdoc/>
        public override AnalyzerResult VisitNormalBody([NotNull] LGTemplateParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        /// <inheritdoc/>
        public override AnalyzerResult VisitNormalTemplateBody([NotNull] LGTemplateParser.NormalTemplateBodyContext context)
        {
            var result = new AnalyzerResult();

            foreach (var templateStr in context.templateString())
            {
                var item = Visit(templateStr.normalTemplateString());
                result.Union(item);
            }

            return result;
        }

        /// <inheritdoc/>
        public override AnalyzerResult VisitStructuredTemplateBody([NotNull] LGTemplateParser.StructuredTemplateBodyContext context)
        {
            var result = new AnalyzerResult();

            var bodys = context.structuredBodyContentLine();
            foreach (var body in bodys)
            {
                var isKVPairBody = body.keyValueStructureLine() != null;
                if (isKVPairBody)
                {
                    result.Union(VisitStructureValue(body.keyValueStructureLine()));
                }
                else
                {
                    result.Union(AnalyzeExpression(body.expressionInStructure().GetText()));
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override AnalyzerResult VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context)
        {
            var result = new AnalyzerResult();

            var ifRules = context.ifElseTemplateBody().ifConditionRule();
            foreach (var ifRule in ifRules)
            {
                var expression = ifRule.ifCondition().expression(0);
                if (expression != null)
                {
                    result.Union(AnalyzeExpression(expression.GetText()));
                }

                if (ifRule.normalTemplateBody() != null)
                {
                    result.Union(Visit(ifRule.normalTemplateBody()));
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override AnalyzerResult VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context)
        {
            var result = new AnalyzerResult();
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            foreach (var iterNode in switchCaseNodes)
            {
                var expression = iterNode.switchCaseStat().expression();
                if (expression.Length > 0)
                {
                    result.Union(AnalyzeExpression(expression[0].GetText()));
                }

                if (iterNode.normalTemplateBody() != null)
                {
                    result.Union(Visit(iterNode.normalTemplateBody()));
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override AnalyzerResult VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context)
        {
            var result = new AnalyzerResult();
            foreach (var expression in context.expression())
            {
                result.Union(AnalyzeExpression(expression.GetText()));
            }

            return result;
        }

        private AnalyzerResult VisitStructureValue(LGTemplateParser.KeyValueStructureLineContext context)
        {
            var values = context.keyValueStructureValue();

            var result = new AnalyzerResult();
            foreach (var item in values)
            {
                if (item.IsPureExpression())
                {
                    result.Union(AnalyzeExpression(item.expressionInStructure(0).GetText()));
                }
                else
                {
                    var expressions = item.expressionInStructure();
                    foreach (var expression in expressions)
                    {
                        result.Union(AnalyzeExpression(expression.GetText()));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extract the templates ref out from an expression
        /// return only those without parameters.
        /// </summary>
        /// <param name="exp">Expression.</param>
        /// <returns>Template refs.</returns>
        private AnalyzerResult AnalyzeExpressionDirectly(Expression exp)
        {
            var result = new AnalyzerResult();

            if (_templateMap.ContainsKey(exp.Type))
            {
                // template function
                var templateName = exp.Type;
                result.Union(new AnalyzerResult(templateReferences: new List<string>() { templateName }));

                if (_templateMap[templateName].Parameters.Count == 0)
                {
                    result.Union(this.AnalyzeTemplate(templateName));
                }
                else
                {
                    // if template has parameters, just get the template ref without variables.
                    result.Union(new AnalyzerResult(templateReferences: this.AnalyzeTemplate(templateName).TemplateReferences));
                }
            }

            if (exp.Children != null)
            {
                exp.Children.ToList().ForEach(x => result.Union(this.AnalyzeExpressionDirectly(x)));
            }

            return result;
        }

        private AnalyzerResult AnalyzeExpression(string exp)
        {
            var result = new AnalyzerResult();
            exp = exp.TrimExpression();
            var parsed = _expressionParser.Parse(exp);

            var references = parsed.References();

            result.Union(new AnalyzerResult(variables: new List<string>(references)));
            result.Union(this.AnalyzeExpressionDirectly(parsed));

            return result;
        }
    }
}
