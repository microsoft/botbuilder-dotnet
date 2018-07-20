// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The referenced conversation has been updated
    /// </summary>
    public class ConversationUpdateActivity : Activity
    {
        public ConversationUpdateActivity() : base(ActivityTypes.ConversationUpdate)
        {
        }

        /// <summary>
        /// Gets or sets members added to the conversation
        /// </summary>
        [JsonProperty(PropertyName = "membersAdded")]
        public IList<ChannelAccount> MembersAdded { get; set; }

        /// <summary>
        /// Gets or sets members removed from the conversation
        /// </summary>
        [JsonProperty(PropertyName = "membersRemoved")]
        public IList<ChannelAccount> MembersRemoved { get; set; }

        /// <summary>
        /// Gets or sets the conversation's updated topic name
        /// </summary>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets true if prior history of the channel is disclosed
        /// </summary>
        [JsonProperty(PropertyName = "historyDisclosed")]
        public bool? HistoryDisclosed { get; set; }
    }
}
