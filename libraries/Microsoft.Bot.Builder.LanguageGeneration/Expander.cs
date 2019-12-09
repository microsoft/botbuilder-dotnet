// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class Expander : LGFileParserBaseVisitor<List<string>>
    {
        private readonly ExpressionEngine expanderExpressionEngine;
        private readonly ExpressionEngine evaluatorExpressionEngine;
        private readonly Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        public Expander(List<LGTemplate> templates, ExpressionEngine expressionEngine)
        {
            Templates = templates;
            TemplateMap = templates.ToDictionary(x => x.Name);

            // generate a new customzied expression engine by injecting the template as functions
            this.expanderExpressionEngine = new ExpressionEngine(CustomizedEvaluatorLookup(expressionEngine.EvaluatorLookup, true));
            this.evaluatorExpressionEngine = new ExpressionEngine(CustomizedEvaluatorLookup(expressionEngine.EvaluatorLookup, false));
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
            var normalTemplateStrs = context.templateString();
            var result = new List<string>();

            foreach (var normalTemplateStr in normalTemplateStrs)
            {
                result.AddRange(Visit(normalTemplateStr.normalTemplateString()));
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

        public override List<string> VisitStructuredBody([NotNull] LGFileParser.StructuredBodyContext context)
        {
            var idToStringDict = new Dictionary<string, string>();
            var stb = context.structuredTemplateBody();
            var result = new JObject();
            var typeName = stb.structuredBodyNameLine().STRUCTURED_CONTENT().GetText().Trim();
            result[Evaluator.LGType] = typeName;
            var expandedResult = new List<JObject>
            {
                result
            };
            var bodys = stb.structuredBodyContentLine().STRUCTURED_CONTENT();
            foreach (var body in bodys)
            {
                var line = body.GetText().Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var start = line.IndexOf('=');
                if (start > 0)
                {
                    // make it insensitive
                    var property = line.Substring(0, start).Trim().ToLower();
                    var originValue = line.Substring(start + 1).Trim();

                    var valueArray = Evaluator.EscapeSeperatorRegex.Split(originValue);
                    if (valueArray.Length == 1)
                    {
                        var id = Guid.NewGuid().ToString();
                        expandedResult.ForEach(x => x[property] = id);
                        idToStringDict.Add(id, originValue);
                    }
                    else
                    {
                        var valueList = new JArray();
                        foreach (var item in valueArray)
                        {
                            var id = Guid.NewGuid().ToString();
                            valueList.Add(id);
                            idToStringDict.Add(id, item.Trim());
                        }

                        expandedResult.ForEach(x => x[property] = valueList);
                    }
                }
                else if (Evaluator.IsPureExpression(line))
                {
                    // [MyStruct
                    // Text = foo
                    // {ST2()}
                    // ]

                    // When the same property exists in both the calling template as well as callee, the content in caller will trump any content in the callee.
                    var propertyObjects = EvalExpression(line).Select(x => JObject.Parse(x)).ToList();
                    var tempResult = new List<JObject>();
                    foreach (var res in expandedResult)
                    {
                        foreach (var propertyObject in propertyObjects)
                        {
                            var tempRes = JObject.FromObject(res);

                            // Full reference to another structured template is limited to the structured template with same type 
                            if (propertyObject[Evaluator.LGType] != null && propertyObject[Evaluator.LGType].ToString() == typeName)
                            {
                                foreach (var item in propertyObject)
                                {
                                    if (tempRes.Property(item.Key) == null)
                                    {
                                        tempRes[item.Key] = item.Value;
                                    }
                                }
                            }

                            tempResult.Add(tempRes);
                        }
                    }

                    expandedResult = tempResult;
                }
            }

            var exps = expandedResult.Select(x => JsonConvert.SerializeObject(x)).ToList();
            var templateRefValues = new Dictionary<string, List<string>>();
            foreach (var idToString in idToStringDict)
            {
                // convert id text or expression to list of evaluated values
                if (idToString.Value.StartsWith("@{") && idToString.Value.EndsWith("}"))
                {
                    templateRefValues.Add(idToString.Key, this.EvalExpression(idToString.Value));
                }
                else
                {
                    templateRefValues.Add(idToString.Key, this.EvalText(idToString.Value));
                }
            }

            var finalResult = new List<string>(exps);
            foreach (var templateRefValue in templateRefValues)
            {
                var tempRes = new List<string>();
                foreach (var res in finalResult)
                {
                    foreach (var refValue in templateRefValue.Value)
                    {
                        tempRes.Add(res.Replace(templateRefValue.Key, refValue));
                    }
                }

                finalResult = tempRes;
            }

            return finalResult;
        }

        public override List<string> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<string>() { string.Empty };
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                    case LGFileParser.MULTILINE_PREFIX:
                    case LGFileParser.MULTILINE_SUFFIX:
                        break;
                    case LGFileParser.ESCAPE_CHARACTER:
                        result = StringListConcat(result, EvalEscape(node.GetText()));
                        break;
                    case LGFileParser.EXPRESSION:
                        result = StringListConcat(result, EvalExpression(node.GetText()));
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

        private List<string> EvalEscape(string exp)
        {
            var value = new List<string>();
            var commonEscapes = new List<string>() { "\\r", "\\n", "\\t" };
            if (commonEscapes.Contains(exp))
            {
                value.Add(Regex.Unescape(exp));
            }

            value.Add(exp.Substring(1));

            return value;
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

        // just don't want to write evaluationTargetStack.Peek() everywhere
        private EvaluationTarget CurrentTarget() => evaluationTargetStack.Peek();

        private (object value, string error) EvalByExpressionEngine(string exp, object scope)
        {
            var expanderExpression = this.expanderExpressionEngine.Parse(exp);
            var evaluatorExpression = this.evaluatorExpressionEngine.Parse(exp);
            var parse = ReconstructExpression(expanderExpression, evaluatorExpression);
            string error;
            object value;
            (value, error) = parse.TryEvaluate(scope);

            return (value, error);
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

        // Genearte a new lookup function based on one lookup function
        private EvaluatorLookup CustomizedEvaluatorLookup(EvaluatorLookup baseLookup, bool isExpander)
        => (string name) =>
        {
            var prebuiltPrefix = "prebuilt.";

            if (name.StartsWith(prebuiltPrefix))
            {
                return baseLookup(name.Substring(prebuiltPrefix.Length));
            }

            if (this.TemplateMap.ContainsKey(name))
            {
                if (isExpander)
                {
                    return new ExpressionEvaluator(name, BuiltInFunctions.Apply(this.TemplateExpander(name)), ReturnType.String, this.ValidTemplateReference);
                }
                else
                {
                    return new ExpressionEvaluator(name, BuiltInFunctions.Apply(this.TemplateEvaluator(name)), ReturnType.String, this.ValidTemplateReference);
                }
            }

            return baseLookup(name);
        };

        private Func<IReadOnlyList<object>, object> TemplateExpander(string templateName) =>
            (IReadOnlyList<object> args) =>
            {
                var newScope = this.ConstructScope(templateName, args.ToList());
                return this.EvaluateTemplate(templateName, newScope);
            };

        private Func<IReadOnlyList<object>, object> TemplateEvaluator(string templateName) =>
            (IReadOnlyList<object> args) =>
            {
                var newScope = this.ConstructScope(templateName, args.ToList());

                var value = this.EvaluateTemplate(templateName, newScope);
                var rd = new Random();
                return value[rd.Next(value.Count)];
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

        private Expression ReconstructExpression(Expression expanderExpression, Expression evaluatorExpression, bool foundPrebuiltFunction = false)
        {
            if (this.TemplateMap.ContainsKey(expanderExpression.Type))
            {
                if (foundPrebuiltFunction)
                {
                    return evaluatorExpression;
                }
            }
            else
            {
                foundPrebuiltFunction = true;
            }

            for (var i = 0; i < expanderExpression.Children.Count(); i++)
            {
                expanderExpression.Children[i] = ReconstructExpression(expanderExpression.Children[i], evaluatorExpression.Children[i], foundPrebuiltFunction);
            }

            return expanderExpression;
        }

        private List<string> EvalTextContainsExpression(string exp)
        {
            var templateRefValues = new Dictionary<string, List<string>>();
            var matches = Evaluator.ExpressionRecognizeRegex.Matches(exp);
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

        private List<string> EvalText(string exp)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return new List<string>() { exp };
            }

            if (Evaluator.IsPureExpression(exp))
            {
                // @{} or {} text, get object result
                return EvalExpression(exp);
            }
            else
            {
                return EvalTextContainsExpression(exp).Select(x => Regex.Unescape(x)).ToList();
            }
        }
    }
}
