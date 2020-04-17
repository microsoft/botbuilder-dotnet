using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class StaticSelectedOption
    {
        [JsonProperty(PropertyName = "value")]
        public string SelectedValue { get; set; }
    }
}
