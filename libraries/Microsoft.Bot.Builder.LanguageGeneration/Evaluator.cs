// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Expressions.Memory;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG template Evaluator.
    /// </summary>
    public class Evaluator : LGFileParserBaseVisitor<object>
    {
        public const string LGType = "lgType";
        public static readonly Regex ExpressionRecognizeRegex = new Regex(@"(?<!\\)@{((\'[^\r\n\']*\')|(\""[^\""\r\n]*\"")|(\`(\\\`|[^\`])*\`)|([^\r\n{}'""`]))*?}", RegexOptions.Compiled);
        private const string ReExecuteSuffix = "!";
        private readonly Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="templates">Template list.</param>
        /// <param name="expressionEngine">expression engine.</param>
        public Evaluator(List<LGTemplate> templates, ExpressionEngine expressionEngine)
        {
            Templates = templates;
            TemplateMap = templates.ToDictionary(x => x.Name);

            // generate a new customized expression engine by injecting the template as functions
            ExpressionEngine = new ExpressionEngine(CustomizedEvaluatorLookup(expressionEngine.EvaluatorLookup));
        }

        /// <summary>
        /// Gets templates.
        /// </summary>
        /// <value>
        /// Templates.
        /// </value>
        public List<LGTemplate> Templates { get; }

        /// <summary>
        /// Gets expression engine.
        /// </summary>
        /// <value>
        /// Expression engine.
        /// </value>
        public ExpressionEngine ExpressionEngine { get; }

        /// <summary>
        /// Gets templateMap.
        /// </summary>
        /// <value>
        /// TemplateMap.
        /// </value>
        public Dictionary<string, LGTemplate> TemplateMap { get; }

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// </summary>
        /// <param name="inputTemplateName">template name.</param>
        /// <param name="scope">scope.</param>
        /// <returns>Evaluate result.</returns>
        public object EvaluateTemplate(string inputTemplateName, object scope)
        {
            if (!(scope is CustomizedMemory))
            {
                scope = new CustomizedMemory(SimpleObjectMemory.Wrap(scope));
            }
            
            (var reExecute, var templateName) = ParseTemplateName(inputTemplateName);

            if (!TemplateMap.ContainsKey(templateName))
            {
                throw new Exception(LGErrors.TemplateNotExist(templateName));
            }

            if (evaluationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new Exception($"{LGErrors.LoopDetected} {string.Join(" => ", evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            var templateTarget = new EvaluationTarget(templateName, scope);

            var currentEvaluateId = templateTarget.GetId();

            EvaluationTarget previousEvaluateTarget = null;
            if (evaluationTargetStack.Count != 0)
            {
                previousEvaluateTarget = evaluationTargetStack.Peek();

                if (!reExecute && previousEvaluateTarget.EvaluatedChildren.ContainsKey(currentEvaluateId))
                {
                    return previousEvaluateTarget.EvaluatedChildren[currentEvaluateId];
                }
            }

            // Using a stack to track the evaluation trace
            evaluationTargetStack.Push(templateTarget);
            var result = Visit(TemplateMap[templateName].ParseTree);
            if (previousEvaluateTarget != null)
            {
                previousEvaluateTarget.EvaluatedChildren[currentEvaluateId] = result;
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
            var typeName = context.structuredBodyNameLine().STRUCTURE_NAME().GetText();
            result[LGType] = typeName;

            var bodys = context.structuredBodyContentLine();
            foreach (var body in bodys)
            {
                var isKVPairBody = body.keyValueStructureLine() != null;
                if (isKVPairBody)
                {
                    // make it insensitive
                    var property = body.keyValueStructureLine().STRUCTURE_IDENTIFIER().GetText().ToLower();
                    var value = VisitStructureValue(body.keyValueStructureLine());
                    result[property] = JToken.FromObject(value);
                }
                else
                {
                    // When the same property exists in both the calling template as well as callee, the content in caller will trump any content in 
                    var propertyObject = JObject.FromObject(EvalExpression(body.objectStructureLine().GetText(), true));

                    // Full reference to another structured template is limited to the structured template with same type 
                    if (propertyObject[LGType] != null && propertyObject[LGType].ToString() == typeName)
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
                        result.Add(node.GetText().Escape());
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

        public object ConstructScope(string inputTemplateName, List<object> args)
        {
            var templateName = ParseTemplateName(inputTemplateName).pureTemplateName;

            if (!TemplateMap.ContainsKey(templateName))
            {
                throw new Exception(LGErrors.TemplateNotExist(templateName));
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

            if (currentScope is CustomizedMemory memory)
            {
                // inherit current memory's global scope
                return new CustomizedMemory(memory.GlobalMemory, new SimpleObjectMemory(newScope));
            }
            else
            {
                throw new Exception("Scope is not a LG customized memory");
            }
        }
        
        private object VisitStructureValue(LGFileParser.KeyValueStructureLineContext context)
        {
            var values = context.keyValueStructureValue();

            var result = new List<object>();
            foreach (var item in values)
            {
                if (item.IsPureExpression(out var text))
                {
                    result.Add(EvalExpression(text));
                }
                else
                {
                    var itemStringResult = new StringBuilder();
                    foreach (ITerminalNode node in item.children)
                    {
                        switch (node.Symbol.Type)
                        {
                            case LGFileParser.ESCAPE_CHARACTER_IN_STRUCTURE_BODY:
                                itemStringResult.Append(node.GetText().Escape());
                                break;
                            case LGFileParser.EXPRESSION_IN_STRUCTURE_BODY:
                                itemStringResult.Append(EvalExpression(node.GetText()));
                                break;
                            default:
                                itemStringResult.Append(node.GetText());
                                break;
                        }
                    }

                    result.Add(itemStringResult.ToString().Trim());
                }
            }

            return result.Count == 1 ? result[0] : result;
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
                exp = exp.TrimExpression();
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

        private object EvalExpression(string exp, bool throwOnNull = false)
        {
            exp = exp.TrimExpression();
            var (result, error) = EvalByExpressionEngine(exp, CurrentTarget().Scope);
            if (error != null)
            {
                throw new Exception(LGErrors.ErrorExpression(exp, error));
            }

            if (result == null)
            {
                if (throwOnNull)
                {
                    throw new Exception(LGErrors.NullExpression(exp));
                }
                else
                {
                    result = "null";
                }
            }

            return result;
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            evaluationTargetStack.Peek();

        private (object value, string error) EvalByExpressionEngine(string exp, object scope)
        {
            var parse = this.ExpressionEngine.Parse(exp);
            return parse.TryEvaluate(scope);
        }

        // Generate a new lookup function based on one lookup function
        private EvaluatorLookup CustomizedEvaluatorLookup(EvaluatorLookup baseLookup)
        => (string name) =>
        {
            var prebuiltPrefix = "prebuilt.";

            if (name.StartsWith(prebuiltPrefix))
            {
                return baseLookup(name.Substring(prebuiltPrefix.Length));
            }

            var templateName = ParseTemplateName(name).pureTemplateName;

            if (this.TemplateMap.ContainsKey(templateName))
            {
                return new ExpressionEvaluator(templateName, ExpressionFunctions.Apply(this.TemplateEvaluator(name)), ReturnType.Object, this.ValidTemplateReference);
            }

            const string template = "template";

            if (name.Equals(template))
            {
                return new ExpressionEvaluator(template, ExpressionFunctions.Apply(this.TemplateFunction()), ReturnType.Object, this.ValidateTemplateFunction);
            }

            const string fromFile = "fromFile";

            if (name.Equals(fromFile))
            {
                return new ExpressionEvaluator(fromFile, ExpressionFunctions.Apply(this.FromFile()), ReturnType.String, ExpressionFunctions.ValidateUnaryString);
            }

            const string activityAttachment = "ActivityAttachment";

            if (name.Equals(activityAttachment))
            {
                return new ExpressionEvaluator(
                    activityAttachment,
                    ExpressionFunctions.Apply(this.ActivityAttachment()),
                    ReturnType.Object,
                    (expr) => ExpressionFunctions.ValidateOrder(expr, null, ReturnType.Object, ReturnType.String));
            }

            const string isTemplate = "isTemplate";

            if (name.Equals(isTemplate))
            {
                return new ExpressionEvaluator(isTemplate, ExpressionFunctions.Apply(this.IsTemplate()), ReturnType.Boolean, ExpressionFunctions.ValidateUnaryString);
            }

            return baseLookup(name);
        };

        private Func<IReadOnlyList<object>, object> IsTemplate()
       => (IReadOnlyList<object> args) =>
       {
           var templateName = args[0].ToString();
           return TemplateMap.ContainsKey(templateName);
       };

        private Func<IReadOnlyList<object>, object> ActivityAttachment()
        => (IReadOnlyList<object> args) =>
        {
            return new JObject
            {
                [LGType] = "attachment",
                ["contenttype"] = args[1].ToString(),
                ["content"] = args[0] as JObject
            };
        };

        private Func<IReadOnlyList<object>, object> FromFile()
       => (IReadOnlyList<object> args) =>
       {
           var filePath = args[0].ToString().NormalizePath();

           var resourcePath = GetResourcePath(filePath);
           var stringContent = File.ReadAllText(resourcePath);

           var evalutor = new MatchEvaluator(m => EvalExpression(m.Value).ToString());
           var result = ExpressionRecognizeRegex.Replace(stringContent, evalutor);
           return result.Escape();
       };

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
                var sourcePath = template.Source.NormalizePath();
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
            ExpressionFunctions.ValidateAtLeastOne(expression);

            var children0 = expression.Children[0];

            if (children0.ReturnType != ReturnType.Object && children0.ReturnType != ReturnType.String)
            {
                throw new Exception(LGErrors.ErrorTemplateNameformat(children0.ToString()));
            }

            // Validate more if the name is string constant
            if (children0.Type == ExpressionType.Constant)
            {
                var templateName = (children0 as Constant).Value.ToString();
                CheckTemplateReference(templateName, expression.Children.Skip(1));
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
            CheckTemplateReference(expression.Type, expression.Children);
        }

        private void CheckTemplateReference(string templateName, IEnumerable<Expression> children)
        {
            if (!this.TemplateMap.ContainsKey(templateName))
            {
                throw new Exception(LGErrors.TemplateNotExist(templateName));
            }

            var expectedArgsCount = this.TemplateMap[templateName].Parameters.Count();
            var actualArgsCount = children.Count();

            if (actualArgsCount != 0 && expectedArgsCount != actualArgsCount)
            {
                throw new Exception(LGErrors.ArgumentMismatch(templateName, expectedArgsCount, actualArgsCount));
            }
        }

        private (bool reExecute, string pureTemplateName) ParseTemplateName(string templateName)
        {
            if (templateName == null)
            {
                throw new ArgumentException("template name is null.");
            }

            return templateName.EndsWith(ReExecuteSuffix) ?
                (true, templateName.Substring(0, templateName.Length - ReExecuteSuffix.Length))
                : (false, templateName);
        }
    }
}
