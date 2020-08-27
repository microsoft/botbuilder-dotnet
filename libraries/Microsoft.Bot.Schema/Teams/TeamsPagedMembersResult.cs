// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Teams
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
        public TeamsPagedMembersResult(string continuationToken = default(string), IList<ChannelAccount> members = default(IList<ChannelAccount>))
        {
            ContinuationToken = continuationToken;
            var teamsChannelAccounts = members.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            Members = teamsChannelAccounts.ToList();
        }

        /// <summary>
        /// Gets or sets the paging token.
        /// </summary>
        /// <value>
        /// The paging token.
        /// </value>
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the list of channel accounts.
        /// </summary>
        /// <value>
        /// The channel accounts.
        /// </value>
        [JsonProperty(PropertyName = "members")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public IList<TeamsChannelAccount> Members { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
