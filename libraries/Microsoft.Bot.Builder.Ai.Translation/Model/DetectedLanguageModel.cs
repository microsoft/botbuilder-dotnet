using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class DetectedLanguageModel
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }
    }
}
