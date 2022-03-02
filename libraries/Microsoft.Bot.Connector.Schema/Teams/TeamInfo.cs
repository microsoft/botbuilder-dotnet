// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Describes a team.
    /// </summary>
    public class TeamInfo
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
        public TeamInfo(string id = default, string name = default)
        {
            Id = id;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique identifier representing a team.
        /// </summary>
        /// <value>The team ID.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of team.
        /// </summary>
        /// <value>The team name.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD Teams group ID.
        /// </summary>
        /// <value>The Azure Active Directory Teams group ID.</value>
        [JsonPropertyName("aadGroupId")]
        public string AadGroupId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
