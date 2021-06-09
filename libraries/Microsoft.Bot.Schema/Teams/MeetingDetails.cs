// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Teams meeting details.
    /// </summary>
    public partial class MeetingDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingDetails"/> class.
        /// </summary>
        public MeetingDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingDetails"/> class.
        /// </summary>
        /// <param name="details">The meeting's detailed information.</param>
        /// <param name="conversation">Conversation Account for the meeting.</param>
        /// <param name="organizer">Information specific to this organizer of the specific meeting.</param>
        public MeetingDetails(MeetingDetailInfo details, ConversationAccount conversation = null, TeamsChannelAccount organizer = null)
        {
            Details = details;
            Conversation = conversation;
            Organizer = organizer;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the meeting's detailed information.
        /// </summary>
        /// <value>
        /// The meetings detailed information.
        /// </value>
        [JsonProperty(PropertyName = "details")]
        public MeetingDetailInfo Details { get; set; }

        /// <summary>
        /// Gets or sets the Conversation Account for the meeting.
        /// </summary>
        /// <value>
        /// The Conversation Account for the meeting.
        /// </value>
        [JsonProperty(PropertyName = "conversation")]
        public ConversationAccount Conversation { get; set; }

        /// <summary>
        /// Gets or sets the meeting organizer's user information.
        /// </summary>
        /// <value>
        /// The organizer's user information.
        /// </value>
        [JsonProperty(PropertyName = "organizer")]
        public TeamsChannelAccount Organizer { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
