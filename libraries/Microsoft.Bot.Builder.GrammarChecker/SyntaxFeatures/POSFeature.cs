namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures
{
    public enum POSTag
    {
        VERB,        // Word is a verb.
        NOUN,        // Word is a noun.
        ADJ,         // Word is adjective
        PRON,        // Word is pronoun.
        NUM,         // Word is number.
        OTHER,       // Word has other pos tag.
    }

    public enum VerbPOSTag
    {
        VB,          // verb base form
        VBD,         // verb past tense
        VBG,         // verb gerund
        VBN,         // verb past participle
        VBP,         // verb non-3sg pres
        VBZ,         // verb 3sg pres
    }

    public enum AdjectivePOSTag
    {
        JJ,          // adjective
        JJR,         // adj., comparative
        JJS,         // adj., superlative
    }

    public enum NounPOSTag
    {
        NN,          // noun, sing. or mass
        NNS,         // noun, plural
        NNP,         // proper noun, sing
        NNPS,        // proper noun, plural
    }

    public enum PronPOSTag
    {
        PRP,         // pronoun
    }
}
