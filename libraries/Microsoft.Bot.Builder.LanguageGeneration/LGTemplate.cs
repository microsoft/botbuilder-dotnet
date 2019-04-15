using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{

    /// <summary>
    /// Here is a data model that can easily understanded and used as the context or all kinds of visitors
    /// wether it's evalator, static checker, anayler.. etc
    /// </summary>
    public class LGTemplate
    {
        /// <summary>
        /// Name of the template, what's followed by '#' in a LG file
        /// </summary>
        public string Name;
        /// <summary>
        /// Paramter list of this template
        /// </summary>
        public List<string> Paramters;
        /// <summary>
        /// Source of this template, source file path if it's from a certain file
        /// </summary>
        public string Source;
        /// <summary>
        /// The parse tree of this template
        /// </summary>
        public LGFileParser.TemplateDefinitionContext ParseTree;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parseTree"></param>
        public LGTemplate(LGFileParser.TemplateDefinitionContext parseTree, string source = "")
        {
            ParseTree = parseTree;
            Source = source;

            Name = ExtractName(parseTree);
            Paramters = ExtractParameters(parseTree);
        }

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
