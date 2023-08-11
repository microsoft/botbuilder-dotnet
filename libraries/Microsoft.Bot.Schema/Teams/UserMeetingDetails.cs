// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Specific details of a user in a Teams meeting.
    /// </summary>
    public partial class UserMeetingDetails
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is in the meeting.
        /// </summary>
        /// <value>
        /// The user in meeting indicator.
        /// </value>
        [JsonProperty(PropertyName = "InMeeting")]
        public bool InMeeting { get;  set; }

        /// <summary>
        /// Gets or sets the value of the user's role.
        /// </summary>
        /// <value>
        /// The user's role.
        /// </value>
        [JsonProperty(PropertyName = "Role")]
        public string Role { get; set; }
    }
}
