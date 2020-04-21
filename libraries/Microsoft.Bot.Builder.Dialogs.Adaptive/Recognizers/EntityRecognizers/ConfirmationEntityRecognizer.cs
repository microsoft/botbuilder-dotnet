using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes yes/no confirmation style input.
    /// </summary>
    public class ConfirmationEntityRecognizer : TextEntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ConfirmationEntityRecognizer";

        public ConfirmationEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return ChoiceRecognizer.RecognizeBoolean(text, culture);
        }
    }
}
