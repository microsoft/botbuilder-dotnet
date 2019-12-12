using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGFile
    {
        public IList<LGTemplate> Templates { get; }

        public IList<LGImport> Imports { get; }

        public IList<LGFile> Reference
        {
            get
            {   
                // TODO. From imports
                return null; 
            }
        }

        public IList<Diagnostic> Diagnostics { get;  }

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
