// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specific details of a Teams meeting start event.
    /// </summary>
    public partial class MeetingStartEventDetails : MeetingEventDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingStartEventDetails"/> class.
        /// </summary>
        public MeetingStartEventDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingStartEventDetails"/> class.
        /// </summary>
        /// <param name="id">The meeting's Id, encoded as a BASE64 string.</param>
        /// <param name="joinUrl">The URL used to join the meeting.</param>
        /// <param name="title">The title of the meeting.</param>
        /// <param name="meetingType">The meeting's type.</param>
        /// <param name="startTime">Timestamp for the meeting start, in UTC.</param>
        public MeetingStartEventDetails(
            string id,
            Uri joinUrl = null,
            string title = null,
            string meetingType = "Scheduled",
            DateTime startTime = default)
            : base(id, joinUrl, title, meetingType)
        {
            StartTime = startTime;

            CustomInit();
        }

        /// <summary>
        /// Gets or sets the meeting's start time, in UTC.
        /// </summary>
        /// <value>
        /// The meeting's start time, in UTC.
        /// </value>
        [JsonProperty(PropertyName = "StartTime")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
