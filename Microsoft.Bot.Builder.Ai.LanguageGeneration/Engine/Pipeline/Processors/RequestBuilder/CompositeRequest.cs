using System;
using System.Collections.Generic;
using System.Text;
using DialogFoundation.Backend.LG;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class CompositeRequest : ICompositeRequest
    {
        public CompositeRequest()
        {
            Requests = new Dictionary<string, LGRequest>();
        }
        public IDictionary<string, LGRequest> Requests { get; set; }
    }
}
