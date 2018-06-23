// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Handles persistence of a user state object using the user ID as part of the key.
    /// </summary>
    /// <typeparam name="TState">The type of the user state object.</typeparam>
    public class UserState<TState> : BotState<TState>
        where TState : class, new()
    {
        /// <summary>
        /// The key to use to read and write this conversation state object to storage.
        /// </summary>
        public static readonly string PropertyName = $"UserState:{typeof(UserState<TState>).Namespace}.{typeof(UserState<TState>).Name}";

        /// <summary>
        /// Creates a new <see cref="UserState{TState}"/> object.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="settings">The state persistance options to use.</param>
        public UserState(IStorage storage, StateSettings settings = null) :
            base(storage,
                PropertyName,
                (context) => $"user/{context.Activity.ChannelId}/{context.Activity.From.Id}",
                settings)
        {
        }

        /// <summary>
        /// Gets the user state object from turn context.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <returns>The user state object.</returns>
        public static TState Get(ITurnContext context)
        {
            return context.Services.Get<TState>(PropertyName);
        }
    }
}
