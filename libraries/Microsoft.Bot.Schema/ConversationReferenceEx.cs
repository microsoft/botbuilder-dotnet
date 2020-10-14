// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// ConversationReference extensions - helper for creating Activity.
    /// </summary>
    public partial class ConversationReference
    {
        /// <summary>
        /// Creates <see cref="Activity"/> from conversation reference as it is posted to bot.
        /// </summary>
        /// <returns>Continuation activity.</returns>
        public Activity GetContinuationActivity()
        {
            return new Activity(ActivityTypes.Event)
            {
                Name = ActivityEventNames.ContinueConversation,
                Id = Guid.NewGuid().ToString(),
                ChannelId = ChannelId,
                Locale = Locale,
                ServiceUrl = ServiceUrl,
                Conversation = Conversation,
                Recipient = Bot,
                From = User,
                RelatesTo = this
            };
        }
    }
}
