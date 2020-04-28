using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class AgeEntityRecognizer : TextEntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.AgeEntityRecognizer";

        public AgeEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberWithUnitRecognizer.RecognizeAge(text, culture);
        }
    }
}
