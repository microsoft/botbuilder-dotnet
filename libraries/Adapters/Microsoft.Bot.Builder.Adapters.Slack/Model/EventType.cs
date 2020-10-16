// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    /// <summary>
    /// Represents a Slack Event Type object https://api.slack.com/events-api#receiving_events.
    /// </summary>
    public class EventType
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        public string Ts { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public JObject Item { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        [JsonProperty(PropertyName = "event_ts")]
        public string EventTs { get; set; }

        public string Channel { get; set; }

        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "bot_id")]
        public string BotId { get; set; }

        [JsonProperty(PropertyName = "thread_ts")]
        public string ThreadTs { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, JToken> AdditionalProperties { get; } = new Dictionary<string, JToken>();
    }
}
