namespace Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos
{
    public enum SubjectVerb
    {
        None,
        Singular,
        Plural
    }

    public class SubjectVerbFeature
    {
        public int ReferencePosition;
        public SubjectVerb Feature;
        public Number SubjectNumber; // Whether subject word is a singular or plural number. e.g. two of them, here two is a plural number.
        public Number SubjectNumberFromPosTagging;

        public SubjectVerbFeature()
        {
            ReferencePosition = -1;
            Feature = SubjectVerb.None;
            SubjectNumber = Number.None;
            SubjectNumberFromPosTagging = Number.None;
        }
    }
}
