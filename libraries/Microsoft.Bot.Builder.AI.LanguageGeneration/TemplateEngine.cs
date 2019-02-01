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
    /// Helper info which help boost the evaluation process
    /// </summary>
    class EvaluationContext
    {
        public EvaluationContext(Dictionary<string, LGFileParser.TemplateDefinitionContext> templateContexts, Dictionary<string, List<string>> templateParameters)
        {
            TemplateContexts = templateContexts;
            TemplateParameters = templateParameters;
        }

        /// <summary>
        /// templateName => templateContext (parseTree) mapping
        /// </summary>
        public Dictionary<string, LGFileParser.TemplateDefinitionContext> TemplateContexts { get; set; }
        
        /// <summary>
        /// templateName => paramaterList mapping (if has parameters)
        /// </summary>
        public Dictionary<string, List<string>> TemplateParameters { get; set; }
    }


    /// <summary>
    /// The template engine that loads .lg file and eval based on memory/scope
    /// </summary>
    public class TemplateEngine
    {
        private readonly EvaluationContext evaluationContext = null;
        //private readonly LGFileParser.FileContext _context = null;
        //private readonly Dictionary<string, LGFileParser.TemplateDefinitionContext> _templates = null;
        private TemplateEngine(LGFileParser.FileContext context)
        {
            // Pre-compute some information to help the evalution process later
            var templateContexts = new Dictionary<string, LGFileParser.TemplateDefinitionContext>();
            var templateParameters = new Dictionary<string, List<string>>();

            // Iterate template parse tree
            var templates = context.paragraph().Select(x => x.templateDefinition()).Where(x => x != null);
            foreach (var template in templates)
            {
                // Extact name
                var templateName = template.templateName().IDENTIFIER().GetText();
                if (!templateContexts.ContainsKey(templateName))
                {
                    templateContexts[templateName] = template;
                }
                else
                {
                    throw new Exception($"Duplicated template definition with name: {templateName}");
                }

                // Extract parameter list
                var parameters = template.templateName().parameters();
                if (parameters != null)
                {
                    templateParameters[templateName] = parameters.IDENTIFIER().Select(x => x.GetText()).ToList();
                }
            }
            evaluationContext = new EvaluationContext(templateContexts, templateParameters);
        }
        
        public string Evaluate(string templateName, object scope)
        {

            var evalutor = new Evaluator(evaluationContext);
            return evalutor.Evaluate(templateName, scope);
            /*
            if (!_templates.ContainsKey(templateName))
            {
                throw new Exception($"No such template defined with name: {templateName}");
            }

            var visitor = new Evaluator(templateName, scope, this, _templates);
            return visitor.Visit(_templates[templateName]) ?? throw new Exception("Evaluation error");
             */    
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
                parser.AddErrorListener(ThrowingErrorListener.INSTANCE);

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
