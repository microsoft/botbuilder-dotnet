// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>Parameters for creating a new conversation.</summary>
    public class ConversationParameters
    {
        /// <summary>Initializes a new instance of the <see cref="ConversationParameters"/> class.</summary>
        public ConversationParameters()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="ConversationParameters"/> class.</summary>
        /// <param name="isGroup">IsGroup.</param>
        /// <param name="bot">The bot address for this conversation.</param>
        /// <param name="members">Members to add to the conversation.</param>
        /// <param name="topicName">(Optional) Topic of the conversation (if supported by the channel).</param>
        /// <param name="activity">(Optional) When creating a new conversation, use this activity as the initial message to the conversation.</param>
        /// <param name="channelData">Channel specific payload for creating the conversation.</param>
        /// <param name="tenantId">(Optional) The tenant ID in which the conversation should be created.</param>
        public ConversationParameters(bool? isGroup = default, ChannelAccount bot = default, IList<ChannelAccount> members = default, string topicName = default, Activity activity = default, object channelData = default, string tenantId = default)
        {
            IsGroup = isGroup;
            Bot = bot;
            Members = members;
            TopicName = topicName;
            Activity = activity;
            ChannelData = channelData;
            TenantId = tenantId;
            CustomInit();
        }

        /// <summary>Gets or sets isGroup.</summary>
        /// <value>IsGroup boolean.</value>
        [JsonPropertyName("isGroup")]
        public bool? IsGroup { get; set; }

        /// <summary>Gets or sets the bot address for this conversation.</summary>
        /// <value>The bot address for this conversation.</value>
        [JsonPropertyName("bot")]
        public ChannelAccount Bot { get; set; }

        /// <summary>Gets or sets members to add to the conversation.</summary>
        /// <value>The members added to the conversation.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("members")]
        public IList<ChannelAccount> Members { get; set; }

        /// <summary>Gets or sets (Optional) Topic of the conversation (if supported by the channel).</summary>
        /// <value>The topic of the conversation.</value>
        [JsonPropertyName("topicName")]
        public string TopicName { get; set; }

        /// <summary>Gets or sets (Optional) When creating a new conversation, use this activity as the initial message to the conversation.</summary>
        /// <value>The activity.</value>
        [JsonPropertyName("activity")]
        public Activity Activity { get; set; }

        /// <summary>Gets or sets channel specific payload for creating the conversation.</summary>
        /// <value>The channel data.</value>
        [JsonPropertyName("channelData")]
        public object ChannelData { get; set; }

        /// <summary>Gets or sets (Optional) The tenant ID in which the conversationshould be created.</summary>
        /// <value>The tenant ID.</value>
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        private void CustomInit()
        {
        }
    }
}
