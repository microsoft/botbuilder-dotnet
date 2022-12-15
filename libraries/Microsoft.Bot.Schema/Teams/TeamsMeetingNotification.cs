// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies meeting notification including channel data, type and value. 
    /// </summary>
    public partial class TeamsMeetingNotification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingNotification"/> class.
        /// </summary>
        public TeamsMeetingNotification()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets Activty type.
        /// </summary>
        /// <value>
        /// Activity type.
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "targetedMeetingNotification";

        /// <summary>
        /// Gets or sets Teams meeting notification information.
        /// </summary>
        /// <value>
        /// Teams meeting notification information.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public TeamsMeetingNotificationInfo Value { get; set; }

        /// <summary>
        /// Gets or sets Teams meeting notification channel data.
        /// </summary>
        /// <value>
        /// Teams meeting notification channel data.
        /// </value>
        [JsonProperty(PropertyName = "channelData")]
        public TeamsMeetingNotificationChannelData ChannelData { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
