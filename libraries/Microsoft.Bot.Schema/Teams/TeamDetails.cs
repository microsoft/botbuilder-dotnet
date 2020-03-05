// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

namespace Microsoft.Bot.Schema.Teams
{
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
        /// <param name="aadGroupId">Azure Active Directory (AAD) Group Id for.
        /// <param name="channelCount">The count of channels in the team.
        /// <param name="memberCount">The count of members in the team.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:Element parameters should be documented", Justification = "It is documented...")]
        public TeamDetails(string id = default(string), string name = default(string), string aadGroupId = default(string), int channelCount = default(int), int memberCount = default(int))
        {
            Id = id;
            Name = name;
            AadGroupId = aadGroupId;
            ChannelCount = channelCount;
            MemberCount = memberCount;
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
