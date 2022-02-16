// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Conversations result.
    /// </summary>
    public partial class ConversationsResult
    {
        /// <summary>Initializes a new instance of the <see cref="ConversationsResult"/> class.</summary>
        public ConversationsResult()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="ConversationsResult"/> class.</summary>
        /// <param name="continuationToken">Paging token.</param>
        /// <param name="conversations">List of conversations.</param>
        public ConversationsResult(string continuationToken = default, IList<ConversationMembers> conversations = default)
        {
            ContinuationToken = continuationToken;
            Conversations = conversations ?? new List<ConversationMembers>();
            CustomInit();
        }

        /// <summary>Gets or sets paging token.</summary>
        /// <value>The continuation token that can be used to get paged results.</value>
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>Gets list of conversations.</summary>
        /// <value>A list of conversations.</value>
        [JsonProperty(PropertyName = "conversations")]
        public IList<ConversationMembers> Conversations { get; private set; } = new List<ConversationMembers>();

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
