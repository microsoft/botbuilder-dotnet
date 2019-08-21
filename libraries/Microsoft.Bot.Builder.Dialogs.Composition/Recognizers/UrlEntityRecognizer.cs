using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class UrlEntityRecognizer : BaseEntityRecognizer
    {
        public UrlEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeURL(text, culture);
        }
    }
}
