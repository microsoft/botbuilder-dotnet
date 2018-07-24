using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class Alignment
    {
        [JsonProperty("proj")]
        public string Projection { get; set; }
    }
}
