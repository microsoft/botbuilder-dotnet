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
    public class Analyzer : LGFileParserBaseVisitor<List<string>>
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

        public List<string> AnalyzeTemplate(string templateName)
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
            var rawDependencies = Visit(templateMap[templateName].ParseTree);

            // we don't exclude paratemters any more
            // because given we don't track down for templates have paramters
            // the only scenario that we are still analyzing an paramterized template is
            // this template is root template to anaylze, in this we also don't have exclude paramters
            var dependencies = rawDependencies.Distinct().ToList();

            evaluationTargetStack.Pop();

            return dependencies;
        }

        public override List<string> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
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

        public override List<string> VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        public override List<string> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new List<string>();

            foreach (var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.AddRange(item);
            }

            return result;
        }

        public override List<string> VisitIfElseBody([NotNull] LGFileParser.IfElseBodyContext context)
        {
            var result = new List<string>();

            var ifRules = context.ifElseTemplateBody().ifConditionRule();
            foreach (var ifRule in ifRules)
            {
                var expression = ifRule.ifCondition().EXPRESSION(0);
                if (expression != null)
                {
                    result.AddRange(AnalyzeExpression(expression.GetText()));
                }

                if (ifRule.normalTemplateBody() != null)
                {
                    result.AddRange(Visit(ifRule.normalTemplateBody()));
                }
            }

            return result;
        }

        public override List<string> VisitSwitchCaseBody([NotNull] LGFileParser.SwitchCaseBodyContext context)
        {
            var result = new List<string>();
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            foreach (var iterNode in switchCaseNodes)
            {
                var expression = iterNode.switchCaseStat().EXPRESSION();
                if (expression.Length > 0)
                {
                    result.AddRange(AnalyzeExpression(expression[0].GetText()));
                }
                if (iterNode.normalTemplateBody() != null)
                {
                    result.AddRange(Visit(iterNode.normalTemplateBody()));
                }
            }

            return result;
        }

        public override List<string> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<string>();
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.EXPRESSION:
                        result.AddRange(AnalyzeExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result.AddRange(AnalyzeTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result.AddRange(AnalyzeMultiLineText(node.GetText()));
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
        private List<string> GetDirectTemplateRefs(Expression exp)
        {
            if (exp.Type == "lgTemplate" && exp.Children.Length == 1)
            {
                return new List<string> { (string)(exp.Children[0] as Constant).Value };
            }
            else
            {
                return exp.Children.Select(x => GetDirectTemplateRefs(x)).SelectMany(x => x).ToList();
            }
        }

        private List<string> AnalyzeExpression(string exp)
        {
            exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');
            var parsed = _expressionParser.Parse(exp);

            var references = parsed.References();

            var referencesInTemplates = GetDirectTemplateRefs(parsed)
                                            .Select(x => AnalyzeTemplate(x))
                                            .SelectMany(x => x)
                                            .ToList();

            return references.Concat(referencesInTemplates).ToList();
        }

        private List<string> AnalyzeTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');

            // Do have args
            if (argsStartPos > 0)
            {
                // Analyze all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');

                var args = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',');
                var refs = args.Select(arg => AnalyzeExpression(arg)).SelectMany(x => x).ToList();

                // Before we have a matural solution to analyze paramterized template, we stop digging into
                // templates with paramters, we just analyze it's args.
                // With this approach we may not get a very fine-grained result
                // but the result will still be accurate
                return refs;
            }
            else
            {
                return AnalyzeTemplate(exp);
            }
        }

        private List<string> AnalyzeMultiLineText(string exp)
        {
            var result = new List<string>();

            // remove ``` ```
            exp = exp.Substring(3, exp.Length - 6);

            var matches = Regex.Matches(exp, @"@\{[^{}]+\}");
            foreach (Match matchItem in matches)
            {
                if (matchItem.Success)
                {
                    result.AddRange(AnalyzeExpression(matchItem.Value));
                }
            }

            return result;
        }
    }
}
