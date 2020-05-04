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
    public class Analyzer : LGTemplateParserBaseVisitor<AnalyzerResult>
    {
        private readonly Dictionary<string, Template> templateMap;

        private readonly IExpressionParser _expressionParser;

        private readonly Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyzer"/> class.
        /// </summary>
        /// <param name="templates">Template list.</param>
        /// <param name="expressionParser">Expression parser.</param>
        public Analyzer(List<Template> templates, ExpressionParser expressionParser)
        {
            Templates = templates;
            templateMap = templates.ToDictionary(t => t.Name);

            // create an evaluator to leverage it's customized function look up for checking
            var evaluator = new Evaluator(Templates, expressionParser);
            this._expressionParser = evaluator.ExpressionParser;
        }

        /// <summary>
        /// Gets templates.
        /// </summary>
        /// <value>
        /// Templates.
        /// </value>
        public List<Template> Templates { get; }

        /// <summary>
        /// Analyzer a template to get the static analyzer results.
        /// </summary>
        /// <param name="templateName">Template name.</param>
        /// <returns>Analyze result including variables and template references.</returns>
        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            if (!templateMap.ContainsKey(templateName))
            {
                throw new Exception(TemplateErrors.TemplateNotExist(templateName));
            }

            if (evaluationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new Exception($"{TemplateErrors.LoopDetected} {string.Join(" => ", evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evaluation trace
            evaluationTargetStack.Push(new EvaluationTarget(templateName, null));

            // we don't exclude parameters any more
            // because given we don't track down for templates have parameters
            // the only scenario that we are still analyzing an parameterized template is
            // this template is root template to analyze, in this we also don't have exclude parameters
            var dependencies = Visit(templateMap[templateName].TemplateBodyParseTree);
            evaluationTargetStack.Pop();

            return dependencies;
        }

        public override AnalyzerResult VisitNormalBody([NotNull] LGTemplateParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

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
                    result.Union(AnalyzeExpression(body.objectStructureLine().GetText()));
                }
            }

            return result;
        }

        public override AnalyzerResult VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context)
        {
            var result = new AnalyzerResult();

            var ifRules = context.ifElseTemplateBody().ifConditionRule();
            foreach (var ifRule in ifRules)
            {
                var expression = ifRule.ifCondition().EXPRESSION(0);
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

        public override AnalyzerResult VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context)
        {
            var result = new AnalyzerResult();
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            foreach (var iterNode in switchCaseNodes)
            {
                var expression = iterNode.switchCaseStat().EXPRESSION();
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

        public override AnalyzerResult VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context)
        {
            var result = new AnalyzerResult();
            foreach (var expression in context.EXPRESSION())
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
                if (item.IsPureExpression(out var text))
                {
                    result.Union(AnalyzeExpression(text));
                }
                else
                {
                    var expressions = item.EXPRESSION_IN_STRUCTURE_BODY();
                    foreach (var expression in expressions)
                    {
                        result.Union(AnalyzeExpression(expression.GetText()));
                    }
                }
            }

            return result;
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            evaluationTargetStack.Peek();

        /// <summary>
        /// Extract the templates ref out from an expression
        /// return only those without parameters.
        /// </summary>
        /// <param name="exp">Expression.</param>
        /// <returns>Template refs.</returns>
        private AnalyzerResult AnalyzeExpressionDirectly(Expression exp)
        {
            var result = new AnalyzerResult();

            if (templateMap.ContainsKey(exp.Type))
            {
                // template function
                var templateName = exp.Type;
                result.Union(new AnalyzerResult(templateReferences: new List<string>() { templateName }));

                if (templateMap[templateName].Parameters.Count == 0)
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
