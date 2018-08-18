using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal interface ICompositeResponse
    {
        IDictionary<string, string> TemplateResolutions { get; set; }
    }
}
