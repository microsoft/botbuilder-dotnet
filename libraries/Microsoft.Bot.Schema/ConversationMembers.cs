// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Conversation and its members.
    /// </summary>
    public class ConversationMembers
    {
        /// <summary>Initializes a new instance of the <see cref="ConversationMembers"/> class.</summary>
        /// <param name="id">Conversation ID.</param>
        /// <param name="members">List of members in this conversation.</param>
        public ConversationMembers(string id = default, IList<ChannelAccount> members = default)
        {
            Id = id;
            Members = members ?? new List<ChannelAccount>();
        }

        /// <summary>Gets or sets conversation ID.</summary>
        /// <value>The conversation ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>Gets list of members in this conversation.</summary>
        /// <value>The members in the conversation.</value>
        [JsonProperty(PropertyName = "members")]
        public IList<ChannelAccount> Members { get; private set; } = new List<ChannelAccount>();
    }
}
