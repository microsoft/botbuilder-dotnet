// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Details related to a team.
    /// </summary>
    public partial class TeamDetails
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
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of team.
        /// </summary>
        /// <value>
        /// Name of team.
        /// </value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets azure Active Directory (AAD) Group Id for the team.
        /// </summary>
        /// <value>
        /// The AAD Group Id.
        /// </value>
        [JsonProperty(PropertyName = "aadGroupId")]
        public string AadGroupId { get; set; }

        /// <summary>
        /// Gets or sets the number of channels in the team.
        /// </summary>
        /// <value>
        /// The number of channels in the team.
        /// </value>
        [JsonProperty(PropertyName = "channelCount")]
        public int ChannelCount { get; set; }

        /// <summary>
        /// Gets or sets the number of members in the team.
        /// </summary>
        /// <value>
        /// The number of members in the team.
        /// </value>
        [JsonProperty(PropertyName = "memberCount")]
        public int MemberCount { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
