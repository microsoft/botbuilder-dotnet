using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Expressions.Memory;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGFile
    {
        private readonly ExpressionEngine expressionEngine;
        private readonly ImportResolverDelegate importResolver;

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
            this.importResolver = importResolver ?? ImportResolver.FileResolver;
        }

        public IList<LGTemplate> AllTemplates
        {
            get
            {
                var referenceTemplates = References.GroupBy(x => x.Id).Select(x => x.First()).SelectMany(x => x.Templates);
                return Templates.Union(referenceTemplates).ToList();
            }
        }

        public IList<LGTemplate> Templates { get; set; }

        public IList<LGImport> Imports { get; set; }

        public IList<LGFile> References { get; set; }

        public IList<Diagnostic> Diagnostics { get; set; }

        public string Content { get; set; }

        public string Id { get; set; }

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
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="inlineStr">inline string which will be evaluated.</param>
        /// <param name="scope">scope object or JToken.</param>
        /// <returns>Evaluate result.</returns>
        public object Evaluate(string inlineStr, object scope = null)
        {
            CheckErrors(Diagnostics);

            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            inlineStr = !inlineStr.Trim().StartsWith("```") && inlineStr.IndexOf('\n') >= 0
                   ? "```" + inlineStr + "```" : inlineStr;
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var lgFile = LGParser.ParseContent(wrappedStr, "inline", importResolver);
            var templates = AllTemplates.Concat(lgFile.AllTemplates).ToList();

            CheckErrors(lgFile.Diagnostics);

            var evaluator = new Evaluator(templates, this.expressionEngine);
            return evaluator.EvaluateTemplate(fakeTemplateId, new CustomizedMemory(scope));
        }

        public IList<string> ExpandTemplate(string templateName, object scope = null)
        {
            CheckErrors(Diagnostics);
            var expander = new Expander(AllTemplates.ToList(), this.expressionEngine);
            return expander.EvaluateTemplate(templateName, new CustomizedMemory(scope));
        }

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
            var content = $"{templateNameLine}\r\n{newTemplateBody}";
            var startLine = template.ParseTree.Start.Line - 1;
            var stopLine = template.ParseTree.Stop.Line - 1;

            var newContent = ReplaceRangeContent(Content, startLine, stopLine, content);
            return LGParser.ParseContent(newContent, Id, importResolver);
        }

        /// <summary>
        /// Add a new template and return LG resource.
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
            var newContent = $"{Content}\r\n{templateNameLine}\r\n{newTemplateBody}";
            return LGParser.ParseContent(newContent, Id, importResolver);
        }

        /// <summary>
        /// Delete an exist template.
        /// </summary>
        /// <param name="templateName">which template should delete.</param>
        /// <returns>return the new lg resource.</returns>
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
            return LGParser.ParseContent(newContent, Id, importResolver);
        }

        public override string ToString() => Content;

        private string ReplaceRangeContent(string originString, int startLine, int stopLine, string replaceString)
        {
            var originList = originString.Split('\n');
            var destList = new List<string>();
            if (startLine < 0 || startLine > stopLine || originList.Length <= stopLine)
            {
                throw new Exception("index out of range.");
            }

            destList.AddRange(originList.Take(startLine));

            if (!string.IsNullOrEmpty(replaceString))
            {
                destList.Add(replaceString);
            }

            destList.AddRange(originList.Skip(stopLine + 1));

            return BuildNewLGContent(destList);
        }

        private string BuildNewLGContent(List<string> destList)
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
                else if (i < destList.Count - 1)
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
            if (Diagnostics.Any(u => u.Severity == DiagnosticSeverity.Error))
            {
                throw new Exception("Please fix the error diagnostics first.");
            }
        }
    }
}
