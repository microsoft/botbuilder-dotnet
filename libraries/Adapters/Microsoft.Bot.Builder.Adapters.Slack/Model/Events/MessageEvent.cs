// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Events
{
    /// <summary>
    /// Represents a Slack Message event https://api.slack.com/events/message.
    /// </summary>
    public class MessageEvent : EventType
    {
        public string Text { get; set; }

        [JsonProperty(PropertyName = "channel_type")]
        public string ChannelType { get; set; }

        public string SubType { get; set; }
    }
}
