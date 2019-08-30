using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class HashtagEntityRecognizer : EntityRecognizer
    {
        public HashtagEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeHashtag(text, culture);
        }
    }
}
