using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.LanguageGeneration.GrmCheckers
{
    public class VoidChecker : ITemplateEngineMiddleware
    {
        // a VoidChecker don't do anything, just return the previous result
        public string Replace(string previous, TemplateReplacementContext context)
        {
            return previous;
        }
    }
}
