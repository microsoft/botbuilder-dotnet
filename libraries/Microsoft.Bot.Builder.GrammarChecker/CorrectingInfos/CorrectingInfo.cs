using Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public class CorrectingInfo
    {
        public int WordIndex;
        public NumberFeature NumberInfo;
        public SubjectVerbFeature VerbInfo;

        public CorrectingInfo()
        {
            WordIndex = -1;
            NumberInfo = new NumberFeature();
            VerbInfo = new SubjectVerbFeature();
        }
    }
}
