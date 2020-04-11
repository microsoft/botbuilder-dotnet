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
        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        /// <param name="templateName">Template name without parameters.</param>
        /// <param name="parameters">Parameter list.</param>
        /// <param name="templateBody">Template content.</param>
        /// <param name="startLine">StartLine of template (zero based).</param>
        /// <param name="stopLine">Stop line of template (zero based).</param>
        /// <param name="source">Source of this template.</param>
        internal Template(
            string templateName,
            List<string> parameters,
            string templateBody,
            int startLine,
            int stopLine,
            string source = "")
        {
            this.Name = templateName ?? string.Empty;
            this.Parameters = parameters ?? new List<string>();
            this.Body = templateBody ?? string.Empty;
            this.StartLine = startLine;
            this.StopLine = stopLine;
            this.Source = source ?? string.Empty;
        }

        public int StartLine { get; }

        public int StopLine { get; }

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
        /// Gets text format of Body of this template. All content except Name and Parameters.
        /// </summary>
        /// <value>
        /// Text format of Body of this template. All content except Name and Parameters.
        /// </value>
        public string Body { get; }

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
        public LGTemplateParser.TemplateBodyContext TemplateBodyParseTree => GetTemplateContext();

        public override string ToString() => $"[{Name}({string.Join(", ", Parameters)})]\"{Body}\"";

        private LGTemplateParser.TemplateBodyContext GetTemplateContext()
        {
            if (string.IsNullOrEmpty(Source))
            {
                return null;
            }

            var input = new AntlrInputStream(Body);
            var lexer = new LGTemplateLexer(input);
            lexer.RemoveErrorListeners();

            var tokens = new CommonTokenStream(lexer);
            var parser = new LGTemplateParser(tokens);
            parser.RemoveErrorListeners();
            var listener = new TemplateErrorListener(Source, StartLine);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.templateBody();
        }
    }
}
