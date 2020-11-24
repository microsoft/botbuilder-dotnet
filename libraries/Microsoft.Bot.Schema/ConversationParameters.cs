// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parameters for creating a new conversation
    /// </summary>
    public partial class ConversationParameters
    {
        /// <summary>
        /// Initializes a new instance of the ConversationParameters class.
        /// </summary>
        public ConversationParameters()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ConversationParameters class.
        /// </summary>
        /// <param name="isGroup">IsGroup</param>
        /// <param name="bot">The bot address for this conversation</param>
        /// <param name="members">Members to add to the conversation</param>
        /// <param name="topicName">(Optional) Topic of the conversation (if
        /// supported by the channel)</param>
        /// <param name="activity">(Optional) When creating a new conversation,
        /// use this activity as the initial message to the
        /// conversation</param>
        /// <param name="channelData">Channel specific payload for creating the
        /// conversation</param>
        /// <param name="tenantId">(Optional) The tenant ID in which the
        /// conversation should be created</param>
        public ConversationParameters(bool? isGroup = default(bool?), ChannelAccount bot = default(ChannelAccount), IList<ChannelAccount> members = default(IList<ChannelAccount>), string topicName = default(string), Activity activity = default(Activity), object channelData = default(object), string tenantId = default(string))
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

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets isGroup
        /// </summary>
        [JsonProperty(PropertyName = "isGroup")]
        public bool? IsGroup { get; set; }

        /// <summary>
        /// Gets or sets the bot address for this conversation
        /// </summary>
        [JsonProperty(PropertyName = "bot")]
        public ChannelAccount Bot { get; set; }

        /// <summary>
        /// Gets or sets members to add to the conversation
        /// </summary>
        [JsonProperty(PropertyName = "members")]
        public IList<ChannelAccount> Members { get; set; }

        /// <summary>
        /// Gets or sets (Optional) Topic of the conversation (if supported by
        /// the channel)
        /// </summary>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets (Optional) When creating a new conversation, use this
        /// activity as the initial message to the conversation
        /// </summary>
        [JsonProperty(PropertyName = "activity")]
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets channel specific payload for creating the conversation
        /// </summary>
        [JsonProperty(PropertyName = "channelData")]
        public object ChannelData { get; set; }

        /// <summary>
        /// Gets or sets (Optional) The tenant ID in which the conversation
        /// should be created
        /// </summary>
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

    }
}
