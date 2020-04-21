using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class MentionEntityRecognizer : TextEntityRecognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MentionEntityRecognizer";

        public MentionEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeMention(text, culture);
        }
    }
}
