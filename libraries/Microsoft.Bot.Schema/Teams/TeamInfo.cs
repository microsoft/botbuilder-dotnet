// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a team.
    /// </summary>
    public partial class TeamInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamInfo"/> class.
        /// </summary>
        public TeamInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamInfo"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a team.</param>
        /// <param name="name">Name of team.</param>
        public TeamInfo(string id = default(string), string name = default(string))
        {
            Id = id;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique identifier representing a team.
        /// </summary>
        /// <value>The team ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of team.
        /// </summary>
        /// <value>The team name.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD Teams group ID.
        /// </summary>
        /// <value>The Azure Active Directory Teams group ID.</value>
        [JsonProperty(PropertyName = "aadGroupId")]
        public string AadGroupId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
