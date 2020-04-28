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
        private readonly string newLine = Environment.NewLine;

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
                this.AddRange(templates);
            }

            Imports = imports ?? new List<TemplateImport>();
            Diagnostics = diagnostics ?? new List<Diagnostic>();
            References = references ?? new List<Templates>();
            Content = content ?? string.Empty;
            ImportResolver = importResolver;
            Id = id ?? string.Empty;
            ExpressionParser = expressionParser ?? new ExpressionParser();
            Options = options ?? new List<string>();
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
        public IList<TemplateImport> Imports { get; set; }

        /// <summary>
        /// Gets or sets all references that this LG file has from <see cref="Imports"/>.
        /// Notice: reference includes all child imports from the LG file,
        /// not only the children belong to this LG file directly.
        /// so, reference count may >= imports count. 
        /// </summary>
        /// <value>
        /// All references that this LG file has from <see cref="Imports"/>.
        /// </value>
        public IList<Templates> References { get; set; }

        /// <summary>
        /// Gets or sets diagnostics.
        /// </summary>
        /// <value>
        /// Diagnostics.
        /// </value>
        public IList<Diagnostic> Diagnostics { get; set; }

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
        public IList<string> Options { get; set; }

        /// <summary>
        /// Gets the evluation options for current LG file.
        /// </summary>
        /// <value>
        /// An EvaluationOption.
        /// </value>
        public EvaluationOptions LgOptions => new EvaluationOptions(Options);

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
            ExpressionParser expressionParser = null) => TemplatesParser.ParseFile(filePath, importResolver, expressionParser);

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
            ExpressionParser expressionParser = null) => TemplatesParser.ParseText(content, id, importResolver, expressionParser);

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <param name="opt">The EvaluationOptions in evaluating a template.</param>
        /// <returns>Evaluate result.</returns>
        public object Evaluate(string templateName, object scope = null, EvaluationOptions opt = null)
        {
            CheckErrors();
            var evalOpt = opt != null ? opt.Merge(LgOptions) : LgOptions;
            var evaluator = new Evaluator(AllTemplates.ToList(), ExpressionParser, evalOpt);
            return evaluator.EvaluateTemplate(templateName, scope);
        }

        /// <summary>
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="text">Inline string which will be evaluated.</param>
        /// <param name="scope">Scope object or JToken.</param>
        /// <param name="opt">The EvaluationOptions in evaluating a template.</param>
        /// <returns>Evaluate result.</returns>
        public object EvaluateText(string text, object scope = null, EvaluationOptions opt = null)
        {
            var evalOpt = opt != null ? opt.Merge(LgOptions) : LgOptions;

            if (text == null)
            {
                throw new ArgumentException("inline string is null.");
            }

            CheckErrors();

            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            var multiLineMark = "```";

            text = !text.Trim().StartsWith(multiLineMark) && text.Contains('\n')
                   ? $"{multiLineMark}{text}{multiLineMark}" : text;

            var newContent = $"# {fakeTemplateId} {newLine} - {text}";

            var newLG = TemplatesParser.ParseTextWithRef(newContent, this);

            return newLG.Evaluate(fakeTemplateId, scope, evalOpt);
        }

        /// <summary>
        /// Expand a template with given name and scope.
        /// Return all possible responses instead of random one.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <param name="opt">The evaluation option for current expander.</param>
        /// <returns>Expand result.</returns>
        public IList<object> ExpandTemplate(string templateName, object scope = null, EvaluationOptions opt = null)
        {
            CheckErrors();
            var evalOpt = opt ?? LgOptions;
            var expander = new Expander(AllTemplates.ToList(), ExpressionParser, evalOpt);
            return expander.ExpandTemplate(templateName, scope);
        }

        /// <summary>
        /// (experimental)
        /// Analyze a template to get the static analyzer results including variables and template references.
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
        /// Update an existing template.
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
                var templateNameLine = BuildTemplateNameLine(newTemplateName, parameters);
                var newTemplateBody = ConvertTemplateBody(templateBody);
                var content = $"{templateNameLine}{newLine}{newTemplateBody}";

                var startLine = template.SourceRange.Range.Start.Line - 1;
                var stopLine = template.SourceRange.Range.End.Line - 1;

                var newContent = ReplaceRangeContent(Content, startLine, stopLine, content);
                Initialize(ParseText(newContent, Id, ImportResolver));
            }

            return this;
        }

        /// <summary>
        /// Add a new template and return LG File.
        /// </summary>
        /// <param name="templateName">New template name.</param>
        /// <param name="parameters">New params.</param>
        /// <param name="templateBody">New  template body.</param>
        /// <returns>Updated LG file.</returns>
        public Templates AddTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = this.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception(TemplateErrors.TemplateExist(templateName));
            }

            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var newContent = $"{Content}{newLine}{templateNameLine}{newLine}{newTemplateBody}";
            Initialize(ParseText(newContent, Id, ImportResolver));

            return this;
        }

        /// <summary>
        /// Delete an exist template.
        /// </summary>
        /// <param name="templateName">Which template should delete.</param>
        /// <returns>Updated LG file.</returns>
        public Templates DeleteTemplate(string templateName)
        {
            var template = this.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                var startLine = template.SourceRange.Range.Start.Line - 1;
                var stopLine = template.SourceRange.Range.End.Line - 1;
                var newContent = ReplaceRangeContent(Content, startLine, stopLine, null);
                Initialize(ParseText(newContent, Id, ImportResolver));
            }

            return this;
        }

        public override string ToString() => Content;

        public override bool Equals(object obj)
        {
            if (!(obj is Templates lgFileObj))
            {
                return false;
            }

            return this.Id == lgFileObj.Id && this.Content == lgFileObj.Content;
        }

        public override int GetHashCode() => (Id, Content).GetHashCode();

        private string ReplaceRangeContent(string originString, int startLine, int stopLine, string replaceString)
        {
            var originList = originString.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            if (startLine < 0 || startLine > stopLine || stopLine >= originList.Length)
            {
                throw new Exception("index out of range.");
            }

            var destList = new List<string>();

            destList.AddRange(originList.Take(startLine));
            destList.Add(replaceString);
            destList.AddRange(originList.Skip(stopLine + 1));

            return string.Join(newLine, destList);
        }

        private string ConvertTemplateBody(string templateBody)
        {
            var lines = templateBody.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var destList = lines.Select(u =>
            {
                return u.TrimStart().StartsWith("#") ? $"- {u.TrimStart()}" : u;
            });

            return string.Join(newLine, destList);
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

        /// <summary>
        /// Use an existing LG file to override current object.
        /// </summary>
        /// <param name="templates">Existing LG file.</param>
        private void Initialize(Templates templates)
        {
            this.Clear();
            this.AddRange(templates);
            Imports = templates.Imports;
            Diagnostics = templates.Diagnostics;
            References = templates.References;
            Content = templates.Content;
            ImportResolver = templates.ImportResolver;
            Id = templates.Id;
            ExpressionParser = templates.ExpressionParser;
        }

        private void CheckErrors()
        {
            if (AllDiagnostics != null)
            {
                var errors = AllDiagnostics.Where(u => u.Severity == DiagnosticSeverity.Error);
                if (errors.Count() != 0)
                {
                    throw new Exception(string.Join(newLine, errors));
                }
            }
        }
    }
}
