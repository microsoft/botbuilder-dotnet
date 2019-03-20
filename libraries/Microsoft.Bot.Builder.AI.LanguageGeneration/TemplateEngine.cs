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
    public class EvaluationContext
    {
        public EvaluationContext()
        {
            TemplateContexts = new Dictionary<string, LGFileParser.TemplateDefinitionContext>();
            TemplateParameters = new Dictionary<string, List<string>>();
        }

        public EvaluationContext(Dictionary<string, LGFileParser.TemplateDefinitionContext> templateContexts, Dictionary<string, List<string>> templateParameters)
        {
            TemplateContexts = templateContexts;
            TemplateParameters = templateParameters;
        }

        public EvaluationContext(EvaluationContext context)
        {
            TemplateContexts = new Dictionary<string, LGFileParser.TemplateDefinitionContext>(context.TemplateContexts);
            TemplateParameters = new Dictionary<string, List<string>>(context.TemplateParameters);
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
        /// <summary>
        /// This is ensentially an index for the parse tree, used to accelerate the evalution process
        /// </summary>
        private readonly EvaluationContext evaluationContext = null;

        /// <summary>
        /// Use for create an empty engine
        /// </summary>
        private TemplateEngine()
        {
            evaluationContext = new EvaluationContext();
        }
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
                var templateName = template.templateNameLine().templateName().GetText();
                if (!templateContexts.ContainsKey(templateName))
                {
                    templateContexts[templateName] = template;
                }
                else
                {
                    //TODO: Understand why this reports duplicate items when there are actually no duplicates
                    //throw new Exception($"Duplicated template definition with name: {templateName}");
                }

                // Extract parameter list
                var parameters = template.templateNameLine().parameters();
                if (parameters != null)
                {
                    templateParameters[templateName] = parameters.IDENTIFIER().Select(x => x.GetText()).ToList();
                }
            }
            evaluationContext = new EvaluationContext(templateContexts, templateParameters);
        }
        
        public string EvaluateTemplate(string templateName, object scope, IGetValue valueBinder = null, IGetMethod methodBinder = null)
        {

            var evaluator = new Evaluator(evaluationContext, methodBinder, valueBinder);
            return evaluator.EvaluateTemplate(templateName, scope);
        }

        public List<string> AnalyzeTemplate(string templateName)
        {
            var analyzer = new Analyzer(evaluationContext);
            return analyzer.AnalyzeTemplate(templateName);
        }


        /// <summary>
        /// Use to evaluate an inline template str
        /// </summary>
        /// <param name="inlineStr"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public string Evaluate(string inlineStr, object scope, IGetValue valueBinder = null, IGetMethod methodBinder = null)
        {
            // TODO: maybe we can directly ref the templateBody without giving a name, but that means
            // we needs to make a little changes in the evalutor, especially the loop detection part
            
            var fakeTemplateId = "__temp__";
            // wrap inline string with "# name and -" to align the evaluation process
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            try
            {
                // Step 1: parse input, construct parse tree
                var input = new AntlrInputStream(wrappedStr);
                var lexer = new LGFileLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new LGFileParser(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(TemplateErrorListener.Instance);
                parser.BuildParseTree = true;
                parser.ErrorHandler = new BailErrorStrategy();
                // the only difference here is that we parse as templateBody, not as the whole file
                var context = parser.templateDefinition();

                // Step 2: constuct a new evalution context on top of the current one
                var evaluationContext = new EvaluationContext(this.evaluationContext);
                evaluationContext.TemplateContexts[fakeTemplateId] = context;
                var evaluator = new Evaluator(evaluationContext, methodBinder, valueBinder);

                // Step 3: evaluate
                return evaluator.EvaluateTemplate(fakeTemplateId, scope);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }
            
        }


        /// <summary>
        /// Make this a signleton ? or give a better name
        /// </summary>
        /// <returns></returns>
        public static TemplateEngine EmptyEngine()
        {
            return FromText("");
        }

        public static TemplateEngine FromFile(string filePath)
        {
            return FromText(File.ReadAllText(filePath));
        }

        public static TemplateEngine FromText(string lgFileContent)
        {
            // Short cut for empty engine
            if (string.IsNullOrEmpty(lgFileContent))
            {
                return new TemplateEngine();
            }

            try
            {
                var input = new AntlrInputStream(lgFileContent);
                var lexer = new LGFileLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new LGFileParser(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(TemplateErrorListener.Instance);
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
