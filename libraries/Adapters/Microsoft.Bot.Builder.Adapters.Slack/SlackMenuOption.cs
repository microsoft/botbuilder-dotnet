using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackMenuOption
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
