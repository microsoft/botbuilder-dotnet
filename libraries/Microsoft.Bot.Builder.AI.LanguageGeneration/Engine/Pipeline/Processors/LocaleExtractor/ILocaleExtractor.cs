using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal interface ILocaleExtractor
    {
        string ExtractLocale(Activity activity);
    }
}
