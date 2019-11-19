// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class Evaluator : LGFileParserBaseVisitor<object>
    {
        public static readonly Regex ExpressionRecognizeRegex = new Regex(@"@{(((\'([^'\r\n])*?\')|(\""([^""\r\n])*?\""))|[^\r\n{}'""])*?}", RegexOptions.Compiled);
        public static readonly Regex EscapeSeperatorRegex = new Regex(@"(?<!\\)\|", RegexOptions.Compiled);
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

        public static bool IsPureExpression(string exp)
        {
            if (string.IsNullOrWhiteSpace(exp))
            {
                return false;
            }

            exp = exp.Trim();
            var expressions = ExpressionRecognizeRegex.Matches(exp);
            return expressions.Count == 1 && expressions[0].Value == exp;
        }

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

            var templateTarget = new EvaluationTarget(templateName, scope);
            var currentEvaluateId = templateTarget.GetId();

            EvaluationTarget previousEvaluateTarget = null;
            if (evaluationTargetStack.Count != 0)
            {
                previousEvaluateTarget = evaluationTargetStack.Peek();

                if (previousEvaluateTarget.EvaluatedChildren.ContainsKey(currentEvaluateId))
                {
                    return previousEvaluateTarget.EvaluatedChildren[currentEvaluateId];
                }
            }

            // Using a stack to track the evalution trace
            evaluationTargetStack.Push(templateTarget);
            var result = Visit(TemplateMap[templateName].ParseTree);
            if (previousEvaluateTarget != null)
            {
                previousEvaluateTarget.EvaluatedChildren.Add(currentEvaluateId, result);
            }

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
            var typeName = context.structuredBodyNameLine().STRUCTURED_CONTENT().GetText().Trim();
            result["$type"] = typeName;

            var bodys = context.structuredBodyContentLine().STRUCTURED_CONTENT();
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

                    var valueArray = EscapeSeperatorRegex.Split(originValue);
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
                    case LGFileParser.MULTILINE_PREFIX:
                    case LGFileParser.MULTILINE_SUFFIX:
                        break;
                    case LGFileParser.ESCAPE_CHARACTER:
                        result.Add(EvalEscape(node.GetText()));
                        break;
                    case LGFileParser.EXPRESSION:
                        result.Add(EvalExpression(node.GetText()));
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
            if (!TemplateMap.ContainsKey(templateName))
            {
                throw new Exception($"No such template {templateName}");
            }

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

        private string EvalEscape(string exp)
        {
            var commonEscapes = new List<string>() { "\\r", "\\n", "\\t" };
            if (commonEscapes.Contains(exp))
            {
                return Regex.Unescape(exp);
            }

            return exp.Substring(1);
        }

        private object EvalExpression(string exp)
        {
            exp = exp.TrimStart('@').TrimStart('{').TrimEnd('}');
            var (result, error) = EvalByExpressionEngine(exp, CurrentTarget().Scope);
            if (error != null)
            {
                throw new Exception($"Error occurs when evaluating expression {exp}: {error}");
            }

            if (result == null)
            {
                throw new Exception($"Error occurs when evaluating expression '{exp}': {exp} is evaluated to null");
            }

            return result;
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            evaluationTargetStack.Peek();

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
                var evalutor = new MatchEvaluator(m => EvalExpression(m.Value).ToString());
                return Regex.Unescape(ExpressionRecognizeRegex.Replace(exp, evalutor));
            }
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

            const string template = "template";

            if (name.Equals(template))
            {
                return new ExpressionEvaluator(template, BuiltInFunctions.Apply(this.TemplateFunction()), ReturnType.Object, this.ValidateTemplateFunction);
            }

            const string fromFile = "fromFile";

            if (name.Equals(fromFile))
            {
                return new ExpressionEvaluator(fromFile, BuiltInFunctions.Apply(this.FromFile()), ReturnType.String, this.ValidateFromFile);
            }

            const string activityAttachment = "ActivityAttachment";

            if (name.Equals(activityAttachment))
            {
                return new ExpressionEvaluator(activityAttachment, BuiltInFunctions.Apply(this.ActivityAttachment()), ReturnType.Object, this.ValidateActivityAttachment);
            }

            return baseLookup(name);
        };

        private Func<IReadOnlyList<object>, object> ActivityAttachment()
        => (IReadOnlyList<object> args) =>
        {
            return new JObject
            {
                ["$type"] = "attachment",
                ["contenttype"] = args[1].ToString(),
                ["content"] = args[0] as JObject
            };
        };

        private void ValidateActivityAttachment(Expression expression)
        {
            if (expression.Children.Length != 2)
            {
                throw new Exception("ActivityAttachment should have two parameters");
            }

            var children0 = expression.Children[0];
            if (children0.ReturnType != ReturnType.Object)
            {
                throw new Exception($"{children0} can't be used as a json file");
            }

            var children1 = expression.Children[1];
            if (children1.ReturnType != ReturnType.Object && children1.ReturnType != ReturnType.String)
            {
                throw new Exception($"{children0} can't be used as an attachment format, must be a string value");
            }
        }

        private Func<IReadOnlyList<object>, object> FromFile()
       => (IReadOnlyList<object> args) =>
       {
           var filePath = ImportResolver.NormalizePath(args[0].ToString());

           var resourcePath = GetResourcePath(filePath);
           return EvalText(File.ReadAllText(resourcePath));
       };

        private void ValidateFromFile(Expression expression)
        {
            if (expression.Children.Length != 1)
            {
                throw new Exception("fromFile should have one parameter");
            }

            var children0 = expression.Children[0];
            if (children0.ReturnType != ReturnType.Object && children0.ReturnType != ReturnType.String)
            {
                throw new Exception($"{children0} can't be used as a file path, must be a string value");
            }
        }

        private string GetResourcePath(string filePath)
        {
            string resourcePath;

            if (Path.IsPathRooted(filePath))
            {
                resourcePath = filePath;
            }
            else
            {
                var template = TemplateMap[CurrentTarget().TemplateName];
                var sourcePath = ImportResolver.NormalizePath(template.Source);
                var baseFolder = Environment.CurrentDirectory;
                if (Path.IsPathRooted(sourcePath))
                {
                    baseFolder = Path.GetDirectoryName(sourcePath);
                }

                resourcePath = Path.GetFullPath(Path.Combine(baseFolder, filePath));
            }

            return resourcePath;
        }

        // Evaluator for template(templateName, ...args) 
        // normal case we can just use templateName(...args), but template function is particularly useful when the template name is not pre-known
        private Func<IReadOnlyList<object>, object> TemplateFunction()
        => (IReadOnlyList<object> args) =>
        {
            var templateName = args[0].ToString();
            var newScope = this.ConstructScope(templateName, args.Skip(1).ToList());
            return this.EvaluateTemplate(templateName, newScope);
        };

        // Validator for template(...)
        private void ValidateTemplateFunction(Expression expression)
        {
            if (expression.Children.Length == 0)
            {
                throw new Exception("No template name is provided when calling template, expected: template(templateName, ...args) ");
            }

            var children0 = expression.Children[0];

            // Validate return type
            if (children0.ReturnType != ReturnType.Object && children0.ReturnType != ReturnType.String)
            {
                throw new Exception($"{children0} can't be used as a template name, must be a string value");
            }

            // Validate more if the name is string constant
            if (children0.Type == ExpressionType.Constant)
            {
                var templateName = (children0 as Constant).Value.ToString();
                if (!this.TemplateMap.ContainsKey(templateName))
                {
                    throw new Exception($"No such template '{templateName}' to call in {expression}");
                }

                var expectedArgsCount = this.TemplateMap[templateName].Parameters.Count();
                var actualArgsCount = expression.Children.Length - 1;

                if (actualArgsCount != 0 && expectedArgsCount != actualArgsCount)
                {
                    throw new Exception($"Arguments mismatch for template {templateName}, expect {expectedArgsCount} actual {actualArgsCount}");
                }
            }
        }

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

            if (actualArgsCount != 0 && expectedArgsCount != actualArgsCount)
            {
                throw new Exception($"arguments mismatch for template {templateName}, expect {expectedArgsCount} actual {actualArgsCount}");
            }
        }
    }
}
