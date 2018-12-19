using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Engine;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests.Mocks
{
    internal class CompositeResponseMock : ICompositeResponse
    {
        public IDictionary<string, string> TemplateResolutions { get; set; }
    }
}
