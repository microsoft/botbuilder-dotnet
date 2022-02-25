// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The referenced conversation has been updated.
    /// </summary>
    public interface IConversationUpdateActivity : IActivity
    {
        /// <summary>
        /// Gets Members added to the conversation.
        /// </summary>
        /// <value>List of ChannelAccount.</value>
        IList<ChannelAccount> MembersAdded { get; }

        /// <summary>
        /// Gets Members removed from the conversation.
        /// </summary>
        /// <value>List of ChannelAccount.</value>
        IList<ChannelAccount> MembersRemoved { get; }

        /// <summary>
        /// Gets or Sets The conversation's updated topic name.
        /// </summary>
        /// <value>Topic Name.</value>
        string TopicName { get; set; }

        /// <summary>
        /// Gets or Sets True if prior history of the channel is disclosed.
        /// </summary>
        /// <value>true or false.</value>
        bool? HistoryDisclosed { get; set; }
    }
}
