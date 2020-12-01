// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>Parameters for creating a new conversation.</summary>
    public partial class ConversationParameters
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

        /// <summary>Gets or sets isGroup.</summary>
        /// <value>IsGroup boolean.</value>
        [JsonProperty(PropertyName = "isGroup")]
        public bool? IsGroup { get; set; }

        /// <summary>Gets or sets the bot address for this conversation.</summary>
        /// <value>The bot address for this conversation.</value>
        [JsonProperty(PropertyName = "bot")]
        public ChannelAccount Bot { get; set; }

        /// <summary>Gets or sets members to add to the conversation.</summary>
        /// <value>The members added to the conversation.</value>
        [JsonProperty(PropertyName = "members")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<ChannelAccount> Members { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>Gets or sets (Optional) Topic of the conversation (if supported by the channel).</summary>
        /// <value>The topic of the conversation.</value>
        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        /// <summary>Gets or sets (Optional) When creating a new conversation, use this activity as the initial message to the conversation.</summary>
        /// <value>The activity.</value>
        [JsonProperty(PropertyName = "activity")]
        public Activity Activity { get; set; }

        /// <summary>Gets or sets channel specific payload for creating the conversation.</summary>
        /// <value>The channel data.</value>
        [JsonProperty(PropertyName = "channelData")]
        public object ChannelData { get; set; }

        /// <summary>Gets or sets (Optional) The tenant ID in which the conversationshould be created.</summary>
        /// <value>The tenant ID.</value>
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
