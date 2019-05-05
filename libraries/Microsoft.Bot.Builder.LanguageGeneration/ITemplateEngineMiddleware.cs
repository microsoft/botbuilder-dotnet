using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public interface ITemplateEngineMiddleware
    {
        string OnReplace(string rawResult, List<Tuple<int, int>> replacements);
    }

}
