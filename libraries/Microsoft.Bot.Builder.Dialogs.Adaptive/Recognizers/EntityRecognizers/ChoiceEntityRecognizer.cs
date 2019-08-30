using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class ChoiceEntityRecognizer : EntityRecognizer
    {
        public ChoiceEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return ChoiceRecognizer.RecognizeBoolean(text, culture);
        }
    }
}
