using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class GuidEntityRecognizer : BaseEntityRecognizer
    {
        public GuidEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeGUID(text, culture);
        }
    }
}
