using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGEventArgs : EventArgs
    {
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets LGContext, may include evaluation stack.
        /// </summary>
        /// <value>
        /// LGContext.
        /// </value>
        public object Context { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class BeginTemplateEvaluationArgs : LGEventArgs
#pragma warning restore SA1402 // File may only contain a single type
    {
        public string Type { get; } = "beginTemplateEvaluation";

        public string TemplateName { get; set; }
    }
}
