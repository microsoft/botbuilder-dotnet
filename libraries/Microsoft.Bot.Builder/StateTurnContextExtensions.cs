// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Provides helper methods for getting state objects from the turn context.
    /// </summary>
    public static class StateTurnContextExtensions
    {
        /// <summary>
        /// Gets a conversation state object from the turn context.
        /// </summary>
        /// <typeparam name="TState">The type of the state object to get.</typeparam>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The state object.</returns>
        public static TState GetConversationState<TState>(this ITurnContext context)
            where TState : class, new()
        {
            return ConversationState<TState>.Get(context);
        }

        /// <summary>
        /// Gets a user state object from the turn context.
        /// </summary>
        /// <typeparam name="TState">The type of the state object to get.</typeparam>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The state object.</returns>
        public static TState GetUserState<TState>(this ITurnContext context)
            where TState : class, new()
        {
            return UserState<TState>.Get(context);
        }
    }
}
