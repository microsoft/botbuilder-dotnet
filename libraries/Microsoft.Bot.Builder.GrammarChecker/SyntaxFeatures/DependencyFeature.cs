namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures
{
    public enum DependencyTag
    {
        other,       // Word has other dependency tag.
        aux,         // auxiliary
        auxpass,     // passive auxiliary
        conj,        // conjunct
        cop,         // copula
        pobj,        // object of a preposition
        nsubj,       // nominal subject
        rcmod,       // relative clause modifier
        num,         // numeric modifier
        prep,        // prepositional modifier
        amod,        //  adjectival modifier
    }

    /// <summary>
    /// Dependency feature, these features are word level.
    /// </summary>
    public class DependencyFeature
    {
        public DependencyFeature()
        {
            PosFeature = new PosFeature();
            WordIndex = -1;
            SubjectIndex = -1;
            NumericModifierIndex = -1;
        }

        public int WordIndex { get; set; }

        public int SubjectIndex { get; set; }

        public int NumericModifierIndex { get; set; }

        public PosFeature PosFeature { get; set; }
    }
}
