using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Extension methods for AdaptiveDialog.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Use to evaluate an inline template str.
        /// </summary>
        /// <param name="lgFile">lg file.</param>
        /// <param name="inlineStr">inline string which will be evaluated.</param>
        /// <param name="scope">scope object or JToken.</param>
        /// <returns>Evaluate result.</returns>
        public static object Evaluate(this LGFile lgFile, string inlineStr, object scope = null)
        {
            if (inlineStr == null)
            {
                throw new ArgumentException("inline string is null.");
            }

            CheckErrors(lgFile.AllDiagnostics);

            // wrap inline string with "# name and -" to align the evaluation process
            var fakeTemplateId = Guid.NewGuid().ToString();
            var multiLineMark = "```";

            inlineStr = !inlineStr.Trim().StartsWith(multiLineMark) && inlineStr.Contains('\n')
                   ? $"{multiLineMark}{inlineStr}{multiLineMark}" : inlineStr;

            var wrappedStr = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var newContent = $"{lgFile.Content}\r\n{wrappedStr}";

            var newLgFile = LGParser.ParseText(newContent, lgFile.Id, lgFile.ImportResolver);
            return newLgFile.EvaluateTemplate(fakeTemplateId, scope);
        }

        private static void CheckErrors(IList<Diagnostic> diagnostics)
        {
            if (diagnostics != null)
            {
                var errors = diagnostics.Where(u => u.Severity == DiagnosticSeverity.Error).ToList();
                if (errors.Count != 0)
                {
                    throw new Exception(string.Join("\n", errors));
                }
            }
        }
    }
}
