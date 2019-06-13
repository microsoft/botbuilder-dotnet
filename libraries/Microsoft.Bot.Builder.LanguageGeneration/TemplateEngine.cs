using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// The template engine that loads .lg file and eval template based on memory/scope.
    /// </summary>
    public class TemplateEngine
    {
        public delegate string FileResolverDelegate(string filePath);

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngine"/> class.
        /// Return an empty engine, you can then use AddFile\AddFiles to add files to it,
        /// or you can just use this empty engine to evaluate inline template.
        /// </summary>
        public TemplateEngine()
        {
        }

        /// <summary>
        /// Gets or sets parsed LG templates.
        /// </summary>
        /// <value>
        /// Parsed LG templates.
        /// </value>
        public List<LGTemplate> Templates { get; set; } = new List<LGTemplate>();

        /// <summary>
        /// Create a template engine from files, a shorthand for.
        ///    new TemplateEngine().AddFiles(filePath).
        /// </summary>
        /// <param name="filePath">paths to LG files.</param>
        /// <param name="fileResolver">resolver to resolve LG file path.</param>
        /// <returns>Engine created.</returns>
        public static TemplateEngine FromFiles(string filePath, FileResolverDelegate fileResolver = null) => new TemplateEngine().Add(new string[] { filePath }, fileResolver);

        /// <summary>
        /// Create a template engine from files, a shorthand for.
        ///    new TemplateEngine().AddFiles(filePath).
        /// </summary>
        /// <param name="filePaths">paths to LG files.</param>
        /// <param name="fileResolver">resolver to resolve LG file path.</param>
        /// <returns>Engine created.</returns>
        public static TemplateEngine FromFiles(string[] filePaths, FileResolverDelegate fileResolver = null) => new TemplateEngine().Add(filePaths, fileResolver);

        /// <summary>
        /// Create a template engine from text, equivalent to.
        ///    new TemplateEngine.AddText(text).
        /// </summary>
        /// <param name="content">lg text content.</param>
        /// <param name="name">name of lg text.</param>
        /// <param name="fileResolver">resolver to resolve LG file path.</param>
        /// <returns>Engine created.</returns>
        public static TemplateEngine FromText(string content, string name, FileResolverDelegate fileResolver = null) => new TemplateEngine().Add(content, name, fileResolver);

        /// <summary>
        /// Load .lg files into template engine
        /// You can add one file, or mutlple file as once
        /// If you have multiple files referencing each other, make sure you add them all at once,
        /// otherwise static checking won't allow you to add it one by one.
        /// </summary>
        /// <param name="filePaths">Paths to .lg files.</param>
        /// <param name="fileResolver">resolver to resolve LG file path.</param>
        /// <returns>Teamplate engine with parsed files.</returns>
        public TemplateEngine Add(string[] filePaths, FileResolverDelegate fileResolver = null)
        {
            var lgFileDic = new Dictionary<string, LGFile>();
            LoopLGFiles(filePaths, lgFileDic, fileResolver);

            foreach (var lgFile in lgFileDic)
            {
                Templates = Templates.Concat(lgFile.Value.Templates).ToList();
            }

            RunStaticCheck(Templates);
            return this;
        }

        /// <summary>
        /// Add text as lg file content to template engine.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="name">Text name.</param>
        /// <param name="fileResolver">resolve lg file delegate.</param>
        /// <returns>Template engine with the parsed content.</returns>
        public TemplateEngine Add(string content, string name, FileResolverDelegate fileResolver = null)
        {
            var lgFileDic = new Dictionary<string, LGFile>();
            LoopLGText(content, name, lgFileDic, fileResolver);

            foreach (var lgFile in lgFileDic)
            {
                Templates = Templates.Concat(lgFile.Value.Templates).ToList();
            }

            RunStaticCheck(Templates);
            return this;
        }

        /// <summary>
        /// Check templates/text to match LG format.
        /// </summary>
        /// <param name="templates">the templates which should be checked.</param>
        public void RunStaticCheck(List<LGTemplate> templates = null)
        {
            var teamplatesToCheck = templates ?? this.Templates;
            var checker = new StaticChecker(teamplatesToCheck);
            var diagnostics = checker.Check();

            var errors = diagnostics.Where(u => u.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count != 0)
            {
                throw new Exception(string.Join("\n", errors));
            }
        }

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <param name="methodBinder">Optional methodBinder to extend or override functions.</param>
        /// <returns>Evaluate result.</returns>
        public string EvaluateTemplate(string templateName, object scope, IGetMethod methodBinder = null)
        {
            var evaluator = new Evaluator(Templates, methodBinder);
            return evaluator.EvaluateTemplate(templateName, scope);
        }

        public List<string> AnalyzeTemplate(string templateName)
        {
            var analyzer = new Analyzer(Templates);
            return analyzer.AnalyzeTemplate(templateName);
        }

        /// <summary>
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="inlineStr">inline string which will be evaluated.</param>
        /// <param name="scope">scope object or JToken.</param>
        /// <param name="methodBinder">input method.</param>
        /// <param name="fileResolver">resolve lg file delegate.</param>
        /// <returns>Evaluate result.</returns>
        public string Evaluate(string inlineStr, object scope, IGetMethod methodBinder = null, FileResolverDelegate fileResolver = null)
        {
            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            inlineStr = !inlineStr.Trim().StartsWith("```") && inlineStr.IndexOf('\n') >= 0
                   ? "```" + inlineStr + "```" : inlineStr;
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var lgFileDic = new Dictionary<string, LGFile>();
            LoopLGText(wrappedStr, "inline", lgFileDic, fileResolver);

            var templates = new List<LGTemplate>(Templates);
            foreach (var lgFile in lgFileDic)
            {
                templates = templates.Concat(lgFile.Value.Templates).ToList();
            }

            RunStaticCheck(templates);

            var evaluator = new Evaluator(templates, methodBinder);
            return evaluator.EvaluateTemplate(fakeTemplateId, scope);
        }

        private void LoopLGFiles(string[] filePaths, Dictionary<string, LGFile> finalLgFiles, FileResolverDelegate fileResolver)
        {
            foreach (var filePath in filePaths)
            {
                var resolvedPath = fileResolver != null ? fileResolver(filePath) : filePath;
                var absolutePath = new FileInfo(resolvedPath).FullName;
                if (finalLgFiles.ContainsKey(absolutePath))
                {
                    continue;
                }

                var text = string.Empty;
                try
                {
                    text = File.ReadAllText(absolutePath);
                }
                catch
                {
                    throw new Exception($"Invalid file path: {absolutePath}.");
                }

                var lgFile = LGParser.Parse(text, absolutePath);
                finalLgFiles.Add(absolutePath, lgFile);
                var importedFilePaths = lgFile.Imports.Select(e => fileResolver != null ? fileResolver(e.Path) : e.Path);
                importedFilePaths = importedFilePaths.Select(e => Path.IsPathRooted(e) ? e : Path.Combine(Path.GetDirectoryName(absolutePath), e));
                LoopLGFiles(importedFilePaths.ToArray(), finalLgFiles, fileResolver);
            }
        }

        private void LoopLGText(string content, string name, Dictionary<string, LGFile> finalLgFiles, FileResolverDelegate fileResolver)
        {
            var lgFile = LGParser.Parse(content, name);
            finalLgFiles.Add(name, lgFile);
            var importedFilePaths = new List<string>();
            lgFile.Imports.ToList().ForEach(e => importedFilePaths.Add(e.Path));
            LoopLGFiles(importedFilePaths.ToArray(), finalLgFiles, fileResolver);
        }
    }
}
