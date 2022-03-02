// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Conversations result.
    /// </summary>
    public class ConversationsResult
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
            Conversations = conversations;
            CustomInit();
        }

        /// <summary>Gets or sets paging token.</summary>
        /// <value>The continuation token that can be used to get paged results.</value>
        [JsonPropertyName("continuationToken")]
        public string ContinuationToken { get; set; }

        /// <summary>Gets or sets list of conversations.</summary>
        /// <value>A list of conversations.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("conversations")]
        public IList<ConversationMembers> Conversations { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        private void CustomInit()
        {
        }
    }
}
