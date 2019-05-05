using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.LanguageGeneration.GrmCheckers
{
    public class VoidChecker : ITemplateEngineMiddleware
    {
        // a VoidChecker don't do anything, just return the rawResult
        public string OnReplace(string rawResult, List<Tuple<int, int>> replacements)
        {
            return rawResult;
        }
    }
}
