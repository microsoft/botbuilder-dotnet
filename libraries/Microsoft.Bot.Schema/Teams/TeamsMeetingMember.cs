// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Data about the joined meeting participants.
    /// </summary>
    public partial class TeamsMeetingMember
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMeetingMember"/> class.
        /// </summary>
        /// <param name="user">The channel user data.</param>
        /// <param name="meeting">The user meeting details.</param>
        public TeamsMeetingMember(TeamsChannelAccount user, UserMeetingDetails meeting) 
        {
            User = user;
            Meeting = meeting;
        }

        /// <summary>
        /// Gets or sets the joined meeting participant.
        /// </summary>
        /// <value>
        /// The joined participant account.
        /// </value>
        [JsonProperty(PropertyName = "user")]
        public TeamsChannelAccount User { get; set; }

        /// <summary>
        /// Gets or sets the joined users meeting details.
        /// </summary>
        /// <value>
        /// The joined users meeting details.
        /// </value>
        [JsonProperty(PropertyName = "Meeting")]
        public UserMeetingDetails Meeting { get; set; }
    }
}
