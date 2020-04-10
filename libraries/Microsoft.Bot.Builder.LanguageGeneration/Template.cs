// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Class which represents a single template which can be evaluated.
    /// </summary>
    /// <remarks>
    /// Defines a data model that can easily understand and use the context for all kinds of visitors,
    /// whether it's an evaluator, static checker, analyzer, and so on.
    /// </remarks>
    public class Template
    {
        private readonly LGFileParser.TemplateDefinitionContext templateParseTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        /// <param name="parseTree">The parse tree of this template.</param>
        /// <param name="source">Source of this template.</param>
        internal Template(LGFileParser.TemplateDefinitionContext parseTree, string source = "")
        {
            templateParseTree = parseTree;
            Source = source;

            ExtractNameAndParameters();
            ExtractBody();
        }

        /// <summary>
        /// Gets or sets name of the template, what's followed by '#' in a LG file.
        /// </summary>
        /// <value>
        /// Name of the template, what's followed by '#' in a LG file.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets parameter list of this template.
        /// </summary>
        /// <value>
        /// Parameter list of this template.
        /// </value>
        public List<string> Parameters { get; set; }

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
        /// Gets or sets the parse tree of this template.
        /// </summary>
        /// <value>
        /// The parse tree of this template.
        /// </value>
        public LGTemplateParser.TemplateBodyContext TemplateBodyParseTree { get; set; }

        public override string ToString() => $"[{Name}({string.Join(", ", Parameters)})]\"{Body}\"";

        /// <summary>
        /// Get the startLine and stopLine of template.
        /// </summary>
        /// <returns>template content range.</returns>
        public (int startLine, int stopLine) GetTemplateRange()
        {
            var startLine = templateParseTree.Start.Line - 1;
            var stopLine = templateParseTree.Stop.Line - 1;

            return (startLine, stopLine);
        }

        private void ExtractBody()
        {
            var templateBodyLines = templateParseTree.templateBodyLine()
                                    .Select(u =>
                                    { 
                                        if (u.TEMPLATE_BODY_LINE() != null)
                                        {
                                            return u.TEMPLATE_BODY_LINE().GetText();
                                        }
                                        else
                                        {
                                            return string.Empty;
                                        }
                                    });
            var templateBody = string.Join("\r\n", templateBodyLines);
            this.Body = templateBody;
            this.TemplateBodyParseTree = GetTemplateContext(templateBody, this.Source);
        }

        private void ExtractNameAndParameters()
        {
            var templateNameLine = templateParseTree.templateNameLine().TEMPLATE_NAME_LINE().GetText();
            var hashIndex = templateNameLine.IndexOf('#');
            templateNameLine = templateNameLine.Substring(hashIndex + 1).Trim();

            var templateName = templateNameLine;
            var parameters = new List<string>();
            var leftBracketIndex = templateNameLine.IndexOf("(");
            if (leftBracketIndex >= 0)
            {
                templateName = templateNameLine.Substring(0, leftBracketIndex).Trim();
                if (templateNameLine.EndsWith(")"))
                {
                    var parameterString = templateNameLine.Substring(leftBracketIndex + 1, templateNameLine.Length - leftBracketIndex - 2);
                    parameters = parameterString.Split(',').Select(u => u.Trim()).ToList();
                }
            }

            this.Name = templateName;
            this.Parameters = parameters;
        }

        private LGTemplateParser.TemplateBodyContext GetTemplateContext(string text, string id)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var input = new AntlrInputStream(text);
            var lexer = new LGTemplateLexer(input);
            lexer.RemoveErrorListeners();

            var tokens = new CommonTokenStream(lexer);
            var parser = new LGTemplateParser(tokens);
            parser.RemoveErrorListeners();
            var listener = new ErrorListener(id);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.templateBody();
        }
    }
}
