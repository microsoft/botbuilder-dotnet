using System.Collections.Generic;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes temperature input.
    /// </summary>
    public class TemperatureEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TemperatureEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureEntityRecognizer"/> class.
        /// </summary>
        public TemperatureEntityRecognizer()
        {
        }

        /// <summary>
        /// Temperature recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberWithUnitRecognizer.RecognizeTemperature(text, culture);
        }
    }
}
