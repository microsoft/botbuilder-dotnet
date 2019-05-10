using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.LanguageGeneration;
using SurfaceRealizationV2;

namespace Microsoft.Bot.Builder.LanguageGeneration.GrmCheckers
{
    public class GrammarChecker : IOutputTransformer
    {
        public SurfaceRealizerENUS surfaceRealizer = null;

        public void Init() {
            surfaceRealizer = new SurfaceRealizerENUS();
        }

        public string Transform(string previous, OutputTransformationContext context)
        {
            var result = surfaceRealizer.CheckSentence(previous);
            return result;
        }
    }
}
