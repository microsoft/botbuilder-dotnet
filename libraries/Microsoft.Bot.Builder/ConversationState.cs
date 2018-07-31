// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

        protected override string GetStorageKey(ITurnContext context) => $"conversation/{context.Activity.ChannelId}/{context.Activity.Conversation.Id}";

        ///// <summary>
        ///// Gets the conversation state property from turn context.
        ///// </summary>
        ///// <param name="context">The context object for this turn.</param>
        ///// <returns>The coversation state object.</returns>
        //public static TState Get(ITurnContext context) { return context.Services.Get<TState>(PropertyName); }

    }
}
