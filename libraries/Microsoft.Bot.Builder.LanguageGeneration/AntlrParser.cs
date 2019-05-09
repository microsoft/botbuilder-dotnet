using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class AntlrParser
    {
        /// <summary>
        /// Get LG template list from input string.
        /// </summary>
        /// <param name="text">LG file content or inline text.</param>
        /// <param name="source">text source.</param>
        /// <returns>LG template list.</returns>
        public static IList<LGTemplate> Parse(string text, string source = "")
        {
            var fileContentContext = GetFileContentContext(text);
            return fileContentContext.ToLGTemplates(source);
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
    }
}
