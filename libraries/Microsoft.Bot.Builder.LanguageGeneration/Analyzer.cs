using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class AnalyzerResult
    {
        public AnalyzerResult(List<string> variables = null, List<string> templateReferences = null)
        {
            this.Variables = (variables ?? new List<string>()).Distinct().ToList();
            this.TemplateReferences = (templateReferences ?? new List<string>()).Distinct().ToList();
        }

        public List<string> Variables { get; set; }

        public List<string> TemplateReferences { get; set; }

        public AnalyzerResult Union(AnalyzerResult outputItem)
        {
            this.Variables = this.Variables.Union(outputItem.Variables).ToList();
            this.TemplateReferences = this.TemplateReferences.Union(outputItem.TemplateReferences).ToList();
            return this;
        }
    }

    public class Analyzer : LGFileParserBaseVisitor<AnalyzerResult>
    {
        private readonly Dictionary<string, LGTemplate> templateMap;

        private readonly IExpressionParser _expressionParser;

        private Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        public Analyzer(List<LGTemplate> templates)
        {
            Templates = templates;
            templateMap = templates.ToDictionary(t => t.Name);
            _expressionParser = new ExpressionEngine(new GetMethodExtensions(new Evaluator(this.Templates, null)).GetMethodX);
        }

        public List<LGTemplate> Templates { get; }

        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            if (!templateMap.ContainsKey(templateName))
            {
                throw new Exception($"[{templateName}] template not found");
            }

            if (evaluationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new Exception($"Loop detected: {string.Join(" => ", evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evalution trace
            evaluationTargetStack.Push(new EvaluationTarget(templateName, null));

            // we don't exclude paratemters any more
            // because given we don't track down for templates have parameters
            // the only scenario that we are still analyzing an parameterized template is
            // this template is root template to anaylze, in this we also don't have exclude parameters
            var dependencies = Visit(templateMap[templateName].ParseTree);
            evaluationTargetStack.Pop();

            return dependencies;
        }

        public override AnalyzerResult VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateNameLine();
            if (templateNameContext.templateName().GetText().Equals(CurrentTarget().TemplateName))
            {
                if (context.templateBody() != null)
                {
                    return Visit(context.templateBody());
                }
            }

            throw new Exception("template name match failed");
        }

        public override AnalyzerResult VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        public override AnalyzerResult VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new AnalyzerResult();

            foreach (var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.Union(item);
            }

            return result;
        }

        public override AnalyzerResult VisitIfElseBody([NotNull] LGFileParser.IfElseBodyContext context)
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

        public override AnalyzerResult VisitSwitchCaseBody([NotNull] LGFileParser.SwitchCaseBodyContext context)
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

        public override AnalyzerResult VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new AnalyzerResult();
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.EXPRESSION:
                        result.Union(AnalyzeExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result.Union(AnalyzeTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result.Union(AnalyzeMultiLineText(node.GetText()));
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            evaluationTargetStack.Peek();

        /// <summary>
        /// Extract the templates ref out from an expression
        /// return only those without paramaters.
        /// </summary>
        /// <param name="exp">Expression.</param>
        /// <returns>template refs.</returns>
        private AnalyzerResult AnalyzeExpressionDirectly(Expression exp)
        {
            var result = new AnalyzerResult();
            if (exp.Type == "lgTemplate")
            {
                var templateName = (exp.Children[0] as Constant).Value.ToString();
                result.Union(new AnalyzerResult(templateReferences: new List<string>() { templateName }));

                if (exp.Children.Length == 1)
                {
                    result.Union(this.AnalyzeTemplate((exp.Children[0] as Constant).Value.ToString()));
                }
                else
                {
                    // only get template ref names
                    var templateRefNames = this.AnalyzeTemplate((exp.Children[0] as Constant).Value.ToString()).TemplateReferences;
                    result.Union(new AnalyzerResult(templateReferences: templateRefNames));

                    // analyzer other children
                    exp.Children.ToList().ForEach(x => result.Union(this.AnalyzeExpressionDirectly(x)));
                }
            }
            else
            {
                exp.Children.ToList().ForEach(x => result.Union(this.AnalyzeExpressionDirectly(x)));
            }

            return result;
        }

        private AnalyzerResult AnalyzeExpression(string exp)
        {
            var result = new AnalyzerResult();
            exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');
            var parsed = _expressionParser.Parse(exp);

            var references = parsed.References();

            result.Union(new AnalyzerResult(variables: new List<string>(references)));
            result.Union(this.AnalyzeExpressionDirectly(parsed));

            return result;
        }

        private AnalyzerResult AnalyzeTemplateRef(string exp)
        {
            var result = new AnalyzerResult();
            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');

            // Do have args
            if (argsStartPos > 0)
            {
                // Analyze all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');

                var args = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',');

                // Before we have a matural solution to analyze parameterized template, we stop digging into
                // templates with parameters, we just analyze it's args.
                // With this approach we may not get a very fine-grained result
                // but the result will still be accurate
                var templateAnalyzerResult = args.Select(arg => this.AnalyzeExpression(arg));
                var templateName = exp.Substring(0, argsStartPos);

                // add this template
                result.Union(new AnalyzerResult(templateReferences: new List<string>() { templateName }));
                templateAnalyzerResult.ToList().ForEach(t => result.Union(t));
            }
            else
            {
                result.Union(new AnalyzerResult(templateReferences: new List<string>() { exp }));

                // We analyze tempalte only if the template has no formal parameters
                // But we should analyzer template reference names for all situation
                if (this.templateMap[exp].Parameters == null || this.templateMap[exp].Parameters.Count == 0)
                {
                    result.Union(this.AnalyzeTemplate(exp));
                }
                else
                {
                    result.Union(new AnalyzerResult(templateReferences: this.AnalyzeTemplate(exp).TemplateReferences));
                }
            }

            return result;
        }

        private AnalyzerResult AnalyzeMultiLineText(string exp)
        {
            var result = new AnalyzerResult();

            // remove ``` ```
            exp = exp.Substring(3, exp.Length - 6);

            var matches = Regex.Matches(exp, @"@\{[^{}]+\}");
            foreach (Match matchItem in matches)
            {
                if (matchItem.Success)
                {
                    result.Union(AnalyzeExpression(matchItem.Value));
                }
            }

            return result;
        }
    }
}
