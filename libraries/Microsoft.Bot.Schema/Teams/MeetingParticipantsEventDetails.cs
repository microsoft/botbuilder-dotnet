// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Data about the meeting participants.
    /// </summary>
    public partial class MeetingParticipantsEventDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingParticipantsEventDetails"/> class.
        /// </summary>
        public MeetingParticipantsEventDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingParticipantsEventDetails"/> class.
        /// </summary>
        /// <param name="members">The members involved in the meeting event.</param>
        public MeetingParticipantsEventDetails(
            IList<TeamsMeetingMember> members = default)
        { 
            Members = members;
            CustomInit();
        }
     
        /// <summary>
        /// Gets the meeting participants info.
        /// </summary>
        /// <value>
        /// The participant accounts info.
        /// </value>
        [JsonProperty(PropertyName = "Members")]
        public IList<TeamsMeetingMember> Members { get; private set; } = new List<TeamsMeetingMember>();

        partial void CustomInit();
    }
}
