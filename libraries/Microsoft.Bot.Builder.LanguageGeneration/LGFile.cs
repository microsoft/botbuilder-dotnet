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
            ImportResolver = importResolver;
            Id = id ?? string.Empty;
            ExpressionEngine = expressionEngine ?? new ExpressionEngine();
        }

        /// <summary>
        /// Gets get all templates from current lg file and reference lg files.
        /// </summary>
        /// <value>
        /// All templates from current lg file and reference lg files.
        /// </value>
        public IList<LGTemplate> AllTemplates => new List<LGFile> { this }.Union(References).SelectMany(x => x.Templates).ToList();

        /// <summary>
        /// Gets get all diagnostics from current lg file and reference lg files.
        /// </summary>
        /// <value>
        /// All diagnostics from current lg file and reference lg files.
        /// </value>
        public IList<Diagnostic> AllDiagnostics => new List<LGFile> { this }.Union(References).SelectMany(x => x.Diagnostics).ToList();

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
        /// Gets or sets expression parser.
        /// </summary>
        /// <value>
        /// expression parser.
        /// </value>
        public ExpressionEngine ExpressionEngine { get; set; }

        /// <summary>
        /// Gets or sets import elements that this LG file contains directly.
        /// </summary>
        /// <value>
        /// import elements that this LG file contains directly.
        /// </value>
        public IList<LGImport> Imports { get; set; }

        /// <summary>
        /// Gets or sets all references that this LG file has from <see cref="Imports"/>.
        /// Notice: reference includs all child imports from the lg file,
        /// not only the children belong to this lgfile directly.
        /// so, reference count may >= imports count. 
        /// </summary>
        /// <value>
        /// all references that this LG file has from <see cref="Imports"/>.
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
            CheckErrors();

            var memory = SimpleObjectMemory.Wrap(scope);
            var evaluator = new Evaluator(AllTemplates.ToList(), ExpressionEngine);
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
            CheckErrors();
            var expander = new Expander(AllTemplates.ToList(), ExpressionEngine);
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
            CheckErrors();
            var analyzer = new Analyzer(AllTemplates.ToList(), ExpressionEngine);
            return analyzer.AnalyzeTemplate(templateName);
        }

        /// <summary>
        /// update an exist template.
        /// </summary>
        /// <param name="templateName">origin template name. the only id of a template.</param>
        /// <param name="newTemplateName">new template Name.</param>
        /// <param name="parameters">new params.</param>
        /// <param name="templateBody">new template body.</param>
        /// <returns>updated lgfile.</returns>
        public LGFile UpdateTemplate(string templateName, string newTemplateName, List<string> parameters, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                var templateNameLine = BuildTemplateNameLine(newTemplateName, parameters);
                var newTemplateBody = ConvertTemplateBody(templateBody);
                var content = $"{templateNameLine}\r\n{newTemplateBody}\r\n";
                var startLine = template.ParseTree.Start.Line - 1;
                var stopLine = template.ParseTree.Stop.Line - 1;

                var newContent = ReplaceRangeContent(Content, startLine, stopLine, content);
                Initialize(LGParser.ParseText(newContent, Id, ImportResolver));
            }

            return this;
        }

        /// <summary>
        /// Add a new template and return LG File.
        /// </summary>
        /// <param name="templateName">new template name.</param>
        /// <param name="parameters">new params.</param>
        /// <param name="templateBody">new  template body.</param>
        /// <returns>updated lgfile.</returns>
        public LGFile AddTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception(LGErrors.TemplateExist(templateName));
            }

            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var newContent = $"{Content.TrimEnd()}\r\n\r\n{templateNameLine}\r\n{newTemplateBody}\r\n";
            Initialize(LGParser.ParseText(newContent, Id, ImportResolver));

            return this;
        }

        /// <summary>
        /// Delete an exist template.
        /// </summary>
        /// <param name="templateName">which template should delete.</param>
        /// <returns>updated lgfile.</returns>
        public LGFile DeleteTemplate(string templateName)
        {
            var template = Templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                var startLine = template.ParseTree.Start.Line - 1;
                var stopLine = template.ParseTree.Stop.Line - 1;

                var newContent = ReplaceRangeContent(Content, startLine, stopLine, null);
                Initialize(LGParser.ParseText(newContent, Id, ImportResolver));
            }

            return this;
        }

        public override string ToString() => Content;

        public override bool Equals(object obj)
        {
            if (!(obj is LGFile lgFileObj))
            {
                return false;
            }

            return this.Id == lgFileObj.Id && this.Content == lgFileObj.Content;
        }

        public override int GetHashCode() => (Id, Content).GetHashCode();

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

        /// <summary>
        /// use an existing LGFile to override current object.
        /// </summary>
        /// <param name="lgFile">Existing LGFile.</param>
        private void Initialize(LGFile lgFile)
        {
            Templates = lgFile.Templates;
            Imports = lgFile.Imports;
            Diagnostics = lgFile.Diagnostics;
            References = lgFile.References;
            Content = lgFile.Content;
            ImportResolver = lgFile.ImportResolver;
            Id = lgFile.Id;
            ExpressionEngine = lgFile.ExpressionEngine;
        }

        private void CheckErrors()
        {
            if (AllDiagnostics != null)
            {
                var errors = AllDiagnostics.Where(u => u.Severity == DiagnosticSeverity.Error);
                if (errors.Count() != 0)
                {
                    throw new Exception(string.Join("\n", errors));
                }
            }
        }
    }
}
