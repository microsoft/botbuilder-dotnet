// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specific details of a Teams meeting.
    /// </summary>
    public partial class MeetingEventDetails : MeetingDetailsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingEventDetails"/> class.
        /// </summary>
        internal MeetingEventDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingEventDetails"/> class.
        /// </summary>
        /// <param name="id">The meeting's Id, encoded as a BASE64 string.</param>
        /// <param name="joinUrl">The URL used to join the meeting.</param>
        /// <param name="title">The title of the meeting.</param>
        /// <param name="meetingType">The meeting's type.</param>
        internal MeetingEventDetails(
            string id,
            Uri joinUrl = null,
            string title = null,
            string meetingType = "Scheduled")
            : base(id, joinUrl, title)
        {
            MeetingType = meetingType;

            CustomInit();
        }

        /// <summary>
        /// Gets or sets the meeting's type.
        /// </summary>
        /// <value>
        /// The meeting's type.
        /// </value>
        [JsonProperty(PropertyName = "MeetingType")]
        public string MeetingType { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
