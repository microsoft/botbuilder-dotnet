using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Composition;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents Slack Multi Select Menu Element https://api.slack.com/reference/block-kit/block-elements#static_multi_select
    /// </summary>
    public class MultiSelectMenuElement : BlockElement
    {
        public string Type => "multi_static_select";

        public TextObject Placeholder { get; set; }

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        public List<OptionObject> Options { get; set; }

        [JsonProperty(PropertyName = "option_groups")]
        public List<OptionGroupObject> OptionGroups { get; set; }

        [JsonProperty(PropertyName = "initial_options")]
        public List<OptionObject> InitialOptions { get; set; }

        public ConfirmObject Confirm { get; set; }

        [JsonProperty(PropertyName = "max_selected_items")]
        public int? MaxSelectedItems { get; set; }
    }
}
