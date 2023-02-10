// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specifies Teams targeted meeting notification.
    /// </summary>
    public class TargetedMeetingNotification : MeetingNotification<TargetedMeetingNotificationValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetedMeetingNotification"/> class.
        /// </summary>
        public TargetedMeetingNotification()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetedMeetingNotification"/> class.
        /// </summary>
        /// <param name="targetedMeetingNotificationValue">The value of the TargetedMeetingNotification.</param>
        public TargetedMeetingNotification(TargetedMeetingNotificationValue targetedMeetingNotificationValue)
        {
            Value = targetedMeetingNotificationValue;
        }

        /// <summary>
        /// Gets or sets Teams Bot meeting notification channel data.
        /// </summary>
        /// <value>
        /// Teams Bot meeting notification channel data.
        /// </value>
        [JsonProperty("channelData")]
        public MeetingNotificationChannelData ChannelData { get; set; }
    }
}
