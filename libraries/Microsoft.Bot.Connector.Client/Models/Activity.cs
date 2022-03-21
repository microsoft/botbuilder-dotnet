// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Connector.Client.Models
{
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework protocol.
    /// </summary>
    public partial class Activity
    {
        /// <summary>
        /// Creates a <see cref="ConversationReference"/> based on this activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains this activity.</returns>
        public ConversationReference GetConversationReference()
        {
            var reference = new ConversationReference
            {
                ActivityId = !string.Equals(Type.ToString(), ActivityTypes.ConversationUpdate.ToString(), StringComparison.OrdinalIgnoreCase) || (!string.Equals(ChannelId, "directline", StringComparison.OrdinalIgnoreCase) && !string.Equals(ChannelId, "webchat", StringComparison.OrdinalIgnoreCase)) ? Id : null,
                User = From,
                Bot = Recipient,
                Conversation = Conversation,
                ChannelId = ChannelId,
                Locale = Locale,
                ServiceUrl = ServiceUrl,
            };

            return reference;
        }
    }
}
