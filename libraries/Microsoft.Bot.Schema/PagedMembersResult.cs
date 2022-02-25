// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Page of members.
    /// </summary>
    public partial class PagedMembersResult
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
            Members = members ?? new List<ChannelAccount>();
            CustomInit();
        }

        /// <summary>
        /// Gets or sets paging token.
        /// </summary>
        /// <value>The continuation token that can be used to get paged results.</value>
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets the Channel Accounts.
        /// </summary>
        /// <value>The Channel Accounts.</value>
        [JsonProperty(PropertyName = "members")]
        public IList<ChannelAccount> Members { get; private set; } = new List<ChannelAccount>();

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
