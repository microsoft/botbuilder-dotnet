// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// Represents a wrapper for a Teams members query result.
    /// </summary>
    public class TeamsPagedMembersResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsPagedMembersResult"/> class.
        /// </summary>
        public TeamsPagedMembersResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsPagedMembersResult"/> class
        /// using the given continuation token and members list.
        /// </summary>
        /// <param name="continuationToken">A paging token.</param>
        /// <param name="members">A list of channel accounts.</param>
        public TeamsPagedMembersResult(string continuationToken = default, IList<ChannelAccount> members = default)
        {
            ContinuationToken = continuationToken;

            if (members != null)
            {
                Members = members.Select(m => m.ToObject<TeamsChannelAccount>()).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the paging token.
        /// </summary>
        /// <value>
        /// The paging token.
        /// </value>
        [JsonPropertyName("continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the list of channel accounts.
        /// </summary>
        /// <value>
        /// The channel accounts.
        /// </value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("members")]
        public IList<TeamsChannelAccount> Members { get; set; }
    }
}
