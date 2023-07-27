// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specific details of a Teams meeting participants added event.
    /// </summary>
    public partial class MeetingParticipantsAddedEventDetails : MeetingEventDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingParticipantsAddedEventDetails"/> class.
        /// </summary>
        public MeetingParticipantsAddedEventDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingParticipantsAddedEventDetails"/> class.
        /// </summary>
        /// <param name="id">The meeting's Id, encoded as a BASE64 string.</param>
        /// <param name="joinUrl">The URL used to join the meeting.</param>
        /// <param name="title">The title of the meeting.</param>
        /// <param name="meetingType">The meeting's type.</param>
        /// <param name="participantsAdded">The added participant accounts.</param>
        public MeetingParticipantsAddedEventDetails(
            string id,
            Uri joinUrl = null,
            string title = null,
            string meetingType = "Scheduled",
            IList<TeamsChannelAccount> participantsAdded = default)
            : base(id, joinUrl, title, meetingType)
        {
            ParticipantsAdded = participantsAdded;

            CustomInit();
        }

        /// <summary>
        /// Gets the added meeting participants.
        /// </summary>
        /// <value>
        /// The added participant accounts.
        /// </value>
        [JsonProperty(PropertyName = "ParticipantsAdded")]
        public IList<TeamsChannelAccount> ParticipantsAdded { get; private set; } = new List<TeamsChannelAccount>();

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
