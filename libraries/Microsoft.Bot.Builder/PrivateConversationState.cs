// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a conversation state object using the conversation.Id and from.Id part of an activity.
    /// </summary>
    public class PrivateConversationState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateConversationState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        public PrivateConversationState(IStorage storage)
            : base(storage, nameof(PrivateConversationState))
        {
        }

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var conversationId = turnContext.Activity.Conversation?.Id ?? throw new ArgumentNullException("invalid activity-missing Conversation.Id");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/conversations/{conversationId}/users/{userId}";
        }
    }
}
