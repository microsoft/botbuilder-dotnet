using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModel
{
    public class EmptyDependencyParser : IDependencyParser
    {
        public List<DependencyFeature> DependencyParsing(List<PosFeature> posFeatures)
        {
            // TODO: add doc to point users the usage of syntaxnet or other models
            // Please refer here to find the usage of dependency parsing of Syntaxnet or other NLP models
            throw new NotImplementedException();
        }
    }
}
