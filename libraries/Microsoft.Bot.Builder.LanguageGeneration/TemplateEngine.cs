﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// The template engine that loads .lg file and eval template based on memory/scope.
    /// </summary>
    public class TemplateEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngine"/> class.
        /// Return an empty engine, you can then use AddFile\AddFiles to add files to it,
        /// or you can just use this empty engine to evaluate inline template.
        /// </summary>
        public TemplateEngine()
        {
        }

        /// <summary>
        /// Delegate for resolving resource id of imported lg file.
        /// </summary>
        /// <param name="resourceId">Resource id to resolve.</param>
        /// <returns>Resolved resource id.</returns>
        public delegate string ImportResolverDelegate(string resourceId);

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
        /// <returns>Teamplate engine with parsed files.</returns>
        public TemplateEngine Add(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                this.Add(content: File.ReadAllText(filePath), name: filePath, importResolver: (id) =>
                {
                    // import paths are in resource files which can be executed on multiple OS environments
                    // Call GetOsPath() to map / & \ in importPath -> OSPath
                    string importPath = GetOsPath(id);
                    if (!Path.IsPathRooted(importPath))
                    {
                        // get full path for importPath relative to path which is doing the import.
                        importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), id));
                    }
                    return File.ReadAllText(importPath);
                });
            }

            RunStaticCheck(Templates);
            return this;
        }

        /// <summary>
        /// Load single .lg file into template engine.
        /// </summary>
        /// <param name="filePath">Paths to .lg file.</param>
        /// <returns>Teamplate engine with single parsed file.</returns>
        public TemplateEngine Add(string filePath) => Add(new List<string> { filePath });

        /// <summary>
        /// Add text as lg file content to template engine.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="name">Text name.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>Template engine with the parsed content.</returns>
        public TemplateEngine Add(string content, string name, ImportResolverDelegate importResolver)
        {
            var sources = new Dictionary<string, LGSource>();
            LoopLGText(content, name, sources, importResolver);

            foreach (var source in sources)
            {
                Templates = Templates.Concat(source.Value.Templates).ToList();
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
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>Evaluate result.</returns>
        public string Evaluate(string inlineStr, object scope, IGetMethod methodBinder = null, ImportResolverDelegate importResolver = null)
        {
            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            inlineStr = !inlineStr.Trim().StartsWith("```") && inlineStr.IndexOf('\n') >= 0
                   ? "```" + inlineStr + "```" : inlineStr;
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var sources = new Dictionary<string, LGSource>();
            LoopLGText(wrappedStr, "inline", sources, importResolver);

            var templates = new List<LGTemplate>(Templates);
            foreach (var source in sources)
            {
                templates = templates.Concat(source.Value.Templates).ToList();
            }

            RunStaticCheck(templates);

            var evaluator = new Evaluator(templates, methodBinder);
            return evaluator.EvaluateTemplate(fakeTemplateId, scope);
        }

        private void ImportIds(string[] ids, Dictionary<string, LGSource> sources, ImportResolverDelegate importResolver)
        {
            if (importResolver == null)
            {
                // default to fileResolver...
                importResolver = FileResolver;
            }

            foreach (var id in ids)
            {
                if (sources.ContainsKey(id))
                {
                    continue;
                }

                try
                {
                    var content = importResolver(id);
                    LoopLGText(content, id, sources, importResolver);
                }
                catch (Exception err)
                {
                    throw new Exception($"{id}:{err.Message}", err);
                }
            }
        }

        private void LoopLGText(string content, string name, Dictionary<string, LGSource> sources, ImportResolverDelegate importResolver)
        {
            var source = LGParser.Parse(content, name);
            sources.Add(name, source);
            ImportIds(source.Imports.Select(lg => lg.Id).ToArray(), sources, importResolver);
        }

        private string FileResolver(string path) => File.ReadAllText(path);

        /// <summary>
        /// Normalize authored path to os path.
        /// </summary>
        /// <remarks>
        /// path is from authored content which doesn't know what OS it is running on.
        /// This method treats / and \ both as seperators regardless of OS, for windows that means / -> \ and for linux/mac \ -> /.
        /// This allows author to use ../foo.lg or ..\foo.lg as equivelents for importing.
        /// </remarks>
        /// <param name="ambigiousPath">authoredPath</param>
        /// <returns>path expressed as OS path</returns>
        private static string GetOsPath(string ambigiousPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // map linux/mac sep -> windows
                return ambigiousPath.Replace("/", "\\");
            }
            else
            {
                // map windows sep -> linux/mac
                return ambigiousPath.Replace("\\", "/");
            }
        }
    }
}
