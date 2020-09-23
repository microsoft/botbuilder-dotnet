using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes percentage input.
    /// </summary>
    public class PercentageEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.PercentageEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentageEntityRecognizer"/> class.
        /// </summary>
        public PercentageEntityRecognizer()
        {
        }

        /// <summary>
        /// Percentage recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizePercentage(text, culture);
        }
    }
}
