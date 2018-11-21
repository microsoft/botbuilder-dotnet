using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Microsoft.Recognizers.Text.Sequence;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class PhoneNumberEntityRecognizer : BaseEntityRecognizer
    {
        public PhoneNumberEntityRecognizer() { }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizePhoneNumber(text, culture);
        }
    }
}
