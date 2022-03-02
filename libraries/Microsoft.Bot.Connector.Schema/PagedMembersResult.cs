// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Page of members.
    /// </summary>
    public class PagedMembersResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedMembersResult"/> class.
        /// </summary>
        public PagedMembersResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedMembersResult"/> class.
        /// </summary>
        /// <param name="continuationToken">Paging token.</param>
        /// <param name="members">The Channel Accounts.</param>
        public PagedMembersResult(string continuationToken = default, IList<ChannelAccount> members = default)
        {
            ContinuationToken = continuationToken;
            Members = members;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets paging token.
        /// </summary>
        /// <value>The continuation token that can be used to get paged results.</value>
        [JsonPropertyName("continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the Channel Accounts.
        /// </summary>
        /// <value>The Channel Accounts.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("members")]
        public IList<ChannelAccount> Members { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
