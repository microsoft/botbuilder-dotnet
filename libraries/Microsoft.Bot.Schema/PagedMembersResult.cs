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
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the Channel Accounts.
        /// </summary>
        [JsonProperty(PropertyName = "members")]
        public IList<ChannelAccount> Members { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
