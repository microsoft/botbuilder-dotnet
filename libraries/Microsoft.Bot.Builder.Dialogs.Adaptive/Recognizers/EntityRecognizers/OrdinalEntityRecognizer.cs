using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class OrdinalEntityRecognizer : TextEntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OrdinalEntityRecognizer";

        public OrdinalEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizeOrdinal(text, culture);
        }
    }
}
