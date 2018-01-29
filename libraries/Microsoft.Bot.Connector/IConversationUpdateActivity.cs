// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// The referenced conversation has been updated
    /// </summary>
    public interface IConversationUpdateActivity : IActivity
    {
        /// <summary>
        /// Members added to the conversation
        /// </summary>
        IList<ChannelAccount> MembersAdded { get; set; }

        /// <summary>
        /// Members removed from the conversation
        /// </summary>
        IList<ChannelAccount> MembersRemoved { get; set; }

        /// <summary>
        /// The conversation's updated topic name
        /// </summary>
        string TopicName { get; set; }

        /// <summary>
        /// True if prior history of the channel is disclosed
        /// </summary>
        bool? HistoryDisclosed { get; set; }
    }
}
