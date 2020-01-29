// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackAction
    {
        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        [JsonProperty(PropertyName = "block_id")]
        public string BlockId { get; set; }

        [JsonProperty(PropertyName = "text")]
        public object Text { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "action_ts")]
        public string ActionTs { get; set; }
    }
}
