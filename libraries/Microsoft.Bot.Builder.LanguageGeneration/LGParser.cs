// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AdaptiveExpressions;
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
    /// Parser to turn lg content into a <see cref="LGFile"/>.
    /// </summary>
    public static class LGParser
    {
        /// <summary>
        /// option regex.
        /// </summary>
        private static readonly Regex OptionRegex = new Regex(@"^> *!#(.*)$");

        /// <summary>
        /// Parser to turn lg content into a <see cref="LGFile"/>.
        /// </summary>
        /// <param name="filePath"> absolut path of a LG file.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">expressionEngine Expression parser for evaluating expressions.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public static LGFile ParseFile(
            string filePath,
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            var fullPath = Path.GetFullPath(filePath.NormalizePath());
            var content = File.ReadAllText(fullPath);

            return ParseText(content, fullPath, importResolver, expressionParser);
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="LGFile"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">expressionEngine Expression parser for evaluating expressions.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public static LGFile ParseText(
            string content,
            string id = "",
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            importResolver = importResolver ?? DefaultFileResolver;
            var lgFile = new LGFile(content: content, id: id, importResolver: importResolver, expressionParser: expressionParser);

            var diagnostics = new List<Diagnostic>();
            try
            {
                var (templates, imports, invalidTemplateErrors, options) = AntlrParse(content, id);
                lgFile.Templates = templates;
                lgFile.Imports = imports;
                lgFile.Options = options;
                diagnostics.AddRange(invalidTemplateErrors);

                lgFile.References = GetReferences(lgFile, importResolver);
                var semanticErrors = new StaticChecker(lgFile).Check();
                diagnostics.AddRange(semanticErrors);
            }
            catch (LGException ex)
            {
                diagnostics.AddRange(ex.Diagnostics);
            }
            catch (Exception err)
            {
                diagnostics.Add(BuildDiagnostic(err.Message, source: id));
            }

            lgFile.Diagnostics = diagnostics;

            return lgFile;
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="LGFile"/> based on the original LGFile.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="lgFile">original LGFile.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        public static LGFile ParseTextWithRef(string content, LGFile lgFile)
        {
            if (lgFile == null)
            {
                throw new ArgumentNullException(nameof(lgFile));
            }

            var id = "inline content";
            var newLgFile = new LGFile(content: content, id: id, importResolver: lgFile.ImportResolver, options: lgFile.Options);
            var diagnostics = new List<Diagnostic>();
            try
            {
                var (templates, imports, invalidTemplateErrors, options) = AntlrParse(content, id);
                newLgFile.Templates = templates;
                newLgFile.Imports = imports;
                newLgFile.Options = options;
                diagnostics.AddRange(invalidTemplateErrors);

                newLgFile.References = GetReferences(newLgFile, newLgFile.ImportResolver)
                        .Union(lgFile.References)
                        .Union(new List<LGFile> { lgFile })
                        .ToList();

                var semanticErrors = new StaticChecker(newLgFile).Check();
                diagnostics.AddRange(semanticErrors);
            }
            catch (LGException ex)
            {
                diagnostics.AddRange(ex.Diagnostics);
            }
            catch (Exception err)
            {
                diagnostics.Add(BuildDiagnostic(err.Message, source: id));
            }

            newLgFile.Diagnostics = diagnostics;

            return newLgFile;
        }

        /// <summary>
        /// Default import resolver, using relative/absolute file path to access the file content.
        /// </summary>
        /// <param name="sourceId">default is file path.</param>
        /// <param name="resourceId">import path.</param>
        /// <returns>target content id.</returns>
        private static (string content, string id) DefaultFileResolver(string sourceId, string resourceId)
        {
            // import paths are in resource files which can be executed on multiple OS environments
            // normalize to map / & \ in importPath -> OSPath
            var importPath = resourceId.NormalizePath();

            if (!Path.IsPathRooted(importPath))
            {
                // get full path for importPath relative to path which is doing the import.
                importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceId), resourceId));
            }

            return (File.ReadAllText(importPath), importPath);
        }

        private static (IList<LGTemplate> templates, IList<LGImport> imports, IList<Diagnostic> diagnostics, IList<string> options) AntlrParse(string content, string id = "")
        {
            var fileContext = GetFileContentContext(content, id);
            var templates = ExtractLGTemplates(fileContext, content, id);
            var imports = ExtractLGImports(fileContext, id);
            var options = ExtractLGOptions(fileContext);
            var diagnostics = GetInvalidTemplateErrors(fileContext, id);

            return (templates, imports, diagnostics, options);
        }

        private static IList<Diagnostic> GetInvalidTemplateErrors(LGFileParser.FileContext fileContext, string id)
        {
            var errorTemplates = fileContext == null ? new List<LGFileParser.ErrorTemplateContext>() :
                   fileContext.paragraph()
                   .Select(x => x.errorTemplate())
                   .Where(x => x != null);

            return errorTemplates.Select(u => BuildDiagnostic("error context.", u, id)).ToList();
        }

        private static Diagnostic BuildDiagnostic(string errorMessage, ParserRuleContext context = null, string source = null)
        {
            errorMessage = LGErrors.StaticFailure + "- " + errorMessage;
            var startPosition = context == null ? new Position(0, 0) : new Position(context.Start.Line, context.Start.Column);
            var stopPosition = context == null ? new Position(0, 0) : new Position(context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
            return new Diagnostic(new Range(startPosition, stopPosition), errorMessage, source: source);
        }

        /// <summary>
        /// Get parsed tree nodes from text by antlr4 engine.
        /// </summary>
        /// <param name="text">Original text which will be parsed.</param>
        /// <returns>Parsed tree nodes.</returns>
        private static LGFileParser.FileContext GetFileContentContext(string text, string id)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var input = new AntlrInputStream(text);
            var lexer = new LGFileLexer(input);
            lexer.RemoveErrorListeners();

            var tokens = new CommonTokenStream(lexer);
            var parser = new LGFileParser(tokens);
            parser.RemoveErrorListeners();
            var listener = new ErrorListener(id);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.file();
        }

        /// <summary>
        /// Extract LG templates from the parse tree of a file.
        /// </summary>
        /// <param name="file">LG file context from ANTLR parser.</param>
        /// <param name="lgfileContent">LG file content.</param>
        /// <param name="source">text source.</param>
        /// <returns>LG template list.</returns>
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
        /// Extract LG options from the parse tree of a file.
        /// </summary>
        /// <param name="file">LG file context from ANTLR parser.</param>
        /// <returns>Option list.</returns>
        private static IList<string> ExtractLGOptions(LGFileParser.FileContext file)
        {
            return file == null ? new List<string>() :
                   file.paragraph()
                   .Select(x => x.optionsDefinition())
                   .Where(x => x != null)
                   .Select(t => ExtractOption(t.GetText()))
                   .Where(t => !string.IsNullOrEmpty(t))
                   .ToList();
        }

        private static string ExtractOption(string originalText)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(originalText))
            {
                return result;
            }

            var matchResult = OptionRegex.Match(originalText);
            if (matchResult.Success && matchResult.Groups.Count == 2)
            {
                result = matchResult.Groups[1].Value?.Trim();
            }

            return result;
        }

        /// <summary>
        /// Extract LG imports from a file parse tree.
        /// </summary>
        /// <param name="file">LG file context from ANTLR parser.</param>
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

        private static IList<LGFile> GetReferences(LGFile file, ImportResolverDelegate importResolver)
        {
            var resourcesFound = new HashSet<LGFile>();
            ResolveImportResources(file, resourcesFound, importResolver);

            resourcesFound.Remove(file);
            return resourcesFound.ToList();
        }

        private static void ResolveImportResources(LGFile start, HashSet<LGFile> resourcesFound, ImportResolverDelegate importResolver)
        {
            var resourceIds = start.Imports.Select(lg => lg.Id);
            resourcesFound.Add(start);

            foreach (var id in resourceIds)
            {
                try
                {
                    var (content, path) = importResolver(start.Id, id);
                    if (resourcesFound.All(u => u.Id != path))
                    {
                        var childResource = ParseText(content, path, importResolver);
                        ResolveImportResources(childResource, resourcesFound, importResolver);
                    }
                }
                catch (LGException err)
                {
                    throw err;
                }
                catch (Exception err)
                {
                    throw new LGException(err.Message, new List<Diagnostic> { BuildDiagnostic(err.Message, source: start.Id) });
                }
            }
        }
    }
}
