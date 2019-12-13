using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGParser
    {
        public static LGFile ParseFile(string filePath, ImportResolverDelegate importResolver = null)
        {
            var lgFile = new LGFile(importResolver: importResolver ?? ImportResolver.FileResolver);
            var diagnostics = new List<Diagnostic>();
            try
            {
                var fullPath = Path.GetFullPath(ImportResolver.NormalizePath(filePath));
                var content = File.ReadAllText(fullPath);
                lgFile.Id = fullPath;
                lgFile.Content = content;
                var (templates, imports, errorTemplatesDiagnostics) = AntlrParse(content, fullPath);
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
                diagnostics.Add(new Diagnostic(new Range(new Position(0, 0), new Position(0, 0)), err.Message));
            }

            lgFile.Diagnostics = diagnostics;

            return lgFile;
        }

        public static LGFile ParseContent(string content, string id = "", ImportResolverDelegate importResolver = null)
        {
            var lgFile = new LGFile(content: content, id: id, importResolver: importResolver ?? ImportResolver.FileResolver);
            var diagnostics = new List<Diagnostic>();
            try
            {
                CheckImportResolver(id, importResolver);
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
                diagnostics.Add(new Diagnostic(new Range(new Position(0, 0), new Position(0, 0)), err.Message));
            }

            lgFile.Diagnostics = diagnostics;

            return lgFile;
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

            var diagnostics = new List<Diagnostic>();

            foreach (var errorTemplate in errorTemplates)
            {
                diagnostics.Add(BuildErrorContextDiagnostic(errorTemplate));
            }

            return diagnostics;
        }

        private static Diagnostic BuildErrorContextDiagnostic(ParserRuleContext context)
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

        private static void CheckImportResolver(string id, ImportResolverDelegate importResolver)
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

        private static List<LGFile> GetReferences(LGFile file, ImportResolverDelegate importResolver)
        {
            var resourcesFound = new HashSet<LGFile>();
            ResolveImportResources(file, importResolver ?? ImportResolver.FileResolver, resourcesFound);

            resourcesFound.Remove(file);
            return resourcesFound.ToList();
        }

        private static void ResolveImportResources(LGFile start, ImportResolverDelegate importResolver, HashSet<LGFile> resourcesFound)
        {
            var resourceIds = start.Imports.Select(lg => lg.Id).ToList();
            resourcesFound.Add(start);

            foreach (var id in resourceIds)
            {
                try
                {
                    var (content, path) = importResolver(start.Id, id);
                    var childResource = ParseContent(content, path, importResolver);
                    if (!resourcesFound.Contains(childResource))
                    {
                        ResolveImportResources(childResource, importResolver, resourcesFound);
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
