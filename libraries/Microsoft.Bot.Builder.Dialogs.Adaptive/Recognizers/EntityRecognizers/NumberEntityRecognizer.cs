using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes number input.
    /// </summary>
    public class NumberEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.NumberEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberEntityRecognizer"/> class.
        /// </summary>
        public NumberEntityRecognizer()
        {
        }

        /// <summary>
        /// Number recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizeNumber(text, culture);
        }
    }
}
