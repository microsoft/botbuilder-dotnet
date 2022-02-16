// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// List of channels under a team.
    /// </summary>
    public partial class ConversationList
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
            Conversations = conversations ?? new List<ChannelInfo>();
            CustomInit();
        }

        /// <summary>
        /// Gets the conversations.
        /// </summary>
        /// <value>The conversations.</value>
        [JsonProperty(PropertyName = "conversations")]
        public IList<ChannelInfo> Conversations { get; private set; } = new List<ChannelInfo>();

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
