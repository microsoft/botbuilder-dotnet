// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Teams meeting participant information, detailing user Azure Active Directory and meeting participant details.
    /// </summary>
    public partial class TeamsMeetingParticipant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingParticipant"/> class.
        /// </summary>
        public TeamsMeetingParticipant()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingParticipant"/> class.
        /// </summary>
        /// <param name="user">Teams Channel Account information for this meeting participant.</param>
        /// <param name="conversation">Conversation Account for the meeting.</param>
        /// <param name="meeting">Information specific to this participant in the specific meeting.</param>
        public TeamsMeetingParticipant(TeamsChannelAccount user, ConversationAccount conversation = null, MeetingParticipantInfo meeting = null)
        {
            User = user;
            Meeting = meeting;
            Conversation = conversation;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the participant's user information.
        /// </summary>
        /// <value>
        /// The participant's user information.
        /// </value>
        [JsonProperty(PropertyName = "user")]
        public TeamsChannelAccount User { get; set; }

        /// <summary>
        /// Gets or sets the participant's meeting information.
        /// </summary>
        /// <value>
        /// The participant's role in the meeting.
        /// </value>
        [JsonProperty(PropertyName = "meeting")]
        public MeetingParticipantInfo Meeting { get; set; }

        /// <summary>
        /// Gets or sets the Conversation Account for the meeting.
        /// </summary>
        /// <value>
        /// The Conversation Account for the meeting.
        /// </value>
        [JsonProperty(PropertyName = "conversation")]
        public ConversationAccount Conversation { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
