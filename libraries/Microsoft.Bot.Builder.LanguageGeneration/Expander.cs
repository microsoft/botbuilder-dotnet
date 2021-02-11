// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdaptiveExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG template expander.
    /// </summary>
    internal class Expander : LGTemplateParserBaseVisitor<List<object>>
    {
        private const string LGType = "lgType";
        private readonly Stack<EvaluationTarget> _evaluationTargetStack = new Stack<EvaluationTarget>();
        private readonly EvaluationOptions _lgOptions;
        private readonly ExpressionParser _expressionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Expander"/> class.
        /// </summary>
        /// <param name="templates">Template list.</param>
        /// <param name="expressionParser">Given expression parser.</param>
        /// <param name="opt">Options for LG. including strictMode, replaceNull and lineBreakStyle.</param>
        public Expander(List<Template> templates, ExpressionParser expressionParser, EvaluationOptions opt = null)
        {
            Templates = templates;
            TemplateMap = templates.ToDictionary(x => x.Name);
            _lgOptions = opt;

            _expressionParser = expressionParser;

            // generate a new customized expression parser by injecting the template as functions
            ExpanderExpressionParser = new ExpressionParser(CustomizedEvaluatorLookup(expressionParser.EvaluatorLookup, true));
            EvaluatorExpressionParser = new ExpressionParser(CustomizedEvaluatorLookup(expressionParser.EvaluatorLookup, false));
        }

        /// <summary>
        /// Gets templates.
        /// </summary>
        /// <value>
        /// Templates.
        /// </value>
        public List<Template> Templates { get; }

        /// <summary>
        /// Gets expander expression parser.
        /// </summary>
        /// <value>
        /// Expression parser.
        /// </value>
        public ExpressionParser ExpanderExpressionParser { get; }

        /// <summary>
        /// Gets evaluator expression parser.
        /// </summary>
        /// <value>
        /// Expression parser.
        /// </value>
        public ExpressionParser EvaluatorExpressionParser { get; }

        /// <summary>
        /// Gets templateMap.
        /// </summary>
        /// <value>
        /// TemplateMap.
        /// </value>
        public Dictionary<string, Template> TemplateMap { get; }

        /// <summary>
        /// Expand the results of a template with given name and scope.
        /// Throws errors if certain errors detected <see cref="TemplateErrors"/>.
        /// </summary>
        /// <param name="templateName">Given template name.</param>
        /// <param name="scope">Given scope.</param>
        /// <returns>All possiable results.</returns>
        public List<object> ExpandTemplate(string templateName, object scope)
        {
            var memory = scope is CustomizedMemory scopeMemory ? scopeMemory : new CustomizedMemory(scope);

            if (!TemplateMap.ContainsKey(templateName))
            {
                throw new ArgumentException(TemplateErrors.TemplateNotExist(templateName));
            }

            var templateTarget = new EvaluationTarget(templateName, memory);

            var currentEvaluateId = templateTarget.GetId();

            if (_evaluationTargetStack.Any(e => e.GetId() == currentEvaluateId))
            {
                throw new InvalidOperationException($"{TemplateErrors.LoopDetected} {string.Join(" => ", _evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evaluation trace
            _evaluationTargetStack.Push(templateTarget);
            var expanderResult = Visit(TemplateMap[templateName].TemplateBodyParseTree);
            _evaluationTargetStack.Pop();

            var result = new List<object>();
            expanderResult.ForEach(u =>
            {
                result.Add(u);
            });
            return result;
        }

        /// <inheritdoc/>
        public override List<object> VisitNormalBody([NotNull] LGTemplateParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        /// <inheritdoc/>
        public override List<object> VisitNormalTemplateBody([NotNull] LGTemplateParser.NormalTemplateBodyContext context)
        {
            var normalTemplateStrs = context.templateString();
            var result = new List<object>();

            foreach (var normalTemplateStr in normalTemplateStrs)
            {
                result.AddRange(Visit(normalTemplateStr.normalTemplateString()));
            }

            return result;
        }

        /// <inheritdoc/>
        public override List<object> VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context)
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

        /// <inheritdoc/>
        public override List<object> VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context)
        {
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            var length = switchCaseNodes.Length;
            var switchExprs = switchCaseNodes[0].switchCaseStat().expression();
            var switchErrorPrefix = "Switch '" + switchExprs[0].GetText() + "': ";
            var switchExprResult = EvalExpression(switchExprs[0].GetText(), switchCaseNodes[0].switchCaseStat().GetText(), switchErrorPrefix);
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

                var caseExprs = switchCaseNode.switchCaseStat().expression();
                var caseErrorPrefix = "Case '" + caseExprs[0].GetText() + "': ";
                var caseExprResult = EvalExpression(caseExprs[0].GetText(), switchCaseNode.switchCaseStat().GetText(), caseErrorPrefix);
                if (switchExprResult[0] == caseExprResult[0] || (switchExprResult[0] != null && switchExprResult[0].Equals(caseExprResult[0])))
                {
                    return Visit(switchCaseNode.normalTemplateBody());
                }

                idx++;
            }

            return null;
        }

        /// <inheritdoc/>
        public override List<object> VisitStructuredBody([NotNull] LGTemplateParser.StructuredBodyContext context)
        {
            var templateRefValues = new Dictionary<string, List<object>>();
            var stb = context.structuredTemplateBody();
            var result = new JObject();
            var typeName = stb.structuredBodyNameLine().STRUCTURE_NAME().GetText();
            result[Evaluator.LGType] = typeName;
            var expandedResult = new List<JObject>
            {
                result
            };
            var bodys = stb.structuredBodyContentLine();
            foreach (var body in bodys)
            {
                var isKVPairBody = body.keyValueStructureLine() != null;
                if (isKVPairBody)
                {
                    var property = body.keyValueStructureLine().STRUCTURE_IDENTIFIER().GetText().ToLowerInvariant();
                    var value = VisitStructureValue(body.keyValueStructureLine());
                    if (value != null && value.Count > 0)
                    {
                        if (value.Count > 1)
                        {
                            var valueList = new JArray();
                            foreach (var item in value)
                            {
                                var id = Guid.NewGuid().ToString();
                                if (item.Count > 0)
                                {
                                    valueList.Add(id);
                                    templateRefValues.Add(id, item);
                                }
                                else
                                {
                                    valueList.Add(new JArray());
                                }
                            }

                            expandedResult.ForEach(x => x[property] = valueList);
                        }
                        else
                        {
                            var id = Guid.NewGuid().ToString();
                            if (value[0].Count > 0)
                            {
                                expandedResult.ForEach(x => x[property] = id);
                                templateRefValues.Add(id, value[0]);
                            }
                            else
                            {
                                expandedResult.ForEach(x => x[property] = new JArray());
                            }
                        }
                    }
                }
                else
                {
                    var propertyObjects = EvalExpression(body.expressionInStructure().GetText(), body.GetText()).Where(x => x != null).Select(x => JObject.Parse(x.ToString())).ToList();
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
                                    if (tempRes.Property(item.Key, StringComparison.Ordinal) == null)
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

            var exps = expandedResult;

            var finalResult = new List<object>(exps);
            foreach (var templateRefValue in templateRefValues)
            {
                var tempRes = new List<object>();
                foreach (var res in finalResult)
                {
                    foreach (var refValue in templateRefValue.Value)
                    {
                        tempRes.Add(res.ToString().Replace(templateRefValue.Key, refValue.ToString().Replace("\"", "\\\"")));
                    }
                }

                finalResult = tempRes;
            }

            return finalResult;
        }

        /// <inheritdoc/>
        public override List<object> VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context)
        {
            var prefixErrorMsg = context.GetPrefixErrorMessage();
            var result = new List<string>() { null };
            foreach (var child in context.children)
            {
                if (child is LGTemplateParser.ExpressionContext expression)
                {
                    result = StringListConcat(result, EvalExpression(expression.GetText(), context.GetText(), prefixErrorMsg));
                }
                else
                {
                    var node = child as ITerminalNode;
                    switch (node.Symbol.Type)
                    {
                        case LGTemplateParser.DASH:
                        case LGTemplateParser.MULTILINE_PREFIX:
                        case LGTemplateParser.MULTILINE_SUFFIX:
                            break;
                        case LGTemplateParser.ESCAPE_CHARACTER:
                            result = StringListConcat(result, new List<string>() { node.GetText().Escape() });
                            break;
                        default:
                            result = StringListConcat(result, new List<string>() { node.GetText() });
                            break;
                    }
                }
            }

            return result.Select(x => x as object).ToList();
        }

        /// <summary>
        /// Constructs the scope for mapping the values of arguments to the parameters of the template.
        /// </summary>
        /// <param name="templateName">The template name to evaluate.</param>
        /// <param name="args">Arguments to map to the template parameters.</param>
        /// <returns>
        /// An object.
        /// If the number of arguments is 0, returns the current scope.
        /// Otherwise, returns an CustomizedMemory that the mapping of the parameter name to the argument value added to the scope.
        /// </returns>
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

        private bool EvalCondition(LGTemplateParser.IfConditionContext condition)
        {
            var expression = condition.expression(0);
            if (expression == null || // no expression means it's else
                EvalExpressionInCondition(expression, condition.GetText(), "Condition '" + expression.GetText() + "':"))
            {
                return true;
            }

            return false;
        }

        private List<List<object>> VisitStructureValue(LGTemplateParser.KeyValueStructureLineContext context)
        {
            var values = context.keyValueStructureValue();

            var result = new List<List<object>>();
            foreach (var item in values)
            {
                if (item.IsPureExpression())
                {
                    result.Add(EvalExpression(item.expressionInStructure(0).GetText(), context.GetText()));
                }
                else
                {
                    var itemStringResult = new List<string>() { string.Empty };
                    foreach (var child in item.children)
                    {
                        if (child is LGTemplateParser.ExpressionInStructureContext expression)
                        {
                            var errorPrefix = "Property '" + context.STRUCTURE_IDENTIFIER().GetText() + "':";
                            itemStringResult = StringListConcat(itemStringResult, EvalExpression(expression.GetText(), context.GetText(), errorPrefix));
                        }
                        else
                        {
                            var node = child as ITerminalNode;
                            switch (node.Symbol.Type)
                            {
                                case LGTemplateParser.ESCAPE_CHARACTER_IN_STRUCTURE_BODY:
                                    itemStringResult = StringListConcat(itemStringResult, new List<string>() { node.GetText().Replace(@"\|", "|").Escape() });
                                    break;
                                default:
                                    itemStringResult = StringListConcat(itemStringResult, new List<string>() { node.GetText() });
                                    break;
                            }
                        }
                    }

                    result.Add(itemStringResult.Cast<object>().ToList());
                }
            }

            return result.Select(x => x.Select(y => y as object).ToList()).ToList();
        }

        private bool EvalExpressionInCondition(ParserRuleContext expressionContext, string contentLine, string errorPrefix = "")
        {
            var exp = expressionContext.GetText().TrimExpression();
            var (result, error) = EvalByAdaptiveExpression(exp, CurrentTarget().Scope);

            if (_lgOptions.StrictMode == true && (error != null || result == null))
            {
                var currentTemplate = CurrentTemplate();
                if (currentTemplate != null)
                {
                    _evaluationTargetStack.Pop();
                    Evaluator.CheckExpressionResult(exp, error, result, currentTemplate.Name, contentLine, errorPrefix);
                }
            }
            else if (error != null
                || result == null
                || (result is bool r1 && r1 == false)
                || (result is int r2 && r2 == 0))
            {
                return false;
            }

            return true;
        }

        private List<object> EvalExpression(string exp, string lineContent = "", string errorPrefix = "")
        {
            exp = exp.TrimExpression();
            var (result, error) = EvalByAdaptiveExpression(exp, CurrentTarget().Scope);

            if (error != null || (result == null && _lgOptions.StrictMode == true))
            {
                var currentTemplate = CurrentTemplate();
                if (currentTemplate != null)
                {
                    _evaluationTargetStack.Pop();
                    Evaluator.CheckExpressionResult(exp, error, result, currentTemplate.Name, lineContent, errorPrefix);
                }
            }

            if (result is IList &&
                result.GetType().IsGenericType &&
                result.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
            {
                var listRes = result as List<object>;

                return listRes.ToList();
            }

            return new List<object>() { result };
        }

        // just don't want to write evaluationTargetStack.Peek() everywhere
        private EvaluationTarget CurrentTarget() => _evaluationTargetStack.Peek();

        private (object value, string error) EvalByAdaptiveExpression(string exp, object scope)
        {
            var expanderExpression = this.ExpanderExpressionParser.Parse(exp);
            var evaluatorExpression = this.EvaluatorExpressionParser.Parse(exp);
            var parse = ReconstructExpression(expanderExpression, evaluatorExpression);
            string error;
            object value;
            var opt = new Options() { Locale = _lgOptions.Locale };
            opt.NullSubstitution = _lgOptions.NullSubstitution;
            (value, error) = parse.TryEvaluate(scope, opt);

            return (value, error);
        }

        private List<string> StringListConcat(ICollection list1, ICollection list2)
        {
            var result = new List<string>();
            foreach (var item1 in list1)
            {
                foreach (var item2 in list2)
                {
                    if (item1 == null && item2 == null)
                    {
                        result.Add(null);
                    }
                    else
                    {
                        result.Add(string.Concat(item1, item2));
                    }
                }
            }

            return result;
        }

        // Generate a new lookup function based on one lookup function
        private EvaluatorLookup CustomizedEvaluatorLookup(EvaluatorLookup baseLookup, bool isExpander)
        => (string name) =>
        {
            var standardFunction = baseLookup(name);

            if (standardFunction != null)
            {
                return standardFunction;
            }

            if (name.StartsWith("lg.", StringComparison.Ordinal))
            {
                name = name.Substring(3);
            }

            if (this.TemplateMap.ContainsKey(name))
            {
                if (isExpander)
                {
                    return new ExpressionEvaluator(name, FunctionUtils.Apply(this.TemplateExpander(name)), ReturnType.Object, this.ValidTemplateReference);
                }
                else
                {
                    return new ExpressionEvaluator(name, FunctionUtils.Apply(this.TemplateEvaluator(name)), ReturnType.Object, this.ValidTemplateReference);
                }
            }

            const string template = "template";

            if (name.Equals(template, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(template, FunctionUtils.Apply(this.TemplateFunction()), ReturnType.Object, this.ValidateTemplateFunction);
            }

            const string fromFile = "fromFile";

            if (name.Equals(fromFile, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(fromFile, FunctionUtils.Apply(this.FromFile()), ReturnType.String, FunctionUtils.ValidateUnaryString);
            }

            const string activityAttachment = "ActivityAttachment";

            if (name.Equals(activityAttachment, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(
                    activityAttachment,
                    FunctionUtils.Apply(this.ActivityAttachment()),
                    ReturnType.Object,
                    (expr) => FunctionUtils.ValidateOrder(expr, null, ReturnType.Object, ReturnType.String));
            }

            const string isTemplate = "isTemplate";

            if (name.Equals(isTemplate, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(isTemplate, FunctionUtils.Apply(this.IsTemplate()), ReturnType.Boolean, FunctionUtils.ValidateUnaryString);
            }

            const string expandText = "expandText";

            if (name.Equals(expandText, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(expandText, FunctionUtils.Apply(this.ExpandText()), ReturnType.Object, FunctionUtils.ValidateUnaryString);
            }

            return null;
        };

        private Func<IReadOnlyList<object>, object> ExpandText()
       => (IReadOnlyList<object> args) =>
       {
           var stringContent = args[0].ToString();
           var newScope = _evaluationTargetStack.Count == 0 ? null : CurrentTarget().Scope;
           var newTemplates = new Templates(templates: Templates, expressionParser: _expressionParser);
           return newTemplates.EvaluateText(stringContent, newScope, _lgOptions);
       };

        private Func<IReadOnlyList<object>, object> TemplateExpander(string templateName) =>
            (IReadOnlyList<object> args) =>
            {
                var newScope = this.ConstructScope(templateName, args.ToList());
                return this.ExpandTemplate(templateName, newScope);
            };

        private Func<IReadOnlyList<object>, object> TemplateEvaluator(string templateName) =>
            (IReadOnlyList<object> args) =>
            {
                var newScope = this.ConstructScope(templateName, args.ToList());

                var value = this.ExpandTemplate(templateName, newScope);
                var randomValue = CurrentTarget().Scope.RandomNext(0, value.Count);
                return value[randomValue];
            };

        // Evaluator for template(templateName, ...args) 
        // normal case we can just use templateName(...args), but template function is particularly useful when the template name is not pre-known
        private Func<IReadOnlyList<object>, object> TemplateFunction()
        => (IReadOnlyList<object> args) =>
        {
            var templateName = args[0].ToString();
            var newScope = this.ConstructScope(templateName, args.Skip(1).ToList());
            return this.ExpandTemplate(templateName, newScope);
        };

        // Validator for template(...)
        private void ValidateTemplateFunction(Expression expression)
        {
            FunctionUtils.ValidateAtLeastOne(expression);

            var children0 = expression.Children[0];

            if ((children0.ReturnType & ReturnType.Object) == 0 && (children0.ReturnType & ReturnType.String) == 0)
            {
                throw new InvalidOperationException(TemplateErrors.InvalidTemplateNameType);
            }

            // Validate more if the name is string constant
            if (children0.Type == ExpressionType.Constant)
            {
                var templateName = (children0 as Constant).Value.ToString();
                CheckTemplateReference(templateName, expression.Children.Skip(1));
            }
        }

        private void ValidTemplateReference(Expression expression)
        {
            var templateName = expression.Type;

            if (!this.TemplateMap.ContainsKey(templateName))
            {
                throw new ArgumentException(TemplateErrors.TemplateNotExist(templateName));
            }

            var expectedArgsCount = this.TemplateMap[templateName].Parameters.Count;
            var actualArgsCount = expression.Children.Length;

            if (expectedArgsCount != actualArgsCount)
            {
                throw new ArgumentException(TemplateErrors.ArgumentMismatch(templateName, expectedArgsCount, actualArgsCount));
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

            for (var i = 0; i < expanderExpression.Children.Length; i++)
            {
                expanderExpression.Children[i] = ReconstructExpression(expanderExpression.Children[i], evaluatorExpression.Children[i], foundPrebuiltFunction);
            }

            return expanderExpression;
        }

        private void CheckTemplateReference(string templateName, IEnumerable<Expression> children)
        {
            if (!this.TemplateMap.ContainsKey(templateName))
            {
                throw new ArgumentException(TemplateErrors.TemplateNotExist(templateName));
            }

            var expectedArgsCount = this.TemplateMap[templateName].Parameters.Count;
            var actualArgsCount = children.Count();

            if (actualArgsCount != 0 && expectedArgsCount != actualArgsCount)
            {
                throw new ArgumentException(TemplateErrors.ArgumentMismatch(templateName, expectedArgsCount, actualArgsCount));
            }
        }

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

           var newScope = _evaluationTargetStack.Count == 0 ? null : CurrentTarget().Scope;
           var newTemplates = new Templates(templates: Templates, expressionParser: _expressionParser);
           return newTemplates.EvaluateText(stringContent, newScope, _lgOptions);
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
                var sourcePath = CurrentTemplate().SourceRange.Source.NormalizePath();
                var baseFolder = Environment.CurrentDirectory;
                if (Path.IsPathRooted(sourcePath))
                {
                    baseFolder = Path.GetDirectoryName(sourcePath);
                }

                resourcePath = Path.GetFullPath(Path.Combine(baseFolder, filePath));
            }

            return resourcePath;
        }

        private Func<IReadOnlyList<object>, object> IsTemplate()
      => (IReadOnlyList<object> args) =>
      {
          var templateName = args[0].ToString();
          return TemplateMap.ContainsKey(templateName);
      };

        private Template CurrentTemplate()
        {
            return _evaluationTargetStack.Count > 0 ?
                    TemplateMap[CurrentTarget().TemplateName]
                    : null;
        }
    }
}
