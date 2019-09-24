// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackRequestBody
    {
        [JsonProperty(PropertyName = "challenge")]
        public string Challenge { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "team_id")]
        public string TeamId { get; set; }

        [JsonProperty(PropertyName = "api_app_id")]
        public string ApiAppId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "event_id")]
        public string EventId { get; set; }

        [JsonProperty(PropertyName = "event_time")]
        public string EventTime { get; set; }

        [JsonProperty(PropertyName = "authed_users")]
        public List<string> AuthedUsers { get; set; }

        [JsonProperty(PropertyName = "trigger_id")]
        public string TriggerId { get; set; }

        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public SlackEvent Payload { get; set; }

        [JsonProperty(PropertyName = "event")]
        public SlackEvent Event { get; set; }
    }
}
