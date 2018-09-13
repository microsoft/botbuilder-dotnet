// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a conversation state object using the conversation ID as part of the key.
    /// </summary>
    public class ConversationState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationState"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        public ConversationState(IStorage storage)
            : base(storage, nameof(ConversationState))
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
            return $"{channelId}/conversations/{conversationId}";
        }
    }
}
