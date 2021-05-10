// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Conversation and its members.
    /// </summary>
    public partial class ConversationMembers
    {
        /// <summary>Initializes a new instance of the <see cref="ConversationMembers"/> class.</summary>
        public ConversationMembers()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="ConversationMembers"/> class.</summary>
        /// <param name="id">Conversation ID.</param>
        /// <param name="members">List of members in this conversation.</param>
        public ConversationMembers(string id = default(string), IList<ChannelAccount> members = default(IList<ChannelAccount>))
        {
            Id = id;
            Members = members;
            CustomInit();
        }

        /// <summary>Gets or sets conversation ID.</summary>
        /// <value>The conversation ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>Gets or sets list of members in this conversation.</summary>
        /// <value>The members in the conversation.</value>
        [JsonProperty(PropertyName = "members")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<ChannelAccount> Members { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
