using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Translation.Model
{
    public class ErrorModel
    {
        [JsonProperty("error")]
        public ErrorMessage Error { get; set; }
    }
}
