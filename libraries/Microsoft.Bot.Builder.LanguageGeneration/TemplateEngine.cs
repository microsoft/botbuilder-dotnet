using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// The template engine that loads .lg file and eval template based on memory/scope.
    /// </summary>
    public class TemplateEngine
    {
        /// <summary>
        /// Parsed LG templates.
        /// </summary>
        public List<LGTemplate> Templates = new List<LGTemplate>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngine"/> class.
        /// Return an empty engine, you can then use AddFile\AddFiles to add files to it,
        /// or you can just use this empty engine to evaluate inline template.
        /// </summary>
        public TemplateEngine()
        {
        }

        /// <summary>
        /// Create a template engine from files, a shorthand for.
        ///    new TemplateEngine().AddFiles(filePath).
        /// </summary>
        /// <param name="filePaths">paths to LG files.</param>
        /// <returns>Engine created.</returns>
        public static TemplateEngine FromFiles(params string[] filePaths)
        {
            return new TemplateEngine().AddFiles(filePaths);
        }

        /// <summary>
        /// Create a template engine from text, equivalent to.
        ///    new TemplateEngine.AddText(text).
        /// </summary>
        /// <param name="text">Content of lg file.</param>
        /// <returns>Engine created.</returns>
        public static TemplateEngine FromText(string text)
        {
            return new TemplateEngine().AddText(text);
        }

        /// <summary>
        /// Load .lg files into template engine
        /// You can add one file, or mutlple file as once
        /// If you have multiple files referencing each other, make sure you add them all at once,
        /// otherwise static checking won't allow you to add it one by one.
        /// </summary>
        /// <param name="filePaths">Paths to .lg files.</param>
        /// <returns>Teamplate engine with parsed files.</returns>
        public TemplateEngine AddFiles(params string[] filePaths)
        {
            var newTemplates = filePaths.Select(filePath =>
            {
                var bytes = File.ReadAllBytes(filePath);
                bytes = RemoveBomMark(bytes);
                var text = new UTF8Encoding(false).GetString(bytes);

                return ToTemplates(Parse(text), filePath);
            }).SelectMany(x => x);

            var mergedTemplates = Templates.Concat(newTemplates).ToList();

            RunStaticCheck(mergedTemplates);

            Templates = mergedTemplates; // only set value after static checking is passed

            return this;
        }

        /// <summary>
        /// Add text as lg file content to template engine.
        /// </summary>
        /// <param name="text">Text content contains lg templates.</param>
        /// <returns>Template engine with the parsed content.</returns>
        public TemplateEngine AddText(string text)
        {
            Templates.AddRange(ToTemplates(Parse(text), "text"));

            RunStaticCheck();
            return this;
        }

        public void RunStaticCheck(List<LGTemplate> templates = null)
        {
            var teamplatesToCheck = templates ?? this.Templates;
            var checker = new StaticChecker(teamplatesToCheck);
            var report = checker.Check();

            var errors = report.Where(u => u.Type == ReportEntryType.ERROR).ToList();
            if (errors.Count != 0)
            {
                throw new Exception(string.Join("\n", errors));
            }
        }

        /// <summary>
        /// Evaluate a template with given name and scope.
        /// </summary>
        /// <param name="templateName">Template name to be evaluated.</param>
        /// <param name="scope">The state visible in the evaluation.</param>
        /// <param name="methodBinder">Optional methodBinder to extend or override functions.</param>
        /// <returns>Evaluate result.</returns>
        public string EvaluateTemplate(string templateName, object scope, IGetMethod methodBinder = null)
        {
            var evaluator = new Evaluator(Templates, methodBinder);
            return evaluator.EvaluateTemplate(templateName, scope);
        }

        public List<string> AnalyzeTemplate(string templateName)
        {
            var analyzer = new Analyzer(Templates);
            return analyzer.AnalyzeTemplate(templateName);
        }

        /// <summary>
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="inlineStr">inline string which will be evaluated.</param>
        /// <param name="scope">scope object or JToken.</param>
        /// <param name="methodBinder">input method.</param>
        /// <returns>Evaluate result.</returns>
        public string Evaluate(string inlineStr, object scope, IGetMethod methodBinder = null)
        {
            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = "__temp__";
            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var parsedTemplates = ToTemplates(Parse(wrappedStr), "inline");

            // merge the existing templates and this new template as a whole for evaluation
            var mergedTemplates = Templates.Concat(parsedTemplates).ToList();

            RunStaticCheck(mergedTemplates);

            var evaluator = new Evaluator(mergedTemplates, methodBinder);
            return evaluator.EvaluateTemplate(fakeTemplateId, scope);
        }

        /// <summary>
        /// Parse text as a LG file using antlr.
        /// </summary>
        /// <param name="text">text to parse.</param>
        /// <returns>ParseTree of the LG file.</returns>
        private LGFileParser.FileContext Parse(string text)
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
        private List<LGTemplate> ToTemplates(LGFileParser.FileContext file, string source = "")
        {
            if (file == null)
            {
                return new List<LGTemplate>();
            }

            var templates = file.paragraph().Select(x => x.templateDefinition()).Where(x => x != null);
            return templates.Select(t => new LGTemplate(t, source)).ToList();
        }

        private byte[] RemoveBomMark(byte[] bytes)
        {
            var bom = new UTF8Encoding(true).GetPreamble();
            while (bytes.Length >= bom.Length
            && bom.SequenceEqual(bytes.Take(bom.Length).ToArray()))
            {
                bytes = bytes.Skip(bom.Length).ToArray();
            }

            return bytes;
        }
    }
}
