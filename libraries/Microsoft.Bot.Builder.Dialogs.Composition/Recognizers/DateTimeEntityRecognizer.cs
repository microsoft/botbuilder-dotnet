using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class DateTimeEntityRecognizer : BaseEntityRecognizer
    {
        public DateTimeEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return DateTimeRecognizer.RecognizeDateTime(text, culture);
        }
    }
}
