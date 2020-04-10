using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class GuidEntityRecognizer : TextEntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.GuidEntityRecognizer";

        public GuidEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeGUID(text, culture);
        }
    }
}
