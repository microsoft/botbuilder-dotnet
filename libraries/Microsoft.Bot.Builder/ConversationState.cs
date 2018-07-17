// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a conversation state object using the conversation ID as part of the key.
    /// </summary>
    /// <typeparam name="TState">The type of the conversation state object.</typeparam>
    public class ConversationState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationState{TState}"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        public ConversationState(IStorage storage)
            : base(storage, nameof(ConversationState),
                (context) => $"conversation/{context.Activity.ChannelId}/{context.Activity.Conversation.Id}")
        {
        }
    }
}
