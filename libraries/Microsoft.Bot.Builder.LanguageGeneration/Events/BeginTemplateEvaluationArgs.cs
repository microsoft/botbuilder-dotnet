using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class BeginTemplateEvaluationArgs : LGEventArgs
    {
        public string Type { get; } = LGEventTypes.BeginTemplateEvaluation;

        public string TemplateName { get; set; }
    }
}
