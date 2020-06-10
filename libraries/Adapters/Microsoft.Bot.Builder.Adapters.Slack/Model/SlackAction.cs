// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    public class SlackAction
    {
        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        [JsonProperty(PropertyName = "block_id")]
        public string BlockId { get; set; }

        [JsonProperty(PropertyName = "text")]
        public object Text { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "action_ts")]
        public string ActionTs { get; set; }

        [JsonProperty(PropertyName = "selected_option")]
        public SelectOption SelectedOption { get; set; }

        [JsonProperty(PropertyName = "selected_options")]
        public List<SelectOption> SelectedOptions { get; } = new List<SelectOption>();
    }
}
