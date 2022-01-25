// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.Slack.Model.Events
{
    /// <summary>
    /// Represents a Slack Message event https://api.slack.com/events/message.
    /// </summary>
    [Obsolete("The Bot Framework Adapters will be deprecated in the next version of the Bot Framework SDK and moved to https://github.com/BotBuilderCommunity/botbuilder-community-dotnet. Please refer to their new location for all future work.")]
    public class MessageEvent : EventType
    {
        public string Text { get; set; }

        [JsonProperty(PropertyName = "channel_type")]
        public string ChannelType { get; set; }

        public string SubType { get; set; }
    }
}
