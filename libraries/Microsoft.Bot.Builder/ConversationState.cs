// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a conversation state object using the conversation ID as part of the key.
    /// </summary>
    /// <typeparam name="TState">The type of the conversation state object.</typeparam>
    public class ConversationState<TState> : BotState<TState>
        where TState : class, new()
    {
        /// <summary>
        /// The key to use to read and write this conversation state object to storage.
        /// </summary>
        private static string _propertyName = $"ConversationState:{typeof(ConversationState<TState>).Namespace}.{typeof(ConversationState<TState>).Name}";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationState{TState}"/> class.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="settings">The state persistance options to use.</param>
        public ConversationState(IStorage storage, StateSettings settings = null)
            : base(
                storage,
                _propertyName,
                (context) => $"conversation/{context.Activity.ChannelId}/{context.Activity.Conversation.Id}",
                settings)
        {
        }

        /// <summary>
        /// Gets the conversation state object from turn context.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The coversation state object.</returns>
        public static TState Get(ITurnContext context)
        {
            return context.Services.Get<TState>(_propertyName);
        }
    }
}
