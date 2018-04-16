using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    public class RecognizerResult
    {
        [JsonProperty("text")]
        public string Text { set; get; }

        [JsonProperty("alteredText")]
        public string AlteredText { set; get; }

        [JsonProperty("intents")]
        public JObject Intents { get; set; }

        [JsonProperty("entities")]
        public JObject Entities { get; set; }
    }
}
