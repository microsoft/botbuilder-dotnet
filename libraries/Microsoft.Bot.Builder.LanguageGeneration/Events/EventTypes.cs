using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class EventTypes
    {
        public const string Message = "message";
        public const string BeginTemplateEvaluation = "beginTemplateEvaluation";
        public const string BeginExpressionEvaluation = "beginExpressionEvaluation";
        public const string RegisteSourceMap = "registeSourceMap";
    }
}
