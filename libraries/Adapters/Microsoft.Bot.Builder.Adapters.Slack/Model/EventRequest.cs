// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    /// <summary>
    /// Represents an incoming request from the Event API https://api.slack.com/events-api#receiving_events.
    /// </summary>
    public class EventRequest
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "team_id")]
        public string TeamId { get; set; }

        [JsonProperty(PropertyName = "api_app_id")]
        public string ApiAppId { get; set; }

        [JsonProperty(PropertyName = "event")]
        public EventType Event { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "authed_users")]
        public List<string> AuthedUsers { get; } = new List<string>();

        [JsonProperty(PropertyName = "event_id")]
        public string EventId { get; set; }

        [JsonProperty(PropertyName = "event_time")]
        public string EventTime { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, JToken> AdditionalProperties { get; } = new Dictionary<string, JToken>();
    }
}
