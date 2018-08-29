using System;
using System.Collections.Generic;
using System.Text;
using DialogFoundation.Backend.LG;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.API
{
    internal class LGResponseMock : LGResponse
    {
        public string SpokenSSML { get; set; }
        public string DisplayText { get; set; }
    }
}
