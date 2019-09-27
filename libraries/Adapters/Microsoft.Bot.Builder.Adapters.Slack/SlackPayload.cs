// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using SlackAPI;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackPayload
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public Channel Channel { get; set; }

        [JsonProperty(PropertyName = "thread_ts")]
        public string ThreadTs { get; set; }

        [JsonProperty(PropertyName = "team")]
        public Team Team { get; set; }

        [JsonProperty(PropertyName = "message")]
        public SlackMessage Message { get; set; }

        [JsonProperty(PropertyName = "user")]
        public User User { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public List<SlackAction> Actions { get; } = new List<SlackAction>();
    }
}
