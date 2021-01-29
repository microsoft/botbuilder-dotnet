// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG template Evaluator.
    /// </summary>
    internal class Evaluator : LGTemplateParserBaseVisitor<object>
    {
        internal const string LGType = "lgType";

        private const string ReExecuteSuffix = "!";

        private static readonly ConcurrentDictionary<string, object> _cachedResult = new ConcurrentDictionary<string, object>();
        private readonly Stack<EvaluationTarget> _evaluationTargetStack = new Stack<EvaluationTarget>();
        private readonly EvaluationOptions _lgOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="templates">Template list.</param>
        /// <param name="expressionParser">Expression parser.</param>
        /// <param name="opt">Options for LG. </param>
        public Evaluator(List<Template> templates, ExpressionParser expressionParser, EvaluationOptions opt = null)
        {
            Templates = templates;
            TemplateMap = templates.ToDictionary(x => x.Name);
            _lgOptions = opt;
            _cachedResult.Clear();

            // generate a new customized expression parser by injecting the template as functions
            ExpressionParser = new ExpressionParser(CustomizedEvaluatorLookup(expressionParser.EvaluatorLookup));
        }

        internal enum FileFormat
        {
            /// <summary>
            /// Get the evaluated result from the <see cref="FileFormat.Raw"/> Result.
            /// </summary>
            Evaluated,

            /// <summary>
            /// Get raw text content of the file.
            /// </summary>
            Raw,

            /// <summary>
            /// Get binary result from the file.
            /// </summary>
            Binary
        }

        /// <summary>
        /// Gets templates.
        /// </summary>
        /// <value>
        /// Templates.
        /// </value>
        public List<Template> Templates { get; }

        /// <summary>
        /// Gets expression parser.
        /// </summary>
        /// <value>
        /// Expression parser.
        /// </value>
        public ExpressionParser ExpressionParser { get; }

        /// <summary>
        /// Gets templateMap.
        /// </summary>
        /// <value>
        /// TemplateMap.
        /// </value>
        public Dictionary<string, Template> TemplateMap { get; }

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// Throws errors if certain errors detected <see cref="TemplateErrors"/>.
        /// </summary>
        /// <param name="inputTemplateName">Template name.</param>
        /// <param name="scope">Scope.</param>
        /// <returns>Evaluate result.</returns>
        public object EvaluateTemplate(string inputTemplateName, object scope)
        {
            var memory = scope is CustomizedMemory scopeMemory ? scopeMemory : new CustomizedMemory(scope);

            (var reExecute, var templateName) = ParseTemplateName(inputTemplateName);

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

            object result = null;
            var hasResult = false;
            if (!reExecute)
            {
                if (_lgOptions.CacheScope == LGCacheScope.Global)
                {
                    if (_cachedResult.ContainsKey(currentEvaluateId))
                    {
                        result = _cachedResult[currentEvaluateId];
                        hasResult = true;
                    }
                }
                else if (_lgOptions.CacheScope == null || _lgOptions.CacheScope == LGCacheScope.Local)
                {
                    EvaluationTarget previousEvaluateTarget = null;
                    if (CurrentTemplate() != null)
                    {
                        previousEvaluateTarget = CurrentTarget();

                        if (previousEvaluateTarget.CachedEvaluatedChildren.ContainsKey(currentEvaluateId))
                        {
                            result = previousEvaluateTarget.CachedEvaluatedChildren[currentEvaluateId];
                            hasResult = true;
                        }
                    }
                }
            }

            if (!hasResult)
            {
                _evaluationTargetStack.Push(templateTarget);

                var currentTemplate = CurrentTemplate();
                _lgOptions.OnEvent?.Invoke(currentTemplate, new BeginTemplateEvaluationArgs { Source = currentTemplate.SourceRange.Source, TemplateName = templateName });

                result = Visit(TemplateMap[templateName].TemplateBodyParseTree);
                
                if (_lgOptions.OnEvent != null)
                {
                    var text = $"Evaluate template [{templateName}] get result: {result}";
                    _lgOptions.OnEvent(currentTemplate, new MessageArgs { Source = currentTemplate.SourceRange.Source, Text = text });
                }

                _evaluationTargetStack.Pop();

                if (!reExecute)
                {
                    if (_lgOptions.CacheScope == LGCacheScope.Global)
                    {
                        _cachedResult[currentEvaluateId] = result;
                    }
                    else if (_lgOptions.CacheScope == null || _lgOptions.CacheScope == LGCacheScope.Local)
                    {
                        if (_evaluationTargetStack.Count > 0)
                        {
                            _evaluationTargetStack.Peek().CachedEvaluatedChildren[currentEvaluateId] = result;
                        }
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override object VisitStructuredTemplateBody([NotNull] LGTemplateParser.StructuredTemplateBodyContext context)
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
                    var property = body.keyValueStructureLine().STRUCTURE_IDENTIFIER().GetText().ToLowerInvariant();
                    var value = VisitStructureValue(body.keyValueStructureLine());
                    result[property] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
                }
                else
                {
                    // When the same property exists in both the calling template as well as callee, the content in caller will trump any content in 
                    var value = EvalExpression(body.expressionInStructure().GetText(), body.expressionInStructure(), body.GetText());
                    if (value != null)
                    {
                        var propertyObject = JObject.FromObject(value);

                        // Full reference to another structured template is limited to the structured template with same type 
                        if (propertyObject[LGType] != null && propertyObject[LGType].ToString() == typeName)
                        {
                            foreach (var item in propertyObject)
                            {
                                if (result.Property(item.Key, StringComparison.Ordinal) == null)
                                {
                                    result[item.Key] = item.Value;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override object VisitNormalBody([NotNull] LGTemplateParser.NormalBodyContext context) => Visit(context.normalTemplateBody());

        /// <inheritdoc/>
        public override object VisitNormalTemplateBody([NotNull] LGTemplateParser.NormalTemplateBodyContext context)
        {
            var normalTemplateStrs = context.templateString();
            var randomValue = CurrentTarget().Scope.RandomNext(0, normalTemplateStrs.Length);
            return Visit(normalTemplateStrs[randomValue].normalTemplateString());
        }

        /// <inheritdoc/>
        public override object VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context)
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
        public override object VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context)
        {
            var switchCaseNodes = context.switchCaseTemplateBody().switchCaseRule();
            var length = switchCaseNodes.Length;
            var switchExprs = switchCaseNodes[0].switchCaseStat().expression();
            var switchErrorPrefix = "Switch '" + switchExprs[0].GetText() + "': ";
            var switchExprResult = EvalExpression(switchExprs[0].GetText(), switchExprs[0], switchCaseNodes[0].switchCaseStat().GetText(), switchErrorPrefix);
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
                var caseExprResult = EvalExpression(caseExprs[0].GetText(), caseExprs[0], switchCaseNode.switchCaseStat().GetText(), caseErrorPrefix);
                if (switchExprResult == caseExprResult || (switchExprResult != null && switchExprResult.Equals(caseExprResult)))
                {
                    return Visit(switchCaseNode.normalTemplateBody());
                }

                idx++;
            }

            return null;
        }

        /// <inheritdoc/>
        public override object VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context)
        {
            var prefixErrorMsg = context.GetPrefixErrorMessage();
            var result = new List<object>();
            foreach (var child in context.children)
            {
                if (child is LGTemplateParser.ExpressionContext expression)
                {
                    result.Add(EvalExpression(expression.GetText(), expression, context.GetText(), prefixErrorMsg));
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
                            result.Add(node.GetText().Escape());
                            break;
                        default:
                            result.Add(node.GetText());
                            break;
                    }
                }
            }

            if (result.Count == 1 && !(result[0] is string))
            {
                return result[0];
            }

            return string.Join(string.Empty, result);
        }

        /// <summary>
        /// Constructs the scope for mapping the values of arguments to the parameters of the template.
        /// Throws errors if certain errors detected <see cref="TemplateErrors"/>.
        /// </summary>
        /// <param name="inputTemplateName">Template name to evaluate.</param>
        /// <param name="args">Arguments to map to the template parameters.</param>
        /// <returns>
        /// An object. 
        /// If the number of arguments is 0, returns the current scope.
        /// Otherwise, returns an CustomizedMemory that the mapping of the parameter name to the argument value added to the scope.
        /// </returns>
        public object ConstructScope(string inputTemplateName, List<object> args)
        {
            var templateName = ParseTemplateName(inputTemplateName).pureTemplateName;

            if (!TemplateMap.ContainsKey(templateName))
            {
                throw new ArgumentException(TemplateErrors.TemplateNotExist(templateName));
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

            var memory = currentScope as CustomizedMemory;
            if (memory == null)
            {
                throw new InvalidOperationException(TemplateErrors.InvalidMemory);
            }

            // inherit current memory's global scope
            return new CustomizedMemory(memory.GlobalMemory, new SimpleObjectMemory(newScope));
        }

        internal static string ConcatErrorMsg(string firstError, string secondError)
        {
            string errorMsg;
            if (string.IsNullOrEmpty(firstError))
            {
                errorMsg = secondError;
            }
            else if (string.IsNullOrEmpty(secondError))
            {
                errorMsg = firstError;
            }
            else
            {
                errorMsg = firstError + " " + secondError;
            }

            return errorMsg;
        }

        internal static void CheckExpressionResult(string exp, string error, object result, string templateName, string lineContent = "", string errorPrefix = "")
        {
            var errorMsg = string.Empty;

            var childErrorMsg = string.Empty;
            if (error != null)
            {
                childErrorMsg = ConcatErrorMsg(childErrorMsg, error);
            }
            else if (result == null)
            {
                childErrorMsg = ConcatErrorMsg(childErrorMsg, TemplateErrors.NullExpression(exp));
            }

            if (!string.IsNullOrWhiteSpace(lineContent))
            {
                errorMsg = ConcatErrorMsg(errorMsg, TemplateErrors.ErrorExpression(lineContent, templateName, errorPrefix));
            }

            throw new InvalidOperationException(ConcatErrorMsg(childErrorMsg, errorMsg));
        }

        private object VisitStructureValue(LGTemplateParser.KeyValueStructureLineContext context)
        {
            var values = context.keyValueStructureValue();

            var result = new List<object>();
            foreach (var item in values)
            {
                if (item.IsPureExpression())
                {
                    result.Add(EvalExpression(item.expressionInStructure(0).GetText(), item.expressionInStructure(0), context.GetText()));
                }
                else
                {
                    var itemStringResult = new StringBuilder();
                    foreach (var child in item.children)
                    {
                        if (child is LGTemplateParser.ExpressionInStructureContext expression)
                        {
                            var errorPrefix = "Property '" + context.STRUCTURE_IDENTIFIER().GetText() + "':";
                            itemStringResult.Append(EvalExpression(expression.GetText(), expression, context.GetText(), errorPrefix));
                        }
                        else
                        {
                            var node = child as ITerminalNode;
                            switch (node.Symbol.Type)
                            {
                                case LGTemplateParser.ESCAPE_CHARACTER_IN_STRUCTURE_BODY:
                                    itemStringResult.Append(node.GetText().Replace(@"\|", "|").Escape());
                                    break;
                                default:
                                    itemStringResult.Append(node.GetText());
                                    break;
                            }
                        }
                    }

                    result.Add(itemStringResult.ToString().Trim());
                }
            }

            return result.Count == 1 ? result[0] : result;
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

        private bool EvalExpressionInCondition(ParserRuleContext expressionContext, string contentLine, string errorPrefix = "")
        {
            var exp = expressionContext.GetText().TrimExpression();
            var (result, error) = EvalByAdaptiveExpression(exp, CurrentTarget().Scope, expressionContext);

            if (_lgOptions.StrictMode == true && (error != null || result == null))
            {
                var currentTemplate = CurrentTemplate();
                if (currentTemplate != null)
                {
                    _evaluationTargetStack.Pop();
                    CheckExpressionResult(exp, error, result, currentTemplate.Name, contentLine, errorPrefix);
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

        private object EvalExpression(string exp, ParserRuleContext expressionContext = null, string lineContent = "", string errorPrefix = "")
        {
            exp = exp.TrimExpression();
            var (result, error) = EvalByAdaptiveExpression(exp, CurrentTarget().Scope, expressionContext);

            if (error != null || (result == null && _lgOptions.StrictMode == true))
            {
                var currentTemplate = CurrentTemplate();
                if (currentTemplate != null)
                {
                    _evaluationTargetStack.Pop();
                    CheckExpressionResult(exp, error, result, currentTemplate.Name, lineContent, errorPrefix);
                }
            }

            return result;
        }

        private EvaluationTarget CurrentTarget() =>

            // just don't want to write evaluationTargetStack.Peek() everywhere
            _evaluationTargetStack.Peek();

        private (object value, string error) EvalByAdaptiveExpression(string exp, object scope, ParserRuleContext expressionContext)
        {
            var currentTemplate = CurrentTemplate();
            if (currentTemplate != null)
            {
                var source = currentTemplate.SourceRange.Source;
                if (expressionContext != null && _lgOptions.OnEvent != null)
                {
                    var lineOffset = currentTemplate.SourceRange.Range.Start.Line;
                    var sourceRange = new SourceRange(expressionContext, source, lineOffset);
                    var expressionRef = new ExpressionRef(exp, sourceRange);

                    var expression = currentTemplate.Expressions.FirstOrDefault(u => u.GetId() == expressionRef.GetId());
                    if (expression != null)
                    {
                        _lgOptions.OnEvent(expression, new BeginExpressionEvaluationArgs { Source = source, Expression = exp });
                    }
                }
            }

            var parse = this.ExpressionParser.Parse(exp);
            var opt = new Options
            {
                Locale = _lgOptions.Locale,
                NullSubstitution = _lgOptions.NullSubstitution
            };
            var result = parse.TryEvaluate(scope, opt);

            if (currentTemplate != null && _lgOptions.OnEvent != null)
            {
                string text;
                if (string.IsNullOrEmpty(result.error))
                {
                    text = $"Evaluate expression '{exp}' get result: {result.value}";
                }
                else
                {
                    text = $"Evaluate expression '{exp}' get error: {result.error}";
                }

                _lgOptions.OnEvent(currentTemplate, new MessageArgs { Source = currentTemplate.SourceRange.Source, Text = text });
            }

            return result;
        }

        // Generate a new lookup function based on one lookup function
        private EvaluatorLookup CustomizedEvaluatorLookup(EvaluatorLookup baseLookup)
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

            var templateName = ParseTemplateName(name).pureTemplateName;

            if (this.TemplateMap.ContainsKey(templateName))
            {
                return new ExpressionEvaluator(templateName, FunctionUtils.Apply(this.TemplateEvaluator(name)), ReturnType.Object, this.ValidTemplateReference);
            }

            const string template = "template";

            if (name.Equals(template, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(template, FunctionUtils.Apply(this.TemplateFunction()), ReturnType.Object, this.ValidateTemplateFunction);
            }

            const string fromFile = "fromFile";

            if (name.Equals(fromFile, StringComparison.Ordinal))
            {
                return new ExpressionEvaluator(fromFile, FunctionUtils.Apply(FromFile()), ReturnType.String, ValidateFromFile);
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
           var newTemplates = new Templates(templates: Templates, expressionParser: ExpressionParser);
           return newTemplates.EvaluateText(stringContent, newScope, _lgOptions);
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
           var format = FileFormat.Evaluated;
           if (args.Count > 1)
           {
               foreach (FileFormat item in Enum.GetValues(typeof(FileFormat)))
               {
                   if (args[1].ToString().Equals(item.ToString(), StringComparison.OrdinalIgnoreCase))
                   {
                       format = item;
                       break;
                   }
               }
           }

           object result;
           if (format == FileFormat.Binary)
           {
               result = File.ReadAllBytes(resourcePath);
           }
           else if (format == FileFormat.Raw)
           {
               result = File.ReadAllText(resourcePath);
           }
           else
           {
               var stringContent = File.ReadAllText(resourcePath);
               var newScope = _evaluationTargetStack.Count == 0 ? null : CurrentTarget().Scope;
               var newTemplates = new Templates(templates: Templates, expressionParser: ExpressionParser);
               result = newTemplates.EvaluateText(stringContent, newScope, _lgOptions);
           }

           return result;
       };

        private void ValidateFromFile(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.String);
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
                throw new ArgumentException(TemplateErrors.TemplateNotExist(templateName));
            }

            var expectedArgsCount = this.TemplateMap[templateName].Parameters.Count;
            var actualArgsCount = children.Count();

            if (actualArgsCount != 0 && expectedArgsCount != actualArgsCount)
            {
                throw new ArgumentException(TemplateErrors.ArgumentMismatch(templateName, expectedArgsCount, actualArgsCount));
            }
        }

        private (bool reExecute, string pureTemplateName) ParseTemplateName(string templateName)
        {
            if (templateName == null)
            {
                throw new ArgumentException("template name is null.");
            }

            return templateName.EndsWith(ReExecuteSuffix, StringComparison.Ordinal) ?
                (true, templateName.Substring(0, templateName.Length - ReExecuteSuffix.Length))
                : (false, templateName);
        }

        private Template CurrentTemplate()
        {
            return _evaluationTargetStack.Count > 0 ?
                    TemplateMap[CurrentTarget().TemplateName]
                    : null;
        }
    }
}
