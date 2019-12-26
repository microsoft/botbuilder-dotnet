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
    public static class LGParser
    {
        /// <summary>
        /// Parser to turn lg content into an <see cref="LGFile"/>.
        /// </summary>
        /// <param name="filePath">LG absolute file path.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public static LGFile ParseFile(string filePath, ImportResolverDelegate importResolver = null)
        {
            var fullPath = Path.GetFullPath(PathUtil.NormalizePath(filePath));
            var content = File.ReadAllText(fullPath);

            return ParseContent(content, fullPath, importResolver);
        }

        /// <summary>
        /// Parser to turn lg content into an <see cref="LGFile"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">id is the content identifier. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public static LGFile ParseContent(string content, string id = "", ImportResolverDelegate importResolver = null)
        {
            importResolver = importResolver ?? DefaultFileResolver;
            var lgFile = new LGFile(content: content, id: id, importResolver: importResolver);
            var diagnostics = new List<Diagnostic>();
            try
            {
                var (templates, imports, errorTemplatesDiagnostics) = AntlrParse(content, id);
                lgFile.Templates = templates;
                lgFile.Imports = imports;
                diagnostics.AddRange(errorTemplatesDiagnostics);

                lgFile.References = GetReferences(lgFile, importResolver);
                var managedDiagnostics = new StaticChecker(lgFile.AllTemplates.ToList()).Check();
                diagnostics.AddRange(managedDiagnostics);
            }
            catch (LGException ex)
            {
                diagnostics.AddRange(ex.Diagnostics);
            }
            catch (Exception err)
            {
                diagnostics.Add(BuildDiagnostic(err.Message));
            }

            lgFile.Diagnostics = diagnostics;

            return lgFile;
        }

        private static (string content, string id) DefaultFileResolver(string filePath, string id)
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
        }

        private static (IList<LGTemplate> templates, IList<LGImport> imports, IList<Diagnostic> diagnostics) AntlrParse(string content, string id = "")
        {
            var fileContext = GetFileContentContext(content, id);
            var templates = ExtractLGTemplates(fileContext, content, id);
            var imports = ExtractLGImports(fileContext, id);

            var diagnostics = GetErrorTemplatesDiagnostics(fileContext);

            return (templates, imports, diagnostics);
        }

        private static IList<Diagnostic> GetErrorTemplatesDiagnostics(LGFileParser.FileContext fileContext)
        {
            var errorTemplates = fileContext == null ? new List<LGFileParser.ErrorTemplateContext>() :
                   fileContext.paragraph()
                   .Select(x => x.errorTemplate())
                   .Where(x => x != null);

            return errorTemplates.Select(u => BuildDiagnostic("error context.", u)).ToList();
        }

        private static Diagnostic BuildDiagnostic(string errorMessage, ParserRuleContext context = null)
        {
            var startPosition = context == null ? new Position(0, 0) : new Position(context.Start.Line, context.Start.Column);
            var stopPosition = context == null ? new Position(0, 0) : new Position(context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
            return new Diagnostic(new Range(startPosition, stopPosition), errorMessage);
        }

        /// <summary>
        /// Get parsed tree node from text by antlr4 engine.
        /// </summary>
        /// <param name="text">Original text which will be parsed.</param>
        /// <returns>Parsed tree node.</returns>
        private static LGFileParser.FileContext GetFileContentContext(string text, string id)
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
        private static IList<LGTemplate> ExtractLGTemplates(LGFileParser.FileContext file, string lgfileContent, string source = "")
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
        private static IList<LGImport> ExtractLGImports(LGFileParser.FileContext file, string source = "")
        {
            return file == null ? new List<LGImport>() :
                   file.paragraph()
                   .Select(x => x.importDefinition())
                   .Where(x => x != null)
                   .Select(t => new LGImport(t, source))
                   .ToList();
        }

        private static List<LGFile> GetReferences(LGFile file, ImportResolverDelegate importResolver)
        {
            var resourcesFound = new HashSet<LGFile>();
            ResolveImportResources(file, resourcesFound, importResolver);

            resourcesFound.Remove(file);
            return resourcesFound.ToList();
        }

        private static void ResolveImportResources(LGFile start, HashSet<LGFile> resourcesFound, ImportResolverDelegate importResolver)
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
                        ResolveImportResources(childResource, resourcesFound, importResolver);
                    }
                }
                catch (Exception err)
                {
                    throw new LGException(err.Message, new List<Diagnostic> { BuildDiagnostic(err.Message) });
                }
            }
        }
    }
}
