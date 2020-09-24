using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes URL input.
    /// </summary>
    public class UrlEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.UrlEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlEntityRecognizer"/> class.
        /// </summary>
        public UrlEntityRecognizer()
        {
        }

        /// <summary>
        /// URL recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeURL(text, culture);
        }
    }
}
