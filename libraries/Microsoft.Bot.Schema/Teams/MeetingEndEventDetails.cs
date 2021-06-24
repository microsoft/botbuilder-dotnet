// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specific details of a Teams meeting end event.
    /// </summary>
    public partial class MeetingEndEventDetails : MeetingEventDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingEndEventDetails"/> class.
        /// </summary>
        public MeetingEndEventDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingEndEventDetails"/> class.
        /// </summary>
        /// <param name="id">The meeting's Id, encoded as a BASE64 string.</param>
        /// <param name="joinUrl">The URL used to join the meeting.</param>
        /// <param name="title">The title of the meeting.</param>
        /// <param name="meetingType">The meeting's type.</param>
        /// <param name="endTime">Timestamp for the meeting end, in UTC.</param>
        public MeetingEndEventDetails(
            string id,
            Uri joinUrl = null,
            string title = null,
            string meetingType = "Scheduled",
            DateTime endTime = default)
            : base(id, joinUrl, title, meetingType)
        {
            EndTime = endTime;

            CustomInit();
        }

        /// <summary>
        /// Gets or sets the meeting's end time, in UTC.
        /// </summary>
        /// <value>
        /// The meeting's end time, in UTC.
        /// </value>
        [JsonProperty(PropertyName = "EndTime")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
