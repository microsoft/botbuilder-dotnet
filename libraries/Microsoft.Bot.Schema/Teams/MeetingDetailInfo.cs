// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Teams meeting details.
    /// </summary>
    public partial class MeetingDetailInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingDetailInfo"/> class.
        /// </summary>
        public MeetingDetailInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingDetailInfo"/> class.
        /// </summary>
        /// <param name="id">The meeting's Id, encoded as a BASE64 string.</param>
        /// <param name="msGraphResourceId">The MsGraphResourceId, used specifically for MS Graph API calls.</param>
        /// <param name="scheduledStartTime">The meeting's scheduled start time, in UTC.</param>
        /// <param name="scheduledEndTime">The meeting's scheduled end time, in UTC.</param>
        /// <param name="joinUrl">The URL used to join the meeting.</param>
        /// <param name="title">The title of the meeting.</param>
        /// <param name="type">The meeting's type.</param>
        public MeetingDetailInfo(
            string id,
            string msGraphResourceId = null,
            DateTime scheduledStartTime = default,
            DateTime scheduledEndTime = default,
            Uri joinUrl = null,
            string title = null,
            string type = "Scheduled")
        {
            Id = id;
            MsGraphResourceId = msGraphResourceId;
            ScheduledStartTime = scheduledStartTime;
            ScheduledEndTime = scheduledEndTime;
            JoinUrl = joinUrl;
            Title = title;
            Type = type;

            CustomInit();
        }

        /// <summary>
        /// Gets or sets the meeting's Id, encoded as a BASE64 string.
        /// </summary>
        /// <value>
        /// The meeting's Id, encoded as a BASE64 string.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the MsGraphResourceId, used specifically for MS Graph API calls.
        /// </summary>
        /// <value>
        /// The MsGraphResourceId, used specifically for MS Graph API calls.
        /// </value>
        [JsonProperty(PropertyName = "msGraphResourceId")]
        public string MsGraphResourceId { get; set; }

        /// <summary>
        /// Gets or sets the meeting's scheduled start time, in UTC.
        /// </summary>
        /// <value>
        /// The meeting's scheduled start time, in UTC.
        /// </value>
        [JsonProperty(PropertyName = "scheduledStartTime")]
        public DateTime ScheduledStartTime { get; set; }

        /// <summary>
        /// Gets or sets the meeting's scheduled end time, in UTC.
        /// </summary>
        /// <value>
        /// The meeting's scheduled end time, in UTC.
        /// </value>
        [JsonProperty(PropertyName = "scheduledEndTime")]
        public DateTime ScheduledEndTime { get; set; }

        /// <summary>
        /// Gets or sets the URL used to join the meeting.
        /// </summary>
        /// <value>
        /// The URL used to join the meeting.
        /// </value>
        [JsonProperty(PropertyName = "joinUrl")]
        public Uri JoinUrl { get; set; }

        /// <summary>
        /// Gets or sets the title of the meeting.
        /// </summary>
        /// <value>
        /// The title of the meeting.
        /// </value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the meeting's type.
        /// </summary>
        /// <value>
        /// The meeting's type.
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
