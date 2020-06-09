using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Composition;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Radiobutton Group Block Element https://api.slack.com/reference/block-kit/block-elements#radiobuttons
    /// </summary>
    public class RadiobuttonGroupElement : BlockElement
    {
        public string Type => "radio_buttons";

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        public List<OptionObject> Options { get; } = new List<OptionObject>();

        [JsonProperty(PropertyName = "initial_option")]
        public OptionObject InitialOption { get; set;  }

        public OptionObject Confirm { get; set; }
    }
}
