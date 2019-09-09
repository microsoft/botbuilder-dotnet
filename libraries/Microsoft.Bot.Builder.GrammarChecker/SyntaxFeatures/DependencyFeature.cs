using Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos;

namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures
{
    public enum DependencyTag
    {
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
        other,       // Word has other dependency tag.
    }

    /// <summary>
    /// Dependency feature, these features are word level
    /// </summary>
    public class DependencyFeature
    {
        private POSTag m_PosTag;
        private int m_WordIndex;
        private int m_SubjectIndex;             // subject index of verb
        private int m_NumericModifierIndex;     // numeric modifier index of noun

        public DependencyFeature()
        {
            m_PosTag = POSTag.OTHER;
            m_WordIndex = -1;
            m_SubjectIndex = -1;
            m_NumericModifierIndex = -1;
        }

        public int WordIndex
        {
            get => m_WordIndex;
            set => m_WordIndex = value;
        }

        public int SubjectIndex
        {
            get => m_SubjectIndex;
            set => m_SubjectIndex = value;
        }

        public int NumericModifierIndex
        {
            get => m_NumericModifierIndex;
            set => m_NumericModifierIndex = value;
        }

        public POSTag PosTag
        {
            get => m_PosTag;
            set => m_PosTag = value;
        }
    }
}
