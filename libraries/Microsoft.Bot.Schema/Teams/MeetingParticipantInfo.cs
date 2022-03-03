﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Teams meeting participant details.
    /// </summary>
    public class MeetingParticipantInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingParticipantInfo"/> class.
        /// </summary>
        /// <param name="role">Role of the participant in the current meeting.</param>
        /// <param name="inMeeting">True, if the participant is in the meeting.</param>
        public MeetingParticipantInfo(string role = default, bool? inMeeting = null)
        { 
            Role = role;
            InMeeting = inMeeting;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the participant is in the meeting or not.
        /// </summary>
        /// <value>
        /// The value indicating if the participant is in the meeting.
        /// </value>
        [JsonProperty(PropertyName = "inMeeting")]
        public bool? InMeeting { get; set; }

        /// <summary>
        /// Gets or sets the participant's role in the meeting.
        /// </summary>
        /// <value>
        /// The participant's role in the meeting.
        /// </value>
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }
    }
}
