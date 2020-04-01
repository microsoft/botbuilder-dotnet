// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <param name="parseTree">The parse tree of this template.</param>
        /// <param name="lgfileContent">lg file content.</param>
        /// <param name="source">Source of this template.</param>
        internal Template(LGFileParser.TemplateDefinitionContext parseTree, string lgfileContent, string source = "")
        {
            ParseTree = parseTree;
            Source = source;

            Name = ExtractName();
            Parameters = ExtractParameters();
            Body = ExtractBody(lgfileContent);
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

        /// <summary>
        /// Get the startLine and stopLine of template.
        /// </summary>
        /// <returns>template content range.</returns>
        public (int startLine, int stopLine) GetTemplateRange()
        {
            var startLine = ParseTree.Start.Line - 1;
            var stopLine = ParseTree.Stop.Line - 1;
            if (ParseTree?.Parent?.Parent is LGFileParser.FileContext fileContext)
            {
                var templateDefinitions = fileContext
                        .paragraph()
                        .Select(u => u.templateDefinition())
                        .Where(u => u != null)
                        .ToList();
                var currentIndex = -1;
                for (var i = 0; i < templateDefinitions.Count; i++)
                {
                    if (templateDefinitions[i] == ParseTree)
                    {
                        currentIndex = i;
                        break;
                    }
                }

                if (currentIndex >= 0 && currentIndex < templateDefinitions.Count - 1)
                {
                    // in the middle of templates
                    stopLine = templateDefinitions[currentIndex + 1].Start.Line - 2;
                }
                else
                {
                    // last item
                    stopLine = fileContext.Stop.Line - 1;
                }
            }

            if (stopLine <= startLine)
            {
                stopLine = startLine;
            }

            return (startLine, stopLine);
        }

        private string ExtractBody(string lgfileContent)
        {
            var (startLine, stopLine) = GetTemplateRange();
            return startLine >= stopLine ? string.Empty : GetRangeContent(lgfileContent, startLine + 1, stopLine);
        }

        private string ExtractName()
        {
            var name = ParseTree.templateNameLine().templateName()?.GetText();
            return name ?? string.Empty;
        }

        private List<string> ExtractParameters()
        {
            var parameters = ParseTree.templateNameLine().parameters();
            if (parameters != null)
            {
                return parameters.IDENTIFIER().Select(param => param.GetText()).ToList();
            }

            return new List<string>();
        }

        private string GetRangeContent(string originString, int startLine, int stopLine)
        {
            if (startLine < 0 || startLine > stopLine)
            {
                throw new Exception("index out of range.");
            }

            var destList = new List<string>();

            using (var strReader = new StringReader(originString))
            {
                string aLine;
                var lineNumber = -1;
                while ((aLine = strReader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (lineNumber >= startLine && lineNumber <= stopLine)
                    {
                        destList.Add(aLine);
                    }
                }
            }

            return string.Join("\r\n", destList);
        }
    }
}
