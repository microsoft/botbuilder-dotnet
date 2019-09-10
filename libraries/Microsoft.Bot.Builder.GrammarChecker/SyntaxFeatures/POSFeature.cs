namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures
{
    public enum BasicPosTag
    {
        OTHER,       // Word has other pos tag.
        VERB,        // Word is a verb.
        NOUN,        // Word is a noun.
        ADJ,         // Word is adjective
        PRON,        // Word is pronoun.
        NUM,         // Word is number.
    }

    public enum VerbPosTag
    {
        OTHER,
        VB,          // verb base form
        VBD,         // verb past tense
        VBG,         // verb gerund
        VBN,         // verb past participle
        VBP,         // verb non-3sg pres
        VBZ,         // verb 3sg pres
    }

    public enum AdjectivePosTag
    {
        OTHER,
        JJ,          // adjective
        JJR,         // adj., comparative
        JJS,         // adj., superlative
    }

    public enum NounPosTag
    {
        OTHER,
        NN,          // noun, sing. or mass
        NNS,         // noun, plural
        NNP,         // proper noun, sing
        NNPS,        // proper noun, plural
    }

    public enum PronPosTag
    {
        OTHER,
        PRP,         // pronoun
    }

    public enum NumPosTag
    {
        OTHER,
        CD,          // num
    }

    public class PosFeature
    {
        public PosFeature()
        {
            WordIndex = -1;
            WordText = string.Empty;
            OtherBasicTag = string.Empty;
            OtherSubTag = string.Empty;
        }

        public int WordIndex { get; set; }

        public string WordText { get; set; }

        public BasicPosTag BasicPosTag { get; set; }

        public VerbPosTag VerbPosTag { get; set; }

        public AdjectivePosTag AdjPosTag { get; set; }

        public NounPosTag NounPosTag { get; set; }

        public PronPosTag PronPosTag { get; set; }

        public NumPosTag NumPosTag { get; set; }

        public string OtherBasicTag { get; set; }

        public string OtherSubTag { get; set; }
    }
}
