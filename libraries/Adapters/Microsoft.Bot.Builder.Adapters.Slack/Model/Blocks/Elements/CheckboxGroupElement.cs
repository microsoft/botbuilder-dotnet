using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Adapters.Slack.Model.Composition;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Checkbox Group Block Element https://api.slack.com/reference/block-kit/block-elements#checkboxes
    /// </summary>
    public class CheckboxGroupElement : BlockElement
    {
        public string Type => "checkboxes";

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        public List<OptionObject> Options { get; } = new List<OptionObject>();

        public List<OptionObject> InitialOptions { get; } = new List<OptionObject>();

        public OptionObject Confirm { get; set; }
    }
}
