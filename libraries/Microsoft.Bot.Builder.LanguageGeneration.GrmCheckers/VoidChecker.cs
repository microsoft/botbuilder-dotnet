using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.LanguageGeneration.GrmCheckers
{
    public class VoidChecker : IOutputTransformer
    {
        // a VoidChecker don't do anything, just return the previous result
        public string Transform(string previous, OutputTransformationContext context)
        {
            return previous;
        }
    }
}
