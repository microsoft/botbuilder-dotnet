using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class HashtagEntityRecognizer : BaseEntityRecognizer
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
