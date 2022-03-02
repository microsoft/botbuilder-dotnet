// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema.Teams
{
    /// <summary>
    /// List of channels under a team.
    /// </summary>
    public class ConversationList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationList"/> class.
        /// </summary>
        public ConversationList()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationList"/> class.
        /// </summary>
        /// <param name="conversations">The IList of conversations.</param>
        public ConversationList(IList<ChannelInfo> conversations = default)
        {
            Conversations = conversations;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the conversations.
        /// </summary>
        /// <value>The conversations.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("conversations")]
        public IList<ChannelInfo> Conversations { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
