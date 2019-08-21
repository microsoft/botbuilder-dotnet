using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class OrdinalEntityRecognizer : BaseEntityRecognizer
    {
        public OrdinalEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizeOrdinal(text, culture);
        }
    }
}
