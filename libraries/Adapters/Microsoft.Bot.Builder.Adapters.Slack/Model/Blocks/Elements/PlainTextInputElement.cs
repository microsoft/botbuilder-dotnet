using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents Slack Plain Text Input Element https://api.slack.com/reference/block-kit/block-elements#input
    /// </summary>
    public class PlainTextInputElement : BlockElement
    {
        public string Type => "plain_text_input";

        public TextObject Placeholder { get; set; }

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        [JsonProperty(PropertyName = "initial_value")]
        public string InitialValue { get; set; }

        public bool Multiline { get; set; }

        [JsonProperty(PropertyName = "min_length")]
        public int? MinLength { get; set; }

        [JsonProperty(PropertyName = "max_length")]
        public int? MaxLength { get; set; }
    }
}
