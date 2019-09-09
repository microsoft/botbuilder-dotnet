namespace Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos
{
    public enum Number
    {
        None, 
        Singular,
        Plural
    }

    public class NumberFeature
    {
        public int ReferencePosition;
        public Number Feature;

        public NumberFeature()
        {
            ReferencePosition = -1;
            Feature = Number.None;
        }
    }
}
