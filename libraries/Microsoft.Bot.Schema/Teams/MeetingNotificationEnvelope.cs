// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies the container for what is required to send a meeting notification to recipients.
    /// </summary>
    public partial class MeetingNotificationEnvelope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingNotificationEnvelope"/> class.
        /// </summary>
        public MeetingNotificationEnvelope()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the type of this notification container, which is 'notification'.
        /// </summary>
        /// <value>The type of this notification container.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "notification";

        /// <summary>
        /// Gets or sets the name of this notification.
        /// </summary>
        /// <value>The name of this notification.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = "application/vnd.microsoft.teams.meeting.notification";

        /// <summary>
        /// Gets or sets the channel for this notification.
        /// </summary>
        /// <value>The channel for this notification.</value>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; } = "msteams";

        /// <summary>
        /// Gets or sets the <see cref="MeetingNotification"/> for this notification.
        /// </summary>
        /// <value>The value for this notification.</value>
        [JsonProperty(PropertyName = "value")]
        public MeetingNotification Value { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MeetingNotificationData"/> for this notification. Including the list of <see cref="OnBehalfOf"/>.
        /// </summary>
        /// <value>The channelData for this notification.</value>
        [JsonProperty(PropertyName = "channelData")]
        public MeetingNotificationData NotificationData { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
