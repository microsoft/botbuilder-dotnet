using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class NumberRangeEntityRecognizer : EntityRecognizer
    {
        public NumberRangeEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizeNumberRange(text, culture);
        }
    }
}
