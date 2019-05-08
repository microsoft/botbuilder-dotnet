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
        /// Name of the template, what's followed by '#' in a LG file.
        /// </summary>
        public string Name;

        /// <summary>
        /// Paramter list of this template.
        /// </summary>
        public List<string> Paramters;

        /// <summary>
        /// Source of this template, source file path if it's from a certain file.
        /// </summary>
        public string Source;

        /// <summary>
        /// Gets or sets text format of Body of this template. All content except Name and Parameters.
        /// </summary>
        /// <value>
        /// Text format of Body of this template. All content except Name and Parameters.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// The parse tree of this template.
        /// </summary>
        public LGFileParser.TemplateDefinitionContext ParseTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGTemplate"/> class.
        /// </summary>
        /// <param name="parseTree">The parse tree of this template.</param>
        /// <param name="source">Source of this template.</param>
        public LGTemplate(LGFileParser.TemplateDefinitionContext parseTree, string source = "")
        {
            ParseTree = parseTree;
            Source = source;

            Name = ExtractName(parseTree);
            Paramters = ExtractParameters(parseTree);
            Body = ExtractBody(parseTree);
        }

        public static IList<LGTemplate> AddSource(IList<LGTemplate> lgtemplates, string source)
        {
            if (lgtemplates == null)
            {
                return null;
            }

            return lgtemplates.Select(u =>
            {
                u.Source = source;
                return u;
            }).ToList();
        }

        private string ExtractBody(LGFileParser.TemplateDefinitionContext parseTree) => parseTree.templateBody().GetText();

        private string ExtractName(LGFileParser.TemplateDefinitionContext parseTree)
        {
            return parseTree.templateNameLine().templateName().GetText();
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
    }
}
