using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes yes/no confirmation style input.
    /// </summary>
    public class ConfirmationEntityRecognizer : EntityRecognizer
    {
        public ConfirmationEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return ChoiceRecognizer.RecognizeBoolean(text, culture);
        }
    }
}
