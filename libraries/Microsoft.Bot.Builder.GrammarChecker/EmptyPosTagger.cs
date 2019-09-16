using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public class EmptyPosTagger : IPosTagger
    {
        public List<PosFeature> PosTagging(string sentence)
        {
            // TODO: add doc to point users the usage of syntaxnet or other models
            // Please refer here to find the usage of pos taggeing of Syntaxnet or other NLP models
            throw new NotImplementedException();
        }
    }
}
