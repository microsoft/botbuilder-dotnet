// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Defines a state management object for private conversation state.
    /// </summary>
    /// <remarks>
    /// Private conversation state is scoped to both the specific conversation and to that specific user.
    /// </remarks>
    public class PrivateConversationState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateConversationState"/> class.
        /// </summary>
        /// <param name="storage">The storage layer to use.</param>
        public PrivateConversationState(IStorage storage)
            : base(storage, nameof(PrivateConversationState))
        {
        }

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        /// <remarks>
        /// Private conversation state includes the channel ID, conversation ID, and user ID as part
        /// of its storage key.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <see cref="ITurnContext.Activity"/> for the
        /// current turn is missing <see cref="Schema.Activity.ChannelId"/>,
        /// <see cref="Schema.Activity.Conversation"/>, or
        /// <see cref="Schema.Activity.From"/> information; or
        /// the conversation's or sender's <see cref="Schema.ConversationAccount.Id"/> is missing.
        /// </exception>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new InvalidOperationException("invalid activity-missing channelId");
            var conversationId = turnContext.Activity.Conversation?.Id ?? throw new InvalidOperationException("invalid activity-missing Conversation.Id");
            var userId = turnContext.Activity.From?.Id ?? throw new InvalidOperationException("invalid activity-missing From.Id");
            return $"{channelId}/conversations/{conversationId}/users/{userId}";
        }
    }
}
