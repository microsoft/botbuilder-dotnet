using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class Evaluator : LGFileParserBaseVisitor<object>
    {
        private readonly Regex expressionRecognizeRegex = new Regex(@"@?(?<!\\)\{.+?(?<!\\)\}", RegexOptions.Compiled);
        private readonly Regex escapeSeperatorRegex = new Regex(@"(?<!\\)\|", RegexOptions.Compiled);
        private readonly Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        public Evaluator(List<LGTemplate> templates, ExpressionEngine expressionEngine)
        {
            Templates = templates;
            TemplateMap = templates.ToDictionary(x => x.Name);

            // generate a new customzied expression engine by injecting the template as functions
            ExpressionEngine = new ExpressionEngine(CustomizedEvaluatorLookup(expressionEngine.EvaluatorLookup));
        }

        public List<LGTemplate> Templates { get; }

        public ExpressionEngine ExpressionEngine { get; }

        public Dictionary<string, LGTemplate> TemplateMap { get; }

        public object EvaluateTemplate(string templateName, object scope)
        {
            if (!TemplateMap.ContainsKey(templateName))
            {
                throw new Exception($"[{templateName}] not found");
            }

            if (evaluationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new Exception($"Loop detected: {string.Join(" => ", evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evalution trace
            evaluationTargetStack.Push(new EvaluationTarget(templateName, scope));
            var result = Visit(TemplateMap[templateName].ParseTree);
            evaluationTargetStack.Pop();

            return result;
        }

        public override object VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateNameLine();
            if (templateNameContext.templateName().GetText().Equals(CurrentTarget().TemplateName))
            {
                return Visit(context.templateBody());
            }

            return null;
        }

        public override object VisitStructuredTemplateBody([NotNull] LGFileParser.StructuredTemplateBodyContext context)
        {
            var result = new JObject();
            var typeName = context.structuredBodyNameLine().STRUCTURED_CONTENT().GetText();
            result["$type"] = typeName;

            var bodys = context.structuredBodyContentLine().STRUCTURED_CONTENT();
            foreach (var body in bodys)
            {
                var line = body.GetText().Trim();
                var start = line.IndexOf('=');
                if (start > 0)
                {
                    // make it insensitive
                    var property = line.Substring(0, start).Trim().ToLower();
                    var originValue = line.Substring(start + 1).Trim();

                    var valueArray = escapeSeperatorRegex.Split(originValue);
                    if (valueArray.Length == 1)
                    {
                        result[property] = EvalText(originValue);
                    }
                    else
                    {
                        var valueList = new JArray();
                        foreach (var item in valueArray)
                        {
                            valueList.Add(EvalText(item.Trim()));
                        }

                        result[property] = valueList;
                    }
                }
                else if (IsPureExpression(line))
                {
                    // [MyStruct
                    // Text = foo
                    // {ST2()}
                    // ]

                    // When the same property exists in both the calling template as well as callee, the content in caller will trump any content in the callee.
                    var propertyObject = JObject.FromObject(EvalExpression(line));

                    // Full reference to another structured template is limited to the structured template with same type 
                    if (propertyObject["$type"] != null && propertyObject["$type"].ToString() == typeName)
                    {
                        foreach (var item in propertyObject)
                        {
                            if (result.Property(item.Key) == null)
                            {
                                result[item.Key] = item.Value;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public override object VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        public override object VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var normalTemplateStrs = context.templateString();
            var rd = new Random();
            return Visit(normalTemplateStrs[rd.Next(normalTemplateStrs.Length)].normalTemplateString());
        }

        public override object VisitIfElseBody([NotNull] LGFileParser.IfElseBodyContext context)
        {
            var ifRules = context.ifElseTemplateBody().ifConditionRule();
            foreach (var ifRule in ifRules)
            {
                if (EvalCondition(ifRule.ifCondition()) && ifRule.normalTemplateBody() != null)
                {
                    return Visit(ifRule.normalTemplateBody());
                }
            }

            return null;
        }

        public override object VisitSwitchCaseBody([NotNull] LGFileParser.SwitchCaseBodyContext context)
        {
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            var length = switchCaseNodes.Length;
            var switchExprs = switchCaseNodes[0].switchCaseStat().EXPRESSION();
            var switchExprResult = EvalExpression(switchExprs[0].GetText()).ToString();
            var idx = 0;
            foreach (var switchCaseNode in switchCaseNodes)
            {
                if (idx == 0)
                {
                    idx++;
                    continue;   // skip the first node, which is switch statement
                }

                if (idx == length - 1 && switchCaseNode.switchCaseStat().DEFAULT() != null)
                {
                    var defaultBody = switchCaseNode.normalTemplateBody();
                    if (defaultBody != null)
                    {
                        return Visit(defaultBody);
                    }
                    else
                    {
                        return null;
                    }
                }

                var caseExprs = switchCaseNode.switchCaseStat().EXPRESSION();
                var caseExprResult = EvalExpression(caseExprs[0].GetText()).ToString();
                if (switchExprResult == caseExprResult)
                {
                    return Visit(switchCaseNode.normalTemplateBody());
                }

                idx++;
            }

            return null;
        }

        public override object VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<object>();
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.ESCAPE_CHARACTER:
                        result.Add(Regex.Unescape(node.GetText()));
                        break;
                    case LGFileParser.EXPRESSION:
                        result.Add(EvalExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result.Add(EvalTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result.Add(EvalMultiLineText(node.GetText()));
                        break;
                    default:
                        result.Add(node.GetText());
                        break;
                }
            }

            if (result.Count == 1 && !(result[0] is string))
            {
                return result[0];
            }

            return string.Join(string.Empty, result);
        }

        public object ConstructScope(string templateName, List<object> args)
        {
            var parameters = TemplateMap[templateName].Parameters;
            var currentScope = CurrentTarget().Scope;

            if (args.Count == 0)
            {
                // no args to construct, inherit from current scope
                return currentScope;
            }

            var newScope = parameters.Zip(args, (k, v) => new { k, v })
                                    .ToDictionary(x => x.k, x => x.v);

            if (currentScope is CustomizedMemoryScope cms)
            {
                // if current scope is already customized, inherit it's global scope
                return new CustomizedMemoryScope(newScope, cms.GlobalScope);
            }
            else
            {
                return new CustomizedMemoryScope(newScope, currentScope);
            }
            
        }

        private bool EvalCondition(LGFileParser.IfConditionContext condition)
        {
            var expression = condition.EXPRESSION(0);
            if (expression == null || // no expression means it's else
                EvalExpressionInCondition(expression.GetText()))
            {
                return true;
            }

            return false;
        }

        private bool EvalExpressionInCondition(string exp)
        {
            try
            {
                exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');
                var (result, error) = EvalByExpressionEngine(exp, CurrentTarget().Scope);

                if (error != null
                    || result == null
                    || (result is bool r1 && r1 == false)
                    || (result is int r2 && r2 == 0))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Expression {exp} evaled as false due to exception");
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private object EvalExpression(string exp)
        {
            exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');
            var (result, error) = EvalByExpressionEngine(exp, CurrentTarget().Scope);
            if (error != null)
            {
                throw new Exception($"Error occurs when evaluating expression ${exp}: {error}");
            }

            if (result == null)
            {
                throw new Exception($"Error occurs when evaluating expression '{exp}': {exp} is evaluated to null");
            }

            return result;
        }

        private object EvalTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();
            if (exp.IndexOf('(') < 0)
            {
                if (TemplateMap.ContainsKey(exp))
                {
                    exp = exp + "(" + string.Join(",", TemplateMap[exp].Parameters) + ")";
                }
                else
                {
                    exp = exp + "()";
                }
            }

            return EvalExpression(exp);
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            evaluationTargetStack.Peek();

        private string EvalMultiLineText(string exp)
        {
            // remove ``` ```
            exp = exp.Substring(3, exp.Length - 6);
            return EvalTextContainsExpression(exp);
        }

        private string EvalTextContainsExpression(string exp)
        {
            var evalutor = new MatchEvaluator(m => EvalExpression(m.Value).ToString());
            return expressionRecognizeRegex.Replace(exp, evalutor);
        }

        private JToken EvalText(string exp)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return exp;
            }

            if (IsPureExpression(exp))
            {
                // @{} or {} text, get object result
                return JToken.FromObject(EvalExpression(exp));
            }
            else
            {
                return Regex.Unescape(EvalTextContainsExpression(exp));
            }
        }

        private bool IsPureExpression(string exp)
        {
            if (string.IsNullOrWhiteSpace(exp))
            {
                return false;
            }

            exp = exp.Trim();
            var expressions = expressionRecognizeRegex.Matches(exp);
            return expressions.Count == 1 && expressions[0].Value == exp;
        }

        private (object value, string error) EvalByExpressionEngine(string exp, object scope)
        {
            var parse = this.ExpressionEngine.Parse(exp);
            return parse.TryEvaluate(scope);
        }

        // Genearte a new lookup function based on one lookup function
        private EvaluatorLookup CustomizedEvaluatorLookup(EvaluatorLookup baseLookup)
        => (string name) =>
        {
            var prebuiltPrefix = "prebuilt.";

            if (name.StartsWith(prebuiltPrefix))
            {
                return baseLookup(name.Substring(prebuiltPrefix.Length));
            }

            if (this.TemplateMap.ContainsKey(name))
            {
                return new ExpressionEvaluator(name, BuiltInFunctions.Apply(this.TemplateEvaluator(name)), ReturnType.Object, this.ValidTemplateReference);
            }

            return baseLookup(name);
        };

        private Func<IReadOnlyList<object>, object> TemplateEvaluator(string templateName)
        => (IReadOnlyList<object> args) =>
        {
            var newScope = this.ConstructScope(templateName, args.ToList());
            return this.EvaluateTemplate(templateName, newScope);
        };

        private void ValidTemplateReference(Expression expression)
        {
            var templateName = expression.Type;

            if (!this.TemplateMap.ContainsKey(templateName))
            {
                throw new Exception($"no such template '{templateName}' to call in {expression}");
            }

            var expectedArgsCount = this.TemplateMap[templateName].Parameters.Count();
            var actualArgsCount = expression.Children.Length;

            if (expectedArgsCount != actualArgsCount)
            {
                throw new Exception($"arguments mismatch for template {templateName}, expect {expectedArgsCount} actual {actualArgsCount}");
            }
        }
    }
}
