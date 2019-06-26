using System;
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
        /// <returns>Resolved resource content and unique id.</returns>
        public delegate (string content, string id) ImportResolverDelegate(string resourceId);

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
                importResolver = importResolver ?? ((id) =>
                 {
                     // import paths are in resource files which can be executed on multiple OS environments
                     // Call GetOsPath() to map / & \ in importPath -> OSPath
                     var importPath = GetOsPath(id);
                     if (!Path.IsPathRooted(importPath))
                     {
                         // get full path for importPath relative to path which is doing the import.
                         importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), id));
                     }

                     return (File.ReadAllText(importPath), importPath);
                 });

                var fullPath = Path.GetFullPath(filePath);
                var rootResource = LGParser.Parse(File.ReadAllText(fullPath), fullPath);
                var lgResources = this.DiscoverLGResources(rootResource, importResolver);
                totalLGResources.AddRange(lgResources);
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
        /// Add text as lg file content to template engine.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="name">Text name.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>Template engine with the parsed content.</returns>
        public TemplateEngine AddText(string content, string name, ImportResolverDelegate importResolver)
        {
            var rootResource = LGParser.Parse(content, name);
            var lgResources = this.DiscoverLGResources(rootResource, importResolver);
            Templates.AddRange(lgResources.SelectMany(x => x.Templates));
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
        public string EvaluateTemplate(string templateName, object scope = null, IGetMethod methodBinder = null)
        {
            var evaluator = new Evaluator(Templates, methodBinder);
            return evaluator.EvaluateTemplate(templateName, scope);
        }

        public AnalyzerResult AnalyzeTemplate(string templateName)
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
        /// <returns>Evaluate result.</returns>
        public string Evaluate(string inlineStr, object scope = null, IGetMethod methodBinder = null)
        {
            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            inlineStr = !inlineStr.Trim().StartsWith("```") && inlineStr.IndexOf('\n') >= 0
                   ? "```" + inlineStr + "```" : inlineStr;
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var lgSource = LGParser.Parse(wrappedStr, "inline");
            var templates = Templates.Concat(lgSource.Templates).ToList();
            RunStaticCheck(templates);

            var evaluator = new Evaluator(templates, methodBinder);
            return evaluator.EvaluateTemplate(fakeTemplateId, scope);
        }

        /// <summary>
        /// Discover all imported lg resources from a start resouce.
        /// </summary>
        /// <param name="start">The lg resource from which to start discovering imported resources.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>LGResource list of parsed lg content.</returns>
        private List<LGResource> DiscoverLGResources(LGResource start, ImportResolverDelegate importResolver)
        {
            var resourcesFound = new HashSet<LGResource>();
            ResolveImportResources(start, importResolver ?? FileResolver, resourcesFound);

            return resourcesFound.ToList();
        }

        /// <summary>
        /// Resolve imported LG resources from a start resource.
        /// All the imports will be visited and resolved to LGResouce list.
        /// </summary>
        /// <param name="start">The lg resource from which to start resolving imported resources.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="resourcesFound">Resources that have been found.</param>
        private void ResolveImportResources(LGResource start, ImportResolverDelegate importResolver, HashSet<LGResource> resourcesFound)
        {
            var resourceIds = start.Imports.Select(lg => lg.Id).ToList();
            resourcesFound.Add(start);

            foreach (var id in resourceIds)
            {
                try
                {
                    var (content, path) = importResolver(id);
                    var childResource = LGParser.Parse(content, path);
                    if (!resourcesFound.Contains(childResource))
                    {
                        ResolveImportResources(childResource, importResolver, resourcesFound);
                    }
                }
                catch (Exception err)
                {
                    throw new Exception($"{id}:{err.Message}", err);
                }
            }
        }

        /// <summary>
        /// Default file resolver.
        /// </summary>
        /// <param name="id">File id.</param>
        /// <returns>File content and unique id.</returns>
        private (string, string) FileResolver(string id)
        {
            id = Path.GetFullPath(id);
            return (File.ReadAllText(id), id);
        }

        /// <summary>
        /// Normalize authored path to os path.
        /// </summary>
        /// <remarks>
        /// path is from authored content which doesn't know what OS it is running on.
        /// This method treats / and \ both as seperators regardless of OS, for windows that means / -> \ and for linux/mac \ -> /.
        /// This allows author to use ../foo.lg or ..\foo.lg as equivelents for importing.
        /// </remarks>
        /// <param name="ambigiousPath">authoredPath.</param>
        /// <returns>path expressed as OS path.</returns>
        private string GetOsPath(string ambigiousPath)
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
