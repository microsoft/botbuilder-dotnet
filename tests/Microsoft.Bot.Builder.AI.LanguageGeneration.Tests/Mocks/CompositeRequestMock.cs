using System;
using System.Collections.Generic;
using System.Text;
using DialogFoundation.Backend.LG;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Engine;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests.Mocks
{
    internal class CompositeRequestMock : ICompositeRequest
    {
        public IDictionary<string, LGRequest> Requests { get; set; }
    }
}
