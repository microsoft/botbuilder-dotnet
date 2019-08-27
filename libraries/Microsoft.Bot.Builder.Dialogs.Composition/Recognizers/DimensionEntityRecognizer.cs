using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class DimensionEntityRecognizer : BaseEntityRecognizer
    {
        public DimensionEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberWithUnitRecognizer.RecognizeDimension(text, culture);
        }
    }
}
