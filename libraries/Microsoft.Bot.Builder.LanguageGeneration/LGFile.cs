using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGFile
    {
        public LGFile(IList<LGTemplate> templates = null, IList<LGImport> imports = null, IList<Diagnostic> diagnostics = null, IList<LGFile> references = null, string content = null)
        {
            Templates = templates ?? new List<LGTemplate>();
            Imports = imports ?? new List<LGImport>();
            Diagnostics = diagnostics ?? new List<Diagnostic>();
            References = references ?? new List<LGFile>();
            Content = content ?? string.Empty;
        }

        public IList<LGTemplate> AllTemplates
        {
            get
            {
                References.SelectMany
            }
        }

        public IList<LGTemplate> Templates { get; set; }

        public IList<LGImport> Imports { get; set; }

        public IList<LGFile> References { get; set; }

        public IList<Diagnostic> Diagnostics { get; set; }

        public string Content { get;  }

        public object EvaluateTemplate(string templateName, object memory = null)
        {
            // TODO
            return null;
        }

        /// <summary>
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="inlineStr">inline string which will be evaluated.</param>
        /// <param name="memory">scope object or JToken.</param>
        /// <returns>Evaluate result.</returns>
        public object Evaluate(string inlineStr, object memory = null)
        {
            // TODO
            return null;
        }

        public IList<string> ExpandTemplate(string templateName, object memory = null)
        {
            // TODO
            return null;
        }

        public AnalyzerResult AnalyzeTemplate(string templateName)
        {
            // TODO
            return null;
        }
    }
}
