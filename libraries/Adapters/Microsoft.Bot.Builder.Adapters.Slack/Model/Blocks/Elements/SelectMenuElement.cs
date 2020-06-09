using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Composition;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Select Menu Element https://api.slack.com/reference/block-kit/block-elements#select
    /// </summary>
    public class SelectMenuElement : BlockElement
    {
        public string Type => "datepicker";

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        public TextObject Placeholder { get; set; }

        public List<OptionObject> Options { get; set; }

        [JsonProperty(PropertyName = "option_groups")]
        public List<OptionGroupObject> OptionGroups { get; set; }

        [JsonProperty(PropertyName = "initial_option")]
        public OptionObject InitialOption { get; set; }

        public ConfirmObject Confirm { get; set; }
    }
}
