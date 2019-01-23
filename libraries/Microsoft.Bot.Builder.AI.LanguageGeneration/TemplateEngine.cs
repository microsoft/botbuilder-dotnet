using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    /// <summary>
    /// The template engine that loads .lg file and eval based on memory/scope
    /// </summary>
    public class TemplateEngine
    {
        private readonly LGFileParser.FileContext _context = null;
        private readonly Dictionary<string, LGFileParser.TemplateDefinitionContext> _templates = null;
        private TemplateEngine(LGFileParser.FileContext context)
        {
            _context = context;
            _templates = new Dictionary<string, LGFileParser.TemplateDefinitionContext>();

            var templateContexts = _context.paragraph().Select(x => x.templateDefinition()).Where(x => x != null);
            foreach (var templateContext in templateContexts)
            {
                var templateName = templateContext.templateName().TEXT().GetText();
                if (!_templates.ContainsKey(templateName))
                {
                    _templates.Add(templateName, templateContext);
                }
                else
                {
                    throw new Exception($"Duplicated template definition with name: {templateName}");
                }
            }
        }
        
        public string Evaluate(string templateName, object scope)
        {
            if (!_templates.ContainsKey(templateName))
            {
                throw new Exception($"No such template defined with name: {templateName}");
            }

            var visitor = new LGFileVisitor(templateName, scope, this);
            return visitor.Visit(_templates[templateName]) ?? throw new Exception("Evaluation error");
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
