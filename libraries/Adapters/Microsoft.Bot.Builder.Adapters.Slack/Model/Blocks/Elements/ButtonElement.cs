using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Button Block Element https://api.slack.com/reference/block-kit/block-elements#button
    /// </summary>
    public class ButtonElement : BlockElement
    {
        public string Type => "button";

        public TextObject Text { get; set; }

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        public string Url { get; set; }

        public string Value { get; set; }

        public string Style { get; set; }

        public ConfirmObject Confirm { get; set; }
    }
}
