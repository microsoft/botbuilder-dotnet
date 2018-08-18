using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class CompositeResponse : ICompositeResponse
    {
        public CompositeResponse()
        {
            TemplateResolutions = new Dictionary<string, string>();
        }
        public IDictionary<string, string> TemplateResolutions { get; set; }
    }
}
