// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Expressions.Memory;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG entrance, including properties that LG file has, and evaluate functions.
    /// </summary>
    public class LGFile
    {
        private readonly ExpressionEngine expressionEngine;

        public LGFile(
            IList<LGTemplate> templates = null,
            IList<LGImport> imports = null,
            IList<Diagnostic> diagnostics = null,
            IList<LGFile> references = null,
            string content = null,
            string id = null,
            ExpressionEngine expressionEngine = null,
            ImportResolverDelegate importResolver = null)
        {
            Templates = templates ?? new List<LGTemplate>();
            Imports = imports ?? new List<LGImport>();
            Diagnostics = diagnostics ?? new List<Diagnostic>();
            References = references ?? new List<LGFile>();
            Content = content ?? string.Empty;
            Id = id ?? string.Empty;
            this.expressionEngine = expressionEngine ?? new ExpressionEngine();
            this.ImportResolver = importResolver;
        }

        public IList<LGTemplate> AllTemplates
        {
            get
            {
                var referenceTemplates = References.GroupBy(x => x.Id).Select(x => x.First()).SelectMany(x => x.Templates);
                return Templates.Union(referenceTemplates).ToList();
            }
        }

        /// <summary>
        /// Gets or sets delegate for resolving resource id of imported lg file.
        /// </summary>
        /// <value>
        /// Delegate for resolving resource id of imported lg file.
        /// </value>
        public ImportResolverDelegate ImportResolver { get; set; }

        /// <summary>
        /// Gets or sets templates that this LG file contains directly.
        /// </summary>
        /// <value>
        /// templates that this LG file contains directly.
        /// </value>
        public IList<LGTemplate> Templates { get; set; }

        /// <summary>
        /// Gets or sets import elements that this LG file contains directly.
        /// </summary>
        /// <value>
        /// import elements that this LG file contains directly.
        /// </value>
        public IList<LGImport> Imports { get; set; }

        /// <summary>
        /// Gets or sets all references that this LG file has from <see cref="Imports"/>.
        /// </summary>
        /// <value>
        /// import elements that this LG file contains directly.
        /// </value>
        public IList<LGFile> References { get; set; }

        /// <summary>
        /// Gets or sets diagnostics.
        /// </summary>
        /// <value>
        /// diagnostics.
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
        /// id of this lg source. For file, is full path.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <returns>Evaluate result.</returns>
        public object EvaluateTemplate(string templateName, object scope = null)
        {
            CheckErrors(Diagnostics);
            if (!(scope is IMemory memory))
            {
                memory = SimpleObjectMemory.Wrap(scope);
            }

            var evaluator = new Evaluator(AllTemplates.ToList(), this.expressionEngine);
            return evaluator.EvaluateTemplate(templateName, new CustomizedMemory(memory));
        }

        /// <summary>
        /// Expand a template with given name and scope.
        /// Return all possible responses instead of random one.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <returns>Expand result.</returns>
        public IList<string> ExpandTemplate(string templateName, object scope = null)
        {
            CheckErrors(Diagnostics);
            var expander = new Expander(AllTemplates.ToList(), this.expressionEngine);
            return expander.EvaluateTemplate(templateName, new CustomizedMemory(scope));
        }

        /// <summary>
        /// (experimental)
        /// Analyzer a template to get the static analyzer results including variables and template references.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <returns>analyzer result.</returns>
        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            CheckErrors(Diagnostics);
            var analyzer = new Analyzer(AllTemplates.ToList(), this.expressionEngine);
            return analyzer.AnalyzeTemplate(templateName);
        }

        /// <summary>
        /// update an exist template.
        /// </summary>
        /// <param name="templateName">origin template name. the only id of a template.</param>
        /// <param name="newTemplateName">new template Name.</param>
        /// <param name="parameters">new params.</param>
        /// <param name="templateBody">new template body.</param>
        /// <returns>new LG resource.</returns>
        public LGFile UpdateTemplate(string templateName, string newTemplateName, List<string> parameters, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template == null)
            {
                return this;
            }

            var templateNameLine = BuildTemplateNameLine(newTemplateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var content = $"{templateNameLine}\r\n{newTemplateBody}\r\n";
            var startLine = template.ParseTree.Start.Line - 1;
            var stopLine = template.ParseTree.Stop.Line - 1;

            var newContent = ReplaceRangeContent(Content, startLine, stopLine, content);
            return LGParser.ParseContent(newContent, Id, ImportResolver);
        }

        /// <summary>
        /// Add a new template and return LG File.
        /// </summary>
        /// <param name="templateName">new template name.</param>
        /// <param name="parameters">new params.</param>
        /// <param name="templateBody">new  template body.</param>
        /// <returns>new lg resource.</returns>
        public LGFile AddTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception($"template {templateName} already exists.");
            }

            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var newContent = $"{Content.TrimEnd()}\r\n\r\n{templateNameLine}\r\n{newTemplateBody}\r\n";
            return LGParser.ParseContent(newContent, Id, ImportResolver);
        }

        /// <summary>
        /// Delete an exist template.
        /// </summary>
        /// <param name="templateName">which template should delete.</param>
        /// <returns>return the new lg file.</returns>
        public LGFile DeleteTemplate(string templateName)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template == null)
            {
                return this;
            }

            var startLine = template.ParseTree.Start.Line - 1;
            var stopLine = template.ParseTree.Stop.Line - 1;

            var newContent = ReplaceRangeContent(Content, startLine, stopLine, null);
            return LGParser.ParseContent(newContent, Id, ImportResolver);
        }

        public override string ToString() => Content;

        private string ReplaceRangeContent(string originString, int startLine, int stopLine, string replaceString)
        {
            var originList = originString.Split('\n');
            var destList = new List<string>();
            if (startLine < 0 || startLine > stopLine || stopLine >= originList.Length)
            {
                throw new Exception("index out of range.");
            }

            destList.AddRange(TrimList(originList.Take(startLine).ToList()));

            if (stopLine < originList.Length - 1)
            {
                destList.Add("\r\n");
                if (!string.IsNullOrEmpty(replaceString))
                {
                    destList.Add(replaceString);
                    destList.Add("\r\n");
                }

                destList.AddRange(TrimList(originList.Skip(stopLine + 1).ToList()));
            }
            else
            {
                // insert at the tail of the content
                if (!string.IsNullOrEmpty(replaceString))
                {
                    destList.Add("\r\n");
                    destList.Add(replaceString);
                }
            }

            return BuildNewLGContent(TrimList(destList));
        }

        /// <summary>
        /// trim the newlines at the beginning or at the tail of the array.
        /// </summary>
        /// <param name="input">input array.</param>
        /// <returns>trimed list.</returns>
        private IList<string> TrimList(IList<string> input)
        {
            if (input == null)
            {
                return null;
            }

            var startIndex = 0;
            var endIndex = input.Count;
            for (var i = 0; i < input.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(input[i]?.Trim()))
                {
                    startIndex = i;
                    break;
                }
            }

            for (var i = input.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(input[i]?.Trim()))
                {
                    endIndex = i + 1;
                    break;
                }
            }

            return input.Skip(startIndex).Take(endIndex - startIndex).ToList();
        }

        private string BuildNewLGContent(IList<string> destList)
        {
            var result = new StringBuilder();
            for (var i = 0; i < destList.Count; i++)
            {
                var currentItem = destList[i];
                result.Append(currentItem);
                if (currentItem.EndsWith("\r"))
                {
                    result.Append("\n");
                }
                else if (i < destList.Count - 1 && !currentItem.EndsWith("\r\n"))
                {
                    result.Append("\r\n");
                }
            }

            return result.ToString();
        }

        private string ConvertTemplateBody(string templateBody)
        {
            if (string.IsNullOrWhiteSpace(templateBody))
            {
                return string.Empty;
            }

            var replaceList = templateBody.Split('\n');

            return string.Join("\n", replaceList.Select(u => WrapTemplateBodyString(u)));
        }

        // we will warp '# abc' into '- #abc', to avoid adding additional template.
        private string WrapTemplateBodyString(string replaceItem) => replaceItem.TrimStart().StartsWith("#") ? $"- {replaceItem.TrimStart()}" : replaceItem;

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

        private void CheckErrors(IList<Diagnostic> diagnostics)
        {
            if (diagnostics == null)
            {
                throw new ArgumentException();
            }

            var errors = diagnostics.Where(u => u.Severity == DiagnosticSeverity.Error);
            if (errors.Count() != 0)
            {
                throw new Exception(string.Join("\n", errors));
            }
        }
    }
}
