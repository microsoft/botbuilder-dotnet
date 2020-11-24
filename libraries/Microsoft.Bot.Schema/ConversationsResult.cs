// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Conversations result
    /// </summary>
    public partial class ConversationsResult
    {
        /// <summary>
        /// Initializes a new instance of the ConversationsResult class.
        /// </summary>
        public ConversationsResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ConversationsResult class.
        /// </summary>
        /// <param name="continuationToken">Paging token</param>
        /// <param name="conversations">List of conversations</param>
        public ConversationsResult(string continuationToken = default(string), IList<ConversationMembers> conversations = default(IList<ConversationMembers>))
        {
            ContinuationToken = continuationToken;
            Conversations = conversations;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets paging token
        /// </summary>
        [JsonProperty(PropertyName = "continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets list of conversations
        /// </summary>
        [JsonProperty(PropertyName = "conversations")]
        public IList<ConversationMembers> Conversations { get; set; }

    }
}
