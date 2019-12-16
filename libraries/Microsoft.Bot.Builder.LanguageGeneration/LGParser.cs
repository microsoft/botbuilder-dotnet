// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Delegate for resolving resource id of imported lg file.
    /// </summary>
    /// <param name="sourceId">The id or path of source file.</param>
    /// <param name="resourceId">Resource id to resolve.</param>
    /// <returns>Resolved resource content and unique id.</returns>
    public delegate (string content, string id) ImportResolverDelegate(string sourceId, string resourceId);

    /// <summary>
    /// Parser to turn lg content into an <see cref="LGFile"/>.
    /// </summary>
    public class LGParser : ILGParser
    {
        /// <summary>
        /// resolver to resolve LG import id to template text.
        /// </summary>
        private readonly ImportResolverDelegate importResolver;

        private readonly ImportResolverDelegate defaultFileResolver = (filePath, id) =>
        {
            // import paths are in resource files which can be executed on multiple OS environments
            // normalize to map / & \ in importPath -> OSPath
            var importPath = PathUtil.NormalizePath(id);

            if (!Path.IsPathRooted(importPath))
            {
                // get full path for importPath relative to path which is doing the import.
                importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filePath), id));
            }

            return (File.ReadAllText(importPath), importPath);
        };

        public LGParser(ImportResolverDelegate importResolver = null)
        {
            this.importResolver = importResolver ?? defaultFileResolver;
        }

        /// <summary>
        /// Parser to turn lg content into an <see cref="LGFile"/>.
        /// </summary>
        /// <param name="filePath">LG absolute file path.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public LGFile ParseFile(string filePath)
        {
            var lgFile = new LGFile(importResolver: importResolver);
            var diagnostics = new List<Diagnostic>();
            try
            {
                var fullPath = Path.GetFullPath(PathUtil.NormalizePath(filePath));
                var content = File.ReadAllText(fullPath);
                lgFile.Id = fullPath;
                lgFile.Content = content;
                var (templates, imports, errorTemplatesDiagnostics) = AntlrParse(content, fullPath);
                lgFile.Templates = templates;
                lgFile.Imports = imports;
                diagnostics.AddRange(errorTemplatesDiagnostics);

                lgFile.References = GetReferences(lgFile);
                var managedDiagnostics = new StaticChecker(lgFile.AllTemplates.ToList()).Check();
                diagnostics.AddRange(managedDiagnostics);
            }
            catch (LGException ex)
            {
                diagnostics.AddRange(ex.Diagnostics);
            }
            catch (Exception err)
            {
                diagnostics.Add(new Diagnostic(new Range(new Position(0, 0), new Position(0, 0)), err.Message));
            }

            lgFile.Diagnostics = diagnostics;

            return lgFile;
        }

        /// <summary>
        /// Parser to turn lg content into an <see cref="LGFile"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">id is the content identifier. If importResolver is null, id must be a full path string. </param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public LGFile ParseContent(string content, string id = "")
        {
            var lgFile = new LGFile(content: content, id: id, importResolver: importResolver);
            var diagnostics = new List<Diagnostic>();
            try
            {
                var (templates, imports, errorTemplatesDiagnostics) = AntlrParse(content, id);
                lgFile.Templates = templates;
                lgFile.Imports = imports;
                diagnostics.AddRange(errorTemplatesDiagnostics);

                lgFile.References = GetReferences(lgFile);
                var managedDiagnostics = new StaticChecker(lgFile.AllTemplates.ToList()).Check();
                diagnostics.AddRange(managedDiagnostics);
            }
            catch (LGException ex)
            {
                diagnostics.AddRange(ex.Diagnostics);
            }
            catch (Exception err)
            {
                diagnostics.Add(new Diagnostic(new Range(new Position(0, 0), new Position(0, 0)), err.Message));
            }

            lgFile.Diagnostics = diagnostics;

            return lgFile;
        }

        private (IList<LGTemplate> templates, IList<LGImport> imports, IList<Diagnostic> diagnostics) AntlrParse(string content, string id = "")
        {
            var fileContext = GetFileContentContext(content, id);
            var templates = ExtractLGTemplates(fileContext, content, id);
            var imports = ExtractLGImports(fileContext, id);

            var diagnostics = GetErrorTemplatesDiagnostics(fileContext);

            return (templates, imports, diagnostics);
        }

        private IList<Diagnostic> GetErrorTemplatesDiagnostics(LGFileParser.FileContext fileContext)
        {
            var errorTemplates = fileContext == null ? new List<LGFileParser.ErrorTemplateContext>() :
                   fileContext.paragraph()
                   .Select(x => x.errorTemplate())
                   .Where(x => x != null);

            var diagnostics = new List<Diagnostic>();

            foreach (var errorTemplate in errorTemplates)
            {
                diagnostics.Add(BuildErrorContextDiagnostic(errorTemplate));
            }

            return diagnostics;
        }

        private Diagnostic BuildErrorContextDiagnostic(ParserRuleContext context)
        {
            var startPosition = new Position(context.Start.Line, context.Start.Column);
            var stopPosition = new Position(context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
            return new Diagnostic(new Range(startPosition, stopPosition), "error context.");
        }

        /// <summary>
        /// Get parsed tree node from text by antlr4 engine.
        /// </summary>
        /// <param name="text">Original text which will be parsed.</param>
        /// <returns>Parsed tree node.</returns>
        private LGFileParser.FileContext GetFileContentContext(string text, string id)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var input = new AntlrInputStream(text);
            var lexer = new LGFileLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new LGFileParser(tokens);
            parser.RemoveErrorListeners();
            var listener = new ErrorListener(id);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.file();
        }

        /// <summary>
        /// Extract LG templates from a file parse tree.
        /// </summary>
        /// <param name="file">LGFile context from antlr parser.</param>
        /// <param name="lgfileContent">LGFile content.</param>
        /// <param name="source">text source.</param>
        /// <returns>lg template list.</returns>
        private IList<LGTemplate> ExtractLGTemplates(LGFileParser.FileContext file, string lgfileContent, string source = "")
        {
            return file == null ? new List<LGTemplate>() :
                   file.paragraph()
                   .Select(x => x.templateDefinition())
                   .Where(x => x != null)
                   .Select(t => new LGTemplate(t, lgfileContent, source))
                   .ToList();
        }

        /// <summary>
        /// Extract LG imports from a file parse tree.
        /// </summary>
        /// <param name="file">LGFile context from antlr parser.</param>
        /// <param name="source">text source.</param>
        /// <returns>lg template list.</returns>
        private IList<LGImport> ExtractLGImports(LGFileParser.FileContext file, string source = "")
        {
            return file == null ? new List<LGImport>() :
                   file.paragraph()
                   .Select(x => x.importDefinition())
                   .Where(x => x != null)
                   .Select(t => new LGImport(t, source))
                   .ToList();
        }

        private List<LGFile> GetReferences(LGFile file)
        {
            var resourcesFound = new HashSet<LGFile>();
            ResolveImportResources(file, resourcesFound);

            resourcesFound.Remove(file);
            return resourcesFound.ToList();
        }

        private void ResolveImportResources(LGFile start, HashSet<LGFile> resourcesFound)
        {
            var resourceIds = start.Imports.Select(lg => lg.Id).ToList();
            resourcesFound.Add(start);

            foreach (var id in resourceIds)
            {
                try
                {
                    var (content, path) = importResolver(start.Id, id);
                    if (resourcesFound.All(u => u.Id != path))
                    {
                        var childResource = ParseContent(content, path);
                        ResolveImportResources(childResource, resourcesFound);
                    }
                }
                catch (Exception err)
                {
                    throw new Exception($"[Error]{id}:{err.Message}", err);
                }
            }
        }
    }
}
