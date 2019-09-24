// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Slack
{
    public class SlackEvent
    {
        [JsonProperty(PropertyName = "client_msg_id")]
        public string ClientMsgId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "subtype")]
        public string SubType { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "ts")]
        public string TimeStamp { get; set; }

        [JsonProperty(PropertyName = "team")]
        public string Team { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public string Channel { get; set; }

        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "event_ts")]
        public string EventTS { get; set; }

        [JsonProperty(PropertyName = "channel_type")]
        public string ChannelType { get; set; }

        [JsonProperty(PropertyName = "thread_ts")]
        public string ThreadTS { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "bot_id")]
        public string BotId { get; set; }

        [JsonProperty(PropertyName = "actions")]
        public List<string> Actions { get; set; }

        [JsonProperty(PropertyName = "item")]
        public string Item { get; set; }

        [JsonProperty(PropertyName = "item_channel")]
        public string ItemChannel { get; set; }
    }
}
