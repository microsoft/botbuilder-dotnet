using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Blocks
{
    /// <summary>
    /// Represents a Slack Datepicker element https://api.slack.com/reference/block-kit/block-elements#datepicker
    /// </summary>
    public class DatepickerElement : BlockElement
    {
        public string Type => "datepicker";

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        public TextObject Placeholder { get; set; }

        [JsonProperty(PropertyName = "initial_date")]
        public string InitialDate { get; set; }

        public ConfirmObject Confirm { get; set; }
    }
}
