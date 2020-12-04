// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
        public PagedMembersResult(string continuationToken = default(string), IList<ChannelAccount> members = default(IList<ChannelAccount>))
        {
            ContinuationToken = continuationToken;
            Members = members;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets paging token.
        /// </summary>
        /// <value>The continuation token that can be used to get paged results.</value>
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the Channel Accounts.
        /// </summary>
        /// <value>The Channel Accounts.</value>
        [JsonProperty(PropertyName = "members")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<ChannelAccount> Members { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
