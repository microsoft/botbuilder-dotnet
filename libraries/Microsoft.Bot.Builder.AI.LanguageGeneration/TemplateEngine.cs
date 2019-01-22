using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    /// <summary>
    /// The template engine that loads .lg file and eval based on memory/scope
    /// </summary>
    public class TemplateEngine
    {
        private readonly LGFileParser.FileContext _context = null;
        private TemplateEngine(LGFileParser.FileContext context)
        {
            _context = context;
        }
        
        public string Evaluate(string templateName, object scope)
        {
            var visitor = new LGFileVisitor(templateName, scope, this);
            return visitor.Visit(_context);
        }

        public static TemplateEngine FromFile(string filePath)
        {
            string lgFileContent = File.ReadAllText(filePath);
            try
            {
                var input = new AntlrInputStream(lgFileContent);
                var lexer = new LGFileLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new LGFileParser(tokens);
                parser.BuildParseTree = true;
                parser.ErrorHandler = new BailErrorStrategy();

                var context = parser.file();

                return new TemplateEngine(context);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
