// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using static Microsoft.Bot.Builder.LanguageGeneration.TemplatesParser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Class for working with Language Generation templates.
    /// </summary>
    /// <remarks>
    /// Templates.ParseFile(path) will load a .LG file .
    /// Templates.ParseText(text) will load language generation templates from text.
    /// </remarks>
    public class Templates : List<Template>
    {
        /// <summary>
        /// Temp Template ID prefix for inline content.
        /// </summary>
        public const string InlineTemplateIdPrefix = "__temp__";
        private readonly string _newLine = Environment.NewLine;
        private readonly Regex _newLineRegex = new Regex("(\r?\n)");
        private readonly string _namespaceKey = "@namespace";
        private readonly string _exportsKey = "@exports";

        /// <summary>
        /// Initializes a new instance of the <see cref="Templates"/> class.
        /// </summary>
        /// <param name="templates">List of Template instances.</param>
        /// <param name="imports">List of TemplateImport instances.</param>
        /// <param name="diagnostics">List of Diagnostic instances.</param>
        /// <param name="references">List of Templates instances.</param>
        /// <param name="content">Content of the current Templates instance.</param>
        /// <param name="id">Id of the current Templates instance.</param>
        /// <param name="expressionParser">ExpressionParser to parse the expressions in the conent.</param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="options">List of strings representing the options during evaluating the templates.</param>
        public Templates(
            IList<Template> templates = null,
            IList<TemplateImport> imports = null,
            IList<Diagnostic> diagnostics = null,
            IList<Templates> references = null,
            string content = null,
            string id = null,
            ExpressionParser expressionParser = null,
            ImportResolverDelegate importResolver = null,
            IList<string> options = null)
        {
            if (templates != null)
            {
                AddRange(templates);
            }

            Imports = imports ?? new List<TemplateImport>();
            Diagnostics = diagnostics ?? new List<Diagnostic>();
            References = references ?? new List<Templates>();
            Content = content ?? string.Empty;
            ImportResolver = importResolver;
            Id = id ?? string.Empty;
            ExpressionParser = expressionParser ?? new ExpressionParser();
            Options = options ?? new List<string>();
            InjectToExpressionFunction();
        }

        /// <summary>
        /// Gets get all templates from current lg file and reference lg files.
        /// </summary>
        /// <value>
        /// All templates from current lg file and reference lg files.
        /// </value>
        public IList<Template> AllTemplates => new List<Templates> { this }.Union(References).SelectMany(x => x).ToList();

        /// <summary>
        /// Gets get all diagnostics from current lg file and reference lg files.
        /// </summary>
        /// <value>
        /// All diagnostics from current lg file and reference lg files.
        /// </value>
        public IList<Diagnostic> AllDiagnostics => new List<Templates> { this }.Union(References).SelectMany(x => x.Diagnostics).ToList();

        /// <summary>
        /// Gets or sets delegate for resolving resource id of imported lg file.
        /// </summary>
        /// <value>
        /// Delegate for resolving resource id of imported lg file.
        /// </value>
        public ImportResolverDelegate ImportResolver { get; set; }

        /// <summary>
        /// Gets or sets expression parser.
        /// </summary>
        /// <value>
        /// Expression parser.
        /// </value>
        public ExpressionParser ExpressionParser { get; set; }

        /// <summary>
        /// Gets or sets import elements that this LG file contains directly.
        /// </summary>
        /// <value>
        /// Import elements that this LG file contains directly.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public IList<TemplateImport> Imports { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets all references that this LG file has from <see cref="Imports"/>.
        /// Notice: reference includes all child imports from the LG file,
        /// not only the children belong to this LG file directly.
        /// so, reference count may >= imports count. 
        /// </summary>
        /// <value>
        /// All references that this LG file has from <see cref="Imports"/>.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public IList<Templates> References { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets diagnostics.
        /// </summary>
        /// <value>
        /// Diagnostics.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public IList<Diagnostic> Diagnostics { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets LG content.
        /// </summary>
        /// <value>
        /// LG content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets id of this LG file.
        /// </summary>
        /// <value>
        /// Id of this lg source. For file, is full path.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets lG file options.
        /// </summary>
        /// <value>
        /// LG file options.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't remove the setter without breaking binary compat)
        public IList<string> Options { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets the evluation options for current LG file.
        /// </summary>
        /// <value>
        /// An EvaluationOption.
        /// </value>
        public EvaluationOptions LgOptions => new EvaluationOptions(Options);

        /// <summary>
        /// Gets the namespace to register for current LG file.
        /// </summary>
        /// <value>
        /// A string value.
        /// </value>
        public string Namespace => ExtractNameSpace(Options);

        /// <summary>
        /// Parser to turn lg content into a <see cref="LanguageGeneration.Templates"/>.
        /// </summary>
        /// <param name="filePath">Absolute path of a LG file.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">expressionEngine Expression engine for evaluating expressions.</param>
        /// <returns>new <see cref="LanguageGeneration.Templates"/> entity.</returns>
        public static Templates ParseFile(
            string filePath,
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            return TemplatesParser.ParseFile(filePath, importResolver, expressionParser).InjectToExpressionFunction();
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="LanguageGeneration.Templates"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">Id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">Expression parser engine for parsing expressions.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        public static Templates ParseText(
            string content,
            string id = "",
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null) => TemplatesParser.ParseText(content, id, importResolver, expressionParser).InjectToExpressionFunction();

        /// <summary>
        /// Evaluates a template with given name and scope.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">State visible in the evaluation.</param>
        /// <param name="opt">EvaluationOptions in evaluating a template.</param>
        /// <returns>Evaluate result.</returns>
        public object Evaluate(string templateName, object scope = null, EvaluationOptions opt = null)
        {
            CheckErrors();
            var evalOpt = opt != null ? opt.Merge(LgOptions) : LgOptions;
            var evaluator = new Evaluator(AllTemplates.ToList(), ExpressionParser, evalOpt);
            var result = evaluator.EvaluateTemplate(templateName, scope);
            if (evalOpt.LineBreakStyle == LGLineBreakStyle.Markdown && result is string str)
            {
                result = _newLineRegex.Replace(str, "$1$1");
            }

            return result;
        }

        /// <summary>
        /// Evaluates an inline template string.
        /// </summary>
        /// <param name="text">Inline string which will be evaluated.</param>
        /// <param name="scope">Scope object or JToken.</param>
        /// <param name="opt">EvaluationOptions in evaluating a template.</param>
        /// <returns>Evaluated result.</returns>
        public object EvaluateText(string text, object scope = null, EvaluationOptions opt = null)
        {
            var evalOpt = opt != null ? opt.Merge(LgOptions) : LgOptions;

            if (text == null)
            {
                throw new ArgumentException("inline string is null.");
            }

            CheckErrors();

            var inlineTemplateId = $"{InlineTemplateIdPrefix}{Guid.NewGuid():N}";

            // wrap inline string with "# name and -" to align the evaluation process
            var multiLineMark = "```";

            text = !text.Trim().StartsWith(multiLineMark, StringComparison.Ordinal) && text.Contains('\n')
                   ? $"{multiLineMark}{text}{multiLineMark}" : text;

            var newContent = $"# {inlineTemplateId} {_newLine} - {text}";

            var newLG = TemplatesParser.ParseTextWithRef(newContent, this);

            return newLG.Evaluate(inlineTemplateId, scope, evalOpt);
        }

        /// <summary>
        /// Expands a template with given name and scope.
        /// Return all possible responses instead of random one.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">State visible in the evaluation.</param>
        /// <param name="opt">EvaluationOptions in expanding a template.</param>
        /// <returns>Expanded result.</returns>
        public IList<object> ExpandTemplate(string templateName, object scope = null, EvaluationOptions opt = null)
        {
            CheckErrors();
            var evalOpt = opt ?? LgOptions;
            var expander = new Expander(AllTemplates.ToList(), ExpressionParser, evalOpt);
            return expander.ExpandTemplate(templateName, scope);
        }

        /// <summary>
        /// (experimental)
        /// Analyzes a template to get the static analyzer results including variables and template references.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <returns>Analyzer result.</returns>
        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            CheckErrors();
            var analyzer = new Analyzer(AllTemplates.ToList(), ExpressionParser);
            return analyzer.AnalyzeTemplate(templateName);
        }

        /// <summary>
        /// Updates an existing template in current Templates instance.
        /// </summary>
        /// <param name="templateName">Original template name. The only id of a template.</param>
        /// <param name="newTemplateName">New template Name.</param>
        /// <param name="parameters">New params.</param>
        /// <param name="templateBody">New template body.</param>
        /// <returns>Updated LG file.</returns>
        public Templates UpdateTemplate(string templateName, string newTemplateName, List<string> parameters, string templateBody)
        {
            var template = this.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                ClearDiagnostics();

                var templateNameLine = BuildTemplateNameLine(newTemplateName, parameters);
                var newTemplateBody = ConvertTemplateBody(templateBody);
                var content = $"{templateNameLine}{_newLine}{newTemplateBody}";

                // update content
                Content = ReplaceRangeContent(
                    Content,
                    template.SourceRange.Range.Start.Line - 1,
                    template.SourceRange.Range.End.Line - 1,
                    content);

                var updatedTemplates = new Templates(content: string.Empty, id: Id, importResolver: ImportResolver, expressionParser: ExpressionParser);
                updatedTemplates = new TemplatesTransformer(updatedTemplates).Transform(AntlrParseTemplates(content, Id));

                var originStartLine = template.SourceRange.Range.Start.Line - 1;
                AppendDiagnosticsWithOffset(updatedTemplates.Diagnostics, originStartLine);

                var newTemplate = updatedTemplates.FirstOrDefault();
                if (newTemplate != null)
                {
                    AdjustRangeForUpdateTemplate(template, newTemplate);
                    new StaticChecker(this).Check().ForEach(u => Diagnostics.Add(u));
                }
            }

            return this;
        }

        /// <summary>
        /// Adds a new template and returns the updated Templates instance.
        /// </summary>
        /// <param name="templateName">New template name.</param>
        /// <param name="parameters">New params.</param>
        /// <param name="templateBody">New template body.</param>
        /// <returns>Updated LG file.</returns>
        public Templates AddTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = this.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception(TemplateErrors.TemplateExist(templateName));
            }

            ClearDiagnostics();

            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var content = $"{templateNameLine}{_newLine}{newTemplateBody}";

            var originStartLine = GetLinesOfText(Content).Length;

            // update content
            Content = $"{Content}{_newLine}{templateNameLine}{_newLine}{newTemplateBody}";

            var newTemplates = new Templates(content: string.Empty, id: Id, importResolver: ImportResolver, expressionParser: ExpressionParser);
            newTemplates = new TemplatesTransformer(newTemplates).Transform(AntlrParseTemplates(content, Id));

            AppendDiagnosticsWithOffset(newTemplates.Diagnostics, originStartLine);

            var newTemplate = newTemplates.FirstOrDefault();
            if (newTemplate != null)
            {
                AdjustRangeForAddTemplate(newTemplate, originStartLine);
                Add(newTemplate);
                new StaticChecker(this).Check().ForEach(u => Diagnostics.Add(u));
            }

            return this;
        }

        /// <summary>
        /// Removes an existing template in current Templates instances.
        /// </summary>
        /// <param name="templateName">Which template should delete.</param>
        /// <returns>Updated LG file.</returns>
        public Templates DeleteTemplate(string templateName)
        {
            var template = this.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                ClearDiagnostics();

                var startLine = template.SourceRange.Range.Start.Line - 1;
                var stopLine = template.SourceRange.Range.End.Line - 1;
                Content = ReplaceRangeContent(Content, startLine, stopLine, null);

                AdjustRangeForDeleteTemplate(template);
                Remove(template);
                new StaticChecker(this).Check().ForEach(u => Diagnostics.Add(u));
            }

            return this;
        }

        /// <inheritdoc/>
        public override string ToString() => Content;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Templates lgFileObj))
            {
                return false;
            }

            return Id == lgFileObj.Id && Content == lgFileObj.Content;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => (Id, Content).GetHashCode();

        private Templates InjectToExpressionFunction()
        {
            var totalTempaltes = new List<Templates> { this }.Union(References);
            foreach (var curTemplates in totalTempaltes)
            {
                var globalFuncs = curTemplates.GetGlobalFunctionTable(curTemplates.Options);
                foreach (var templateName in globalFuncs)
                {
                    if (curTemplates.Any(u => u.Name == templateName))
                    {
                        var newGlobalName = $"{curTemplates.Namespace}.{templateName}";
                        Expression.Functions.Add(newGlobalName, new ExpressionEvaluator(newGlobalName, FunctionUtils.Apply(GlobalTemplateFunction(templateName)), ReturnType.Object));
                    }
                }
            }

            return this;
        }

        private void AppendDiagnosticsWithOffset(IList<Diagnostic> diagnostics, int offset)
        {
            if (diagnostics != null)
            {
                diagnostics.ToList().ForEach(u =>
                {
                    u.Range.Start.Line += offset;
                    u.Range.End.Line += offset;
                    Diagnostics.Add(u);
                });
            }
        }

        private void AdjustRangeForUpdateTemplate(Template oldTemplate, Template newTemplate)
        {
            var newRange = newTemplate.SourceRange.Range.End.Line - newTemplate.SourceRange.Range.Start.Line;
            var oldRange = oldTemplate.SourceRange.Range.End.Line - oldTemplate.SourceRange.Range.Start.Line;
            var lineOffset = newRange - oldRange;

            var hasFound = false;

            for (var i = 0; i < Count; i++)
            {
                if (hasFound)
                {
                    this[i].SourceRange.Range.Start.Line += lineOffset;
                    this[i].SourceRange.Range.End.Line += lineOffset;
                }
                else if (this[i].Name == oldTemplate.Name)
                {
                    hasFound = true;
                    newTemplate.SourceRange.Range.Start.Line = oldTemplate.SourceRange.Range.Start.Line;
                    newTemplate.SourceRange.Range.End.Line = oldTemplate.SourceRange.Range.End.Line + lineOffset;
                    this[i] = newTemplate;
                }
            }
        }

        private void AdjustRangeForAddTemplate(Template newTemplate, int lineOffset)
        {
            var lineLength = newTemplate.SourceRange.Range.End.Line - newTemplate.SourceRange.Range.Start.Line;
            newTemplate.SourceRange.Range.Start.Line = lineOffset + 1;
            newTemplate.SourceRange.Range.End.Line = lineLength + lineOffset + 1;
        }

        private void AdjustRangeForDeleteTemplate(Template oldTemplate)
        {
            var lineOffset = oldTemplate.SourceRange.Range.End.Line - oldTemplate.SourceRange.Range.Start.Line + 1;
            var hasFound = false;
            for (var i = 0; i < Count; i++)
            {
                if (hasFound)
                {
                    this[i].SourceRange.Range.Start.Line -= lineOffset;
                    this[i].SourceRange.Range.End.Line -= lineOffset;
                }
                else if (this[i].Name == oldTemplate.Name)
                {
                    hasFound = true;
                }
            }
        }

        private void ClearDiagnostics()
        {
            Diagnostics = new List<Diagnostic>();
        }

        private string ReplaceRangeContent(string originString, int startLine, int stopLine, string replaceString)
        {
            var originList = originString.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            if (startLine < 0 || startLine > stopLine || stopLine >= originList.Length)
            {
                throw new Exception("index out of range.");
            }

            var destList = new List<string>();

            destList.AddRange(originList.Take(startLine));
            
            if (replaceString != null)
            {
                destList.Add(replaceString);
            }

            destList.AddRange(originList.Skip(stopLine + 1));

            return string.Join(_newLine, destList);
        }

        private string ConvertTemplateBody(string templateBody)
        {
            var lines = GetLinesOfText(templateBody);
            var destList = lines.Select(u =>
            {
                return u.TrimStart().StartsWith("#", StringComparison.Ordinal) ? $"- {u.TrimStart()}" : u;
            });

            return string.Join(_newLine, destList);
        }

        private string[] GetLinesOfText(string text)
        {
            if (text == null)
            {
                return Array.Empty<string>();
            }

            return text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        private string BuildTemplateNameLine(string templateName, List<string> parameters)
        {
            if (parameters == null)
            {
                return $"# {templateName}";
            }
            else
            {
                return $"# {templateName}({string.Join(", ", parameters)})";
            }
        }

        private void CheckErrors()
        {
            if (AllDiagnostics != null)
            {
                var errors = AllDiagnostics.Where(u => u.Severity == DiagnosticSeverity.Error);
                if (errors.Any())
                {
                    throw new Exception(string.Join(_newLine, errors));
                }
            }
        }

        private string ExtractOptionsByKey(string nameOfKey, IList<string> options)
        {
            string result = null;
            foreach (var option in options)
            {
                if (!string.IsNullOrWhiteSpace(option) && option.Contains("="))
                {
                    var index = option.IndexOf('=');
                    var key = option.Substring(0, index).Trim().ToLowerInvariant();
                    var value = option.Substring(index + 1).Trim();
                    if (key == nameOfKey)
                    {
                        result = value;
                    }
                }
            }

            return result;
        }

        private string ExtractNameSpace(IList<string> options)
        {
            var result = ExtractOptionsByKey(_namespaceKey, options);

            if (result == null)
            {
                if (Path.IsPathRooted(Id))
                {
                    result = Path.GetFileNameWithoutExtension(Id);
                }
                else
                {
                    throw new Exception("namespace is required or the id should be an absoulte path!");
                }
            }

            return result;
        }

        private IList<string> GetGlobalFunctionTable(IList<string> options)
        {
            var result = new List<string>();
            var value = ExtractOptionsByKey(_exportsKey, options);
            if (value != null)
            {
                var templateList = value.Split(',').ToList();
                templateList.ForEach(u => result.Add(u.Trim()));
            }

            return result;
        }

        private Func<IReadOnlyList<object>, object> GlobalTemplateFunction(string templateName)
        => (IReadOnlyList<object> args) =>
        {
            var evaluator = new Evaluator(AllTemplates.ToList(), ExpressionParser, LgOptions);
            var newScope = evaluator.ConstructScope(templateName, args.ToList());
            return evaluator.EvaluateTemplate(templateName, newScope);
        };
    }
}
