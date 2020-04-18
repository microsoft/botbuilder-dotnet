using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class ModalBlock
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "selected_option")]
        public StaticSelectedOption SelectedOption { get; set; }

        [JsonProperty(PropertyName = "selected_options")]
        public List<StaticSelectedOption> SelectedOptions { get; } = new List<StaticSelectedOption>();
    }
}
