// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines a state management object for conversation state.
    /// </summary>
    /// <remarks>
    /// Conversation state is available in any turn in a specific conversation, regardless of user,
    /// such as in a group conversation.
    /// </remarks>
    public class ConversationState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationState"/> class.
        /// </summary>
        /// <param name="storage">The storage layer to use.</param>
        public ConversationState(IStorage storage)
            : base(storage, nameof(ConversationState))
        {
        }

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        /// <remarks>
        /// Conversation state includes the channel ID and conversation ID as part of its storage key.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <see cref="ITurnContext.Activity"/> for the
        /// current turn is missing <see cref="Schema.Activity.ChannelId"/> or
        /// <see cref="Schema.Activity.Conversation"/> information, or the conversation's
        /// <see cref="Schema.ConversationAccount.Id"/> is missing.</exception>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new InvalidOperationException("invalid activity-missing channelId");
            var conversationId = turnContext.Activity.Conversation?.Id ?? throw new InvalidOperationException("invalid activity-missing Conversation.Id");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            return $"{channelId}/conversations/{conversationId}";
        }
    }
}
