using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class CurrencyEntityRecognizer : EntityRecognizer
    {
        public CurrencyEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberWithUnitRecognizer.RecognizeCurrency(text, culture);
        }
    }
}
