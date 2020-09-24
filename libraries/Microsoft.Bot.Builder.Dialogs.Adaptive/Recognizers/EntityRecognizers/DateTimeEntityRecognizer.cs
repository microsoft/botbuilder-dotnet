using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes DateTime input.
    /// </summary>
    public class DateTimeEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.DateTimeEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeEntityRecognizer"/> class.
        /// </summary>
        public DateTimeEntityRecognizer()
        {
        }

        /// <summary>
        /// DateTime recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return DateTimeRecognizer.RecognizeDateTime(text, culture);
        }
    }
}
