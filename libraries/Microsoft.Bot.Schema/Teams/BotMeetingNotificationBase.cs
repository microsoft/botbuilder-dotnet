// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies Bot meeting notification base including channel data and type.
    /// </summary>
    public class BotMeetingNotificationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotMeetingNotificationBase"/> class.
        /// </summary>
        protected BotMeetingNotificationBase()
        {
        }

        /// <summary>
        /// Gets or sets type of Bot meeting notification.
        /// </summary>
        /// <value>
        /// Bot meeting notification type.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets Teams Bot meeting notification channel data.
        /// </summary>
        /// <value>
        /// Teams Bot meeting notification channel data.
        /// </value>
        [JsonProperty("channelData")]
        public BotMeetingNotificationChannelData ChannelData { get; set; }
    }
}
