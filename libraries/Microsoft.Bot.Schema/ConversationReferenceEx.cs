// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// ConversationReference extensions - helper for creating Activity
    /// </summary>
    public partial class ConversationReference 
    {
        /// <summary>
        /// Creates an <see cref="EventActivity"/> from conversation reference as it is posted to bot.
        /// </summary>
        public EventActivity GetContinuationActivity() =>
            new EventActivity
            {
                Name = "ContinueConversation",
                Id = Guid.NewGuid().ToString(),
                ChannelId = this.ChannelId,
                ServiceUrl = this.ServiceUrl,
                Conversation = this.Conversation,
                Recipient = this.Bot,
                From = this.User,
                RelatesTo = this,
            };
    }
}
