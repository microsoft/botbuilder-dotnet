﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Teams participant channel account detailing user Azure Active Directory and meeting participant details.
    /// </summary>
    public class TeamsParticipantChannelAccount : TeamsChannelAccount
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsParticipantChannelAccount"/> class.
        /// </summary>
        /// <param name="id">Channel id for the user or bot on this channel.
        /// (Example: joe@smith.com, or @joesmith or 123456).</param>
        /// <param name="name">Display friendly name.</param>
        /// <param name="givenName">Given name part of the user name.</param>
        /// <param name="surname">Surname part of the user name.</param>
        /// <param name="email">Email Id of the user.</param>
        /// <param name="userPrincipalName">Unique user principal name.</param>
        /// <param name="tenantId">TenantId of the user.</param>
        /// <param name="userRole">UserRole of the user.</param>
        /// <param name="meetingRole">Role of the participant in the current meeting.</param>
        /// <param name="inMeeting">True, if the participant is in the meeting.</param>
        /// <param name="conversation">Conversation Account for the meeting.</param>
        public TeamsParticipantChannelAccount(string id = default, string name = default, string givenName = default, string surname = default, string email = default, string userPrincipalName = default, string tenantId = default, string userRole = default, string meetingRole = default, bool inMeeting = default, ConversationAccount conversation = null)
            : base(id, name, givenName, surname, email, userPrincipalName, tenantId, userRole)
        {
            MeetingRole = meetingRole;
            InMeeting = inMeeting;
            Conversation = conversation;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the participant is in the meeting or not.
        /// </summary>
        /// <value>
        /// The value indicating if the participant is in the meeting.
        /// </value>
        [JsonProperty(PropertyName = "inMeeting")]
        public bool InMeeting { get; set; }

        /// <summary>
        /// Gets or sets the participant's role in the meeting.
        /// </summary>
        /// <value>
        /// The participant's role in the meeting.
        /// </value>
        [JsonProperty(PropertyName = "meetingRole")]
        public string MeetingRole { get; set; }

        /// <summary>
        /// Gets or sets the Conversation Account for the meeting.
        /// </summary>
        /// <value>
        /// The Conversation Account for the meeting.
        /// </value>
        [JsonProperty(PropertyName = "conversation")]
        public ConversationAccount Conversation { get; set; }
    }
}
