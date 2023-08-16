// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a member.
    /// </summary>
    public partial class TeamMember
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMember"/> class.
        /// </summary>
        public TeamMember()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMember"/> class.
        /// </summary>
        /// <param name="id">Unique identifier representing a member (user or channel).</param>
        public TeamMember(string id = default)
        {
            Id = id;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique identifier representing a member (user or channel).
        /// </summary>
        /// <value>The member ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
