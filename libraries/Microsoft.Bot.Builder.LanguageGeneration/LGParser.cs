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
            var fullPath = Path.GetFullPath(ImportResolver.NormalizePath(filePath));
            var lgFile = AntlrParse(File.ReadAllText(fullPath), fullPath);

            // TODO
            List<LGFile> references = null;
            lgFile.References = references;
            return lgFile;
        }

        public static LGFile ParseContent(string content, string id = "", ImportResolverDelegate importResolver = null)
        {
            CheckImportResolver(id, importResolver);
            var lgFile = AntlrParse(content, id);

            // TODO
            List<LGFile> references = null;
            lgFile.References = references;
            return lgFile;
        }

        // do not throw exception
        private static LGFile AntlrParse(string content, string id = "")
        {
            var fileContext = GetFileContentContext(content, id);
            var templates = ExtractLGTemplates(fileContext, content, id);
            var imports = ExtractLGImports(fileContext, id);

            // TODO how to handler it.
            var errorTemplates = fileContext == null ? new List<LGFileParser.ErrorTemplateContext>() :
                   fileContext.paragraph()
                   .Select(x => x.errorTemplate());

            // todo. trycatch to get diagnostic
            return new LGFile(templates, imports, null, content: content);
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
    }
}
