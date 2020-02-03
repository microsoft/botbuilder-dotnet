// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions.Memory;

namespace Microsoft.Bot.Expressions.Properties
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

            var newContent = $"# {fakeTemplateId} \r\n - {inlineStr}";

            var newLgFile = LGParser.ParseText(newContent, lgFile.Id, lgFile.ImportResolver);

            var memory = SimpleObjectMemory.Wrap(scope);
            var allTemplates = lgFile.AllTemplates.Union(newLgFile.AllTemplates).ToList();
            var evaluator = new Evaluator(allTemplates, lgFile.ExpressionEngine);
            return evaluator.EvaluateTemplate(fakeTemplateId, new CustomizedMemory(memory));
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
