using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGParser
    {
        /// <summary>
        /// Get LG template list from input string.
        /// </summary>
        /// <param name="text">LG file content or inline text.</param>
        /// <param name="id">text source.</param>
        /// <returns>LG template list.</returns>
        public static LGResource Parse(string text, string id = "")
        {
            var parseSuccess = TryParse(text, out var templates, out var imports, out var error, id);
            if (!parseSuccess)
            {
                throw new Exception(error.ToString());
            }

            return new LGResource(templates, imports, id);
        }

        /// <summary>
        /// Try Get LG template list from input string.
        /// </summary>
        /// <param name="text">LG file content or inline text.</param>
        /// <param name="templates">LG template list.</param>
        /// <param name="imports">LG import list.</param>
        /// <param name="error">error/warning list.</param>
        /// <param name="source">text source.</param>
        /// <returns>LG template if parse success.</returns>
        public static bool TryParse(string text, out IList<LGTemplate> templates, out IList<LGImport> imports, out Diagnostic error, string source = "")
        {
            LGFileParser.FileContext fileContext = null;
            error = null;
            templates = new List<LGTemplate>();
            imports = new List<LGImport>();

            try
            {
                fileContext = GetFileContentContext(text, source);
                templates = ExtractLGTemplates(fileContext, source);
                imports = ExtractLGImports(fileContext, source);
            }
            catch (Exception e)
            {
                error = JsonConvert.DeserializeObject<Diagnostic>(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get parsed tree node from text by antlr4 engine.
        /// </summary>
        /// <param name="text">Original text which will be parsed.</param>
        /// <returns>Parsed tree node.</returns>
        private static LGFileParser.FileContext GetFileContentContext(string text, string source)
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
            var listener = new ErrorListener(source);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.file();
        }

        /// <summary>
        /// Extract LG templates from a file parse tree.
        /// </summary>
        /// <param name="file">LGFile context from antlr parser.</param>
        /// <param name="source">text source.</param>
        /// <returns>lg template list.</returns>
        private static IList<LGTemplate> ExtractLGTemplates(LGFileParser.FileContext file, string source = "")
        {
            return file == null ? new List<LGTemplate>() :
                   file.paragraph()
                   .Select(x => x.templateDefinition())
                   .Where(x => x != null)
                   .Select(t => new LGTemplate(t, source))
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
    }
}
