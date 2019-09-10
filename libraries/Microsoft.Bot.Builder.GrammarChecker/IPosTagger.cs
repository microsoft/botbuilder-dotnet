using System.Collections.Generic;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public interface IPosTagger
    {
        List<PosFeature> PosTagging(string sentence);
    }
}
