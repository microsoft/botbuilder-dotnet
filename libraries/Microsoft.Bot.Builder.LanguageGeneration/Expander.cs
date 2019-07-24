using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class Expander : LGFileParserBaseVisitor<List<string>>
    {
        private readonly IGetMethod getMethodX;
        private readonly Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        public Expander(List<LGTemplate> templates, IGetMethod getMethod)
        {
            Templates = templates;
            TemplateMap = templates.ToDictionary(x => x.Name);
            getMethodX = getMethod ?? new GetExpanderMethod(this);
        }

        public List<LGTemplate> Templates { get; }

        public Dictionary<string, LGTemplate> TemplateMap { get; }

        public List<string> EvaluateTemplate(string templateName, object scope)
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

        public override List<string> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateNameLine();
            if (templateNameContext.templateName().GetText().Equals(CurrentTarget().TemplateName))
            {
                return Visit(context.templateBody());
            }

            return null;
        }

        public override List<string> VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        public override List<string> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var normalTemplateStrs = context.normalTemplateString();
            var result = new List<string>();

            foreach (var normalTemplateStr in normalTemplateStrs)
            {
                result.AddRange(Visit(normalTemplateStr));
            }

            return result;
        }

        public override List<string> VisitIfElseBody([NotNull] LGFileParser.IfElseBodyContext context)
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

        public override List<string> VisitSwitchCaseBody([NotNull] LGFileParser.SwitchCaseBodyContext context)
        {
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            var length = switchCaseNodes.Length;
            var switchExprs = switchCaseNodes[0].switchCaseStat().EXPRESSION();
            var switchExprResult = EvalExpression(switchExprs[0].GetText());
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
                var caseExprResult = EvalExpression(caseExprs[0].GetText());
                if (switchExprResult[0] == caseExprResult[0])
                {
                    return Visit(switchCaseNode.normalTemplateBody());
                }

                idx++;
            }

            return null;
        }

        public override List<string> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<string>() { string.Empty };
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.ESCAPE_CHARACTER:
                        result = StringListConcat(result, new List<string>() { EvalEscapeCharacter(node.GetText()) });
                        break;
                    case LGFileParser.EXPRESSION:
                        result = StringListConcat(result, EvalExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result = StringListConcat(result, EvalTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result = StringListConcat(result, EvalMultiLineText(node.GetText()));
                        break;
                    default:
                        result = StringListConcat(result, new List<string>() { node.GetText() });
                        break;
                }
            }

            return result;
        }

        public object ConstructScope(string templateName, List<object> args)
        {
            var parameters = TemplateMap[templateName].Parameters;

            if (args.Count == 0)
            {
                // no args to construct, inherit from current scope
                return CurrentTarget().Scope;
            }

            var newScope = parameters.Zip(args, (k, v) => new { k, v })
                                    .ToDictionary(x => x.k, x => x.v);
            return newScope;
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

        private string EvalEscapeCharacter(string exp)
        {
            var validCharactersDict = new Dictionary<string, string>
            {
                // Top four items :C# later render engine will treat them as escape characters, so the format is unchanged
                { @"\r", "\r" },
                { @"\n", "\n" },
                { @"\t", "\t" },
                { @"\\", "\\" },
                { @"\[", "[" },
                { @"\]", "]" },
                { @"\{", "{" },
                { @"\}", "}" },
            };

            return validCharactersDict[exp];
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

        private List<string> EvalExpression(string exp)
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

            if (result is IList &&
                result.GetType().IsGenericType &&
                result.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
            {
                return (List<string>)result;
            }

            return new List<string>() { result.ToString() };
        }

        private List<string> EvalTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();
            exp = exp.IndexOf('(') < 0 ? exp + "()" : exp;

            return EvalExpression(exp);
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            evaluationTargetStack.Peek();

        private List<string> EvalMultiLineText(string exp)
        {
            // remove ``` ```
            exp = exp.Substring(3, exp.Length - 6);
            var templateRefValues = new Dictionary<string, List<string>>();
            var reg = @"@\{[^{}]+\}";
            var matches = Regex.Matches(exp, reg);
            if (matches != null)
            {
                foreach (Match match in matches)
                {
                    templateRefValues.Add(match.Value, EvalExpression(match.Value));
                }
            }

            var result = new List<string>() { exp };
            foreach (var templateRefValue in templateRefValues)
            {
                var tempRes = new List<string>();
                foreach (var res in result)
                {
                    foreach (var refValue in templateRefValue.Value)
                    {
                        tempRes.Add(res.Replace(templateRefValue.Key, refValue));
                    }
                }

                result = tempRes;
            }

            return result;
        }

        private (object value, string error) EvalByExpressionEngine(string exp, object scope)
        {
            var parse = new ExpressionEngine(getMethodX.GetMethodX).Parse(exp);
            return parse.TryEvaluate(scope);
        }

        private List<string> StringListConcat(List<string> list1, List<string> list2)
        {
            var result = new List<string>();
            foreach (var item1 in list1)
            {
                foreach (var item2 in list2)
                {
                    result.Add(item1 + item2);
                }
            }

            return result;
        }
    }

    internal class GetExpanderMethod : IGetMethod
    {
        // Hold an evaluator instance to make sure all functions have access
        // This ensentially make all functions as closure
        // This is perticularly used for using templateName as lambda
        // Such as {foreach(alarms, ShowAlarm)}
        private readonly Expander _expander;

        public GetExpanderMethod(Expander expander)
        {
            _expander = expander;
        }

        public ExpressionEvaluator GetMethodX(string name)
        {
            // user can always choose to use builtin.xxx to disambiguate with template xxx
            var builtInPrefix = "builtin.";

            if (name.StartsWith(builtInPrefix))
            {
                return BuiltInFunctions.Lookup(name.Substring(builtInPrefix.Length));
            }

            // TODO: Should add verifiers and validators
            switch (name)
            {
                case "join":
                    return new ExpressionEvaluator("join", BuiltInFunctions.Apply(this.Join));
            }

            if (_expander.TemplateMap.ContainsKey(name))
            {
                return new ExpressionEvaluator($"{name}", BuiltInFunctions.Apply(this.TemplateEvaluator(name)), ReturnType.String, this.ValidTemplateReference);
            }

            return BuiltInFunctions.Lookup(name);
        }

        public Func<IReadOnlyList<object>, object> TemplateEvaluator(string templateName) =>
            (IReadOnlyList<object> args) =>
            {
                var newScope = _expander.ConstructScope(templateName, args.ToList());
                return _expander.EvaluateTemplate(templateName, newScope);
            };

        public void ValidTemplateReference(Expression expression)
        {
            var templateName = expression.Type;

            if (!_expander.TemplateMap.ContainsKey(templateName))
            {
                throw new Exception($"no such template '{templateName}' to call in {expression}");
            }

            var expectedArgsCount = _expander.TemplateMap[templateName].Parameters.Count();
            var actualArgsCount = expression.Children.Length;

            if (expectedArgsCount != actualArgsCount)
            {
                throw new Exception($"arguments mismatch for template {templateName}, expect {expectedArgsCount} actual {actualArgsCount}");
            }
        }

        public object Join(IReadOnlyList<object> parameters)
        {
            object result = null;
            if (parameters.Count == 2 &&
                BuiltInFunctions.TryParseList(parameters[0], out var p0) &&
                parameters[1] is string sep)
            {
                var p = p0.OfType<object>().Select(x => BuiltInFunctions.TryParseList(x, out var p1) ? p1[0].ToString() : x.ToString());
                result = string.Join(sep, p);
            }
            else if (parameters.Count == 3 &&
                BuiltInFunctions.TryParseList(parameters[0], out var li) &&
                parameters[1] is string sep1 &&
                parameters[2] is string sep2)
            {
                var p = li.OfType<object>().Select(x => BuiltInFunctions.TryParseList(x, out var p1) ? p1[0].ToString() : x.ToString());

                if (li.Count < 3)
                {
                    result = string.Join(sep2, p);
                }
                else
                {
                    var firstPart = string.Join(sep1, p.TakeWhile(o => o != null && o != p.LastOrDefault()));
                    result = firstPart + sep2 + p.Last().ToString();
                }
            }

            return result;
        }
    }
}
