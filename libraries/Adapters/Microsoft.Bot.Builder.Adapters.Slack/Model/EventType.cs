// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model
{
    /// <summary>
    /// Represents a Slack Event Type object https://api.slack.com/events-api#receiving_events.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class EventType
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        public string Ts { get; set; }

        public JObject Item { get; } = new JObject();

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
