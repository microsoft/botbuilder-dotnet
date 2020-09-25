using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes ordinal input.
    /// </summary>
    public class OrdinalEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OrdinalEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalEntityRecognizer"/> class.
        /// </summary>
        public OrdinalEntityRecognizer()
        {
        }

        /// <summary>
        /// Ordinal recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizeOrdinal(text, culture);
        }
    }
}
