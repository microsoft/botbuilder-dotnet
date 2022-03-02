// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Details related to a team.
    /// </summary>
    public class TeamDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamDetails"/> class.
        /// </summary>
        public TeamDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamDetails"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a team.</param>
        /// <param name="name">Name of team.</param>
        /// <param name="aadGroupId">Azure Active Directory (AAD) Group Id.</param>
        [Obsolete("Use the parameter initialization method instead.")]
        public TeamDetails(string id = default, string name = default, string aadGroupId = default)
        {
            Id = id;
            Name = name;
            AadGroupId = aadGroupId;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique identifier representing a team.
        /// </summary>
        /// /// <value>
        /// The team Id.
        /// </value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of team.
        /// </summary>
        /// <value>
        /// Name of team.
        /// </value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets azure Active Directory (AAD) Group Id for the team.
        /// </summary>
        /// <value>
        /// The AAD Group Id.
        /// </value>
        [JsonPropertyName("aadGroupId")]
        public string AadGroupId { get; set; }

        /// <summary>
        /// Gets or sets the number of channels in the team.
        /// </summary>
        /// <value>
        /// The number of channels in the team.
        /// </value>
        [JsonPropertyName("channelCount")]
        public int ChannelCount { get; set; }

        /// <summary>
        /// Gets or sets the number of members in the team.
        /// </summary>
        /// <value>
        /// The number of members in the team.
        /// </value>
        [JsonPropertyName("memberCount")]
        public int MemberCount { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new NotImplementedException();
        }
    }
}
