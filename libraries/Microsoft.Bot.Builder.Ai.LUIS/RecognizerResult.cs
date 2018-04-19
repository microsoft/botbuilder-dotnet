using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Contains intent recognizer results.
    /// </summary>
    public class RecognizerResult
    {
        /// <summary>
        /// The query sent to the intent regocnizer.
        /// </summary>
        [JsonProperty("text")]
        public string Text { set; get; }

        /// <summary>
        /// The altered query used by the intent recognizer to extract intent and entities.
        /// </summary>
        [JsonProperty("alteredText")]
        public string AlteredText { set; get; }

        /// <summary>
        /// The intents found in the query text.
        /// </summary>
        [JsonProperty("intents")]
        public JObject Intents { get; set; }

        /// <summary>
        /// The entities found in the query text.
        /// </summary>
        [JsonProperty("entities")]
        public JObject Entities { get; set; }
    }
}
