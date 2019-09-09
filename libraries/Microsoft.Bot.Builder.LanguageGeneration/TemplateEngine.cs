using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// The template engine that loads .lg file and eval template based on memory/scope.
    /// </summary>
    public class TemplateEngine
    {
        private readonly ExpressionEngine expressionEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngine"/> class.
        /// Return an empty engine, you can then use AddFile\AddFiles to add files to it,
        /// or you can just use this empty engine to evaluate inline template.
        /// </summary>
        /// <param name="expressionEngine">The expression engine this template engine based on.</param>
        public TemplateEngine(ExpressionEngine expressionEngine = null)
        {
            this.expressionEngine = expressionEngine ?? new ExpressionEngine();
        }

        /// <summary>
        /// Gets or sets parsed LG templates.
        /// </summary>
        /// <value>
        /// Parsed LG templates.
        /// </value>
        public List<LGTemplate> Templates { get; set; } = new List<LGTemplate>();

        /// <summary>
        /// Load .lg files into template engine
        /// You can add one file, or mutlple file as once
        /// If you have multiple files referencing each other, make sure you add them all at once,
        /// otherwise static checking won't allow you to add it one by one.
        /// </summary>
        /// <param name="filePaths">Paths to .lg files.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>Teamplate engine with parsed files.</returns>
        public TemplateEngine AddFiles(IEnumerable<string> filePaths, ImportResolverDelegate importResolver = null)
        {
            var totalLGResources = new List<LGResource>();
            foreach (var filePath in filePaths)
            {
                var fullPath = Path.GetFullPath(ImportResolver.NormalizePath(filePath));
                var rootResource = LGParser.Parse(File.ReadAllText(fullPath), fullPath);
                var lgresources = rootResource.DiscoverDependencies(importResolver);
                totalLGResources.AddRange(lgresources);
            }

            // Remove duplicated lg files by id
            var deduplicatedLGResources = totalLGResources.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            Templates.AddRange(deduplicatedLGResources.SelectMany(x => x.Templates));
            RunStaticCheck(Templates);

            return this;
        }

        /// <summary>
        /// Load single .lg file into template engine.
        /// </summary>
        /// <param name="filePath">Path to .lg file.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>Teamplate engine with single parsed file.</returns>
        public TemplateEngine AddFile(string filePath, ImportResolverDelegate importResolver = null) => AddFiles(new List<string> { filePath }, importResolver);

        /// <summary>
        /// Add text as lg file content to template engine. A fullpath id is needed when importResolver is empty, or simply pass in customized importResolver.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">id is the content identifier. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>Template engine with the parsed content.</returns>
        public TemplateEngine AddText(string content, string id = "", ImportResolverDelegate importResolver = null)
        {
            CheckImportResolver(id, importResolver);

            var rootResource = LGParser.Parse(content, id);
            var lgresources = rootResource.DiscoverDependencies(importResolver);
            Templates.AddRange(lgresources.SelectMany(x => x.Templates));
            RunStaticCheck(Templates);

            return this;
        }

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <returns>Evaluate result.</returns>
        public object EvaluateTemplate(string templateName, object scope = null)
        {
            var evaluator = new Evaluator(Templates, this.expressionEngine);
            return evaluator.EvaluateTemplate(templateName, scope);
        }

        /// <summary>
        /// Expand a template with given name and scope.
        /// Return all possible responses instead of random one.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <returns>Expand result.</returns>
        public List<string> ExpandTemplate(string templateName, object scope = null)
        {
            var expander = new Expander(Templates, this.expressionEngine);
            return expander.EvaluateTemplate(templateName, scope);
        }

        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            var analyzer = new Analyzer(Templates, this.expressionEngine);
            return analyzer.AnalyzeTemplate(templateName);
        }

        /// <summary>
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="inlineStr">inline string which will be evaluated.</param>
        /// <param name="scope">scope object or JToken.</param>
        /// <returns>Evaluate result.</returns>
        public object Evaluate(string inlineStr, object scope = null)
        {
            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            inlineStr = !inlineStr.Trim().StartsWith("```") && inlineStr.IndexOf('\n') >= 0
                   ? "```" + inlineStr + "```" : inlineStr;
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var lgsource = LGParser.Parse(wrappedStr, "inline");
            var templates = Templates.Concat(lgsource.Templates).ToList();
            RunStaticCheck(templates);

            var evaluator = new Evaluator(templates, this.expressionEngine);
            return evaluator.EvaluateTemplate(fakeTemplateId, scope);
        }

        /// <summary>
        /// Check templates/text to match LG format.
        /// </summary>
        /// <param name="templates">the templates which should be checked.</param>
        private void RunStaticCheck(List<LGTemplate> templates = null)
        {
            var teamplatesToCheck = templates ?? this.Templates;
            var diagnostics = new StaticChecker(this.expressionEngine).CheckTemplates(teamplatesToCheck);

            var errors = diagnostics.Where(u => u.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count != 0)
            {
                throw new Exception(string.Join("\n", errors));
            }
        }

        private void CheckImportResolver(string id, ImportResolverDelegate importResolver)
        {
            // Currently if no resolver is passed into AddText(),
            // the default fileResolver is used to resolve the imports.
            // default fileResolver require resource id should be fullPath,
            // so that it can resolve relative path based on this fullPath.
            // But we didn't check the id provided with AddText is fullPath or not.
            // So when id != fullPath, fileResolver won't work.
            if (importResolver == null)
            {
                var importPath = ImportResolver.NormalizePath(id);
                if (!Path.IsPathRooted(importPath))
                {
                    throw new Exception("[Error] id must be full path when importResolver is null");
                }
            }
        }
    }
}
