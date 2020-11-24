﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Globalization;

    /// <summary>
    /// An object relating to a particular point in a conversation
    /// </summary>
    public partial class ConversationReference
    {
        /// <summary>
        /// Initializes a new instance of the ConversationReference class.
        /// </summary>
        public ConversationReference()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ConversationReference class.
        /// </summary>
        /// <param name="activityId">(Optional) ID of the activity to refer
        /// to</param>
        /// <param name="user">(Optional) User participating in this
        /// conversation</param>
        /// <param name="bot">Bot participating in this conversation</param>
        /// <param name="conversation">Conversation reference</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="serviceUrl">Service endpoint where operations
        /// concerning the referenced conversation may be performed</param>
        public ConversationReference(string activityId = default(string), ChannelAccount user = default(ChannelAccount), ChannelAccount bot = default(ChannelAccount), ConversationAccount conversation = default(ConversationAccount), string channelId = default(string), string serviceUrl = default(string))
                : this(default(CultureInfo), activityId, user, bot, conversation, channelId, serviceUrl)
        { }
        /// <summary>
        /// Initializes a new instance of the ConversationReference class.
        /// </summary>
        /// <param name="locale">A locale name for the contents of the text
        /// field.
        /// The locale name is a combination of an ISO 639 two- or three-letter
        /// culture code associated with a language
        /// and an ISO 3166 two-letter subculture code associated with a
        /// country or region.
        /// The locale name can also correspond to a valid BCP-47 language
        /// tag.</param>
        /// <param name="activityId">(Optional) ID of the activity to refer
        /// to</param>
        /// <param name="user">(Optional) User participating in this
        /// conversation</param>
        /// <param name="bot">Bot participating in this conversation</param>
        /// <param name="conversation">Conversation reference</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="serviceUrl">Service endpoint where operations
        /// concerning the referenced conversation may be performed</param>
        public ConversationReference(CultureInfo locale, string activityId = default(string), ChannelAccount user = default(ChannelAccount), ChannelAccount bot = default(ChannelAccount), ConversationAccount conversation = default(ConversationAccount), string channelId = default(string), string serviceUrl = default(string))
        {
            ActivityId = activityId;
            User = user;
            Bot = bot;
            Conversation = conversation;
            ChannelId = channelId;
            Locale = locale?.ToString();
            ServiceUrl = serviceUrl;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets (Optional) ID of the activity to refer to
        /// </summary>
        [JsonProperty(PropertyName = "activityId")]
        public string ActivityId { get; set; }

        /// <summary>
        /// Gets or sets (Optional) User participating in this conversation
        /// </summary>
        [JsonProperty(PropertyName = "user")]
        public ChannelAccount User { get; set; }

        /// <summary>
        /// Gets or sets (Optional) Bot participating in this conversation
        /// </summary>
        [JsonProperty(PropertyName = "bot")]
        public ChannelAccount Bot { get; set; }

        /// <summary>
        /// Gets or sets Reference to the conversation
        /// </summary>
        [JsonProperty(PropertyName = "conversation")]
        public ConversationAccount Conversation { get; set; }

        /// <summary>
        /// Gets or sets ID of the channel in which the referenced conversation exists
        /// </summary>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { get; set; }
        
        /// <summary>
        /// Gets or sets (Optional) A BCP-47 locale name for the referenced conversation
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets (Optional) Service endpoint where operations concerning
        /// the referenced conversation may be performed
        /// </summary>
        [JsonProperty(PropertyName = "serviceUrl")]
        public string ServiceUrl { get; set; }    
       
    }
}
