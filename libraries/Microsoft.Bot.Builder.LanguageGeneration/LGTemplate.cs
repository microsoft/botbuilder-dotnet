using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Here is a data model that can easily understanded and used as the context or all kinds of visitors
    /// wether it's evalator, static checker, anayler.. etc.
    /// </summary>
    public class LGTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGTemplate"/> class.
        /// </summary>
        /// <param name="parseTree">The parse tree of this template.</param>
        /// <param name="lgfileContent">lg file content.</param>
        /// <param name="source">Source of this template.</param>
        public LGTemplate(LGFileParser.TemplateDefinitionContext parseTree, string lgfileContent, string source = "")
        {
            ParseTree = parseTree;
            Source = source;

            Name = ExtractName(parseTree);
            Parameters = ExtractParameters(parseTree);
            Body = ExtractBody(parseTree, lgfileContent);
        }

        /// <summary>
        /// Gets name of the template, what's followed by '#' in a LG file.
        /// </summary>
        /// <value>
        /// Name of the template, what's followed by '#' in a LG file.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets parameter list of this template.
        /// </summary>
        /// <value>
        /// Parameter list of this template.
        /// </value>
        public List<string> Parameters { get; }

        /// <summary>
        /// Gets or sets text format of Body of this template. All content except Name and Parameters.
        /// </summary>
        /// <value>
        /// Text format of Body of this template. All content except Name and Parameters.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Gets source of this template, source file path if it's from a certain file.
        /// </summary>
        /// <value>
        /// Source of this template, source file path if it's from a certain file.
        /// </value>
        public string Source { get; }

        /// <summary>
        /// Gets the parse tree of this template.
        /// </summary>
        /// <value>
        /// The parse tree of this template.
        /// </value>
        public LGFileParser.TemplateDefinitionContext ParseTree { get; }

        public override string ToString() => $"[{Name}({string.Join(", ", Parameters)})]\"{Body}\"";

        private string ExtractBody(LGFileParser.TemplateDefinitionContext parseTree, string lgfileContent)
        {
            var templateBody = parseTree.templateBody();
            if (templateBody == null)
            {
                return string.Empty;
            }

            var startLine = templateBody.Start.Line - 1;
            var stopLine = templateBody.Stop.Line - 1;

            return GetRangeContent(lgfileContent, startLine, stopLine);
        }

        private string ExtractName(LGFileParser.TemplateDefinitionContext parseTree)
        {
            var name = parseTree.templateNameLine().templateName()?.GetText();
            return name ?? string.Empty;
        }

        private List<string> ExtractParameters(LGFileParser.TemplateDefinitionContext parseTree)
        {
            var parameters = parseTree.templateNameLine().parameters();
            if (parameters != null)
            {
                return parameters.IDENTIFIER().Select(param => param.GetText()).ToList();
            }

            return new List<string>();
        }

        private string GetRangeContent(string originString, int startLine, int stopLine)
        {
            var originList = originString.Split('\n');
            if (startLine < 0 || startLine > stopLine || originList.Length <= stopLine)
            {
                throw new Exception("index out of range.");
            }

            var destList = originList.Skip(startLine).Take(stopLine - startLine + 1);
            return string.Join("\n", destList).TrimEnd('\r');
        }
    }
}
