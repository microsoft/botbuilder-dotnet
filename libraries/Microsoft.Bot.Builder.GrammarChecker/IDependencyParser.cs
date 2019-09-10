using System.Collections.Generic;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public interface IDependencyParser
    {
        List<DependencyFeature> DependencyParsing(List<PosFeature> posFeatures);
    }
}
