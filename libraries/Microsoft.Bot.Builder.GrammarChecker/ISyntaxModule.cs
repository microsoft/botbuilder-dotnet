using System.Collections.Generic;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public interface ISyntaxModule
    {
        bool PosTagging(string sentence, out List<object> tags);

        bool DependencyParsing(List<object> tags, out List<DependencyFeature> dependencyFeatures);
    }
}
