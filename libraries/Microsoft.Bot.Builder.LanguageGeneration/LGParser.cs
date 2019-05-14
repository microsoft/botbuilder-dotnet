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
        /// <param name="source">text source.</param>
        /// <returns>LG template list.</returns>
        public static IList<LGTemplate> Parse(string text, string source = "")
        {
            var parseSuccess = TryParse(text, out var templates, out var diagnostic, source);
            if (parseSuccess)
            {
                return templates;
            }

            throw new Exception(diagnostic.ToString());
        }

        /// <summary>
        /// Try Get LG template list from input string.
        /// </summary>
        /// <param name="text">LG file content or inline text.</param>
        /// <param name="templates">LG template list.</param>
        /// <param name="diagnostic">error/warning list.</param>
        /// <param name="source">text source.</param>
        /// <returns>LG template if parse success.</returns>
        public static bool TryParse(string text, out IList<LGTemplate> templates, out Diagnostic diagnostic, string source = "")
        {
            LGFileParser.FileContext fileContext = null;
            diagnostic = null;
            templates = new List<LGTemplate>();

            try
            {
                fileContext = GetFileContentContext(text);
            }
            catch (Exception e)
            {
                diagnostic = JsonConvert.DeserializeObject<Diagnostic>(e.Message);
            }

            templates = ToLGTemplates(fileContext, source);

            return diagnostic == null;
        }

        private static LGFileParser.FileContext GetFileContentContext(string text)
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
            var listener = new ErrorListener();

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.file();
        }

        /// <summary>
        /// Convert a file parse tree to a list of LG templates.
        /// </summary>
        /// <param name="file">LGFile context from antlr parser.</param>
        /// <param name="source">text source.</param>
        /// <returns>lg template list.</returns>
        private static IList<LGTemplate> ToLGTemplates(LGFileParser.FileContext file, string source = "")
        {
            if (file == null)
            {
                return new List<LGTemplate>();
            }

            var templates = file.paragraph().Select(x => x.templateDefinition()).Where(x => x != null);
            return templates.Select(t => new LGTemplate(t, source)).ToList();
        }
    }
}
