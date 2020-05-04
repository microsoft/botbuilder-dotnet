using System;
using System.Linq;

namespace Microsoft.Bot.Builder
{
    public static class AdapterExtensions
    {
        /// <summary>
        /// Registers a storage layer with the adapter. The storage object will be available via the turn context's
        /// <see cref="TurnContext.TurnState"/>.<see cref="TurnContextStateCollection.Get{IStorage}()"/> method.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> on which to register the storage object.</param>
        /// <param name="storage">The <see cref="IStorage"/> object to register.</param>
        /// <returns>The updated adapter.</returns>
        /// <remarks>
        /// This adds <see cref="IMiddleware"/> to the adapter to register the storage layer.
        /// </remarks>
        public static BotAdapter UseStorage(this BotAdapter botAdapter, IStorage storage)
        {
            return botAdapter.Use(new RegisterClassMiddleware<IStorage>(storage ?? throw new ArgumentNullException(nameof(storage))));
        }

        /// <summary>
        /// Registers bot state object into the TurnContext. The botstate will be available via the turn context's
        /// <see cref="TurnContext.TurnState"/>.<see cref="TurnContextStateCollection.Get{typeof(instance)}()"/> method.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> on which to register the storage object.</param>
        /// <param name="botStates">One or <see cref="BotState"/> object to register.</param>
        /// <returns>The updated adapter.</returns>
        /// <remarks>
        /// This adds <see cref="IMiddleware"/> to register the a BotState object into turnstate.
        /// </remarks>
        public static BotAdapter UseBotState(this BotAdapter botAdapter, params BotState[] botStates)
        {
            if (botStates == null)
            {
                throw new ArgumentNullException(nameof(botStates));
            }

            foreach (var botState in botStates)
            {
                botAdapter.Use(new RegisterClassMiddleware<BotState>(botState, botState.GetType().FullName));
            }

            return botAdapter;
        }

        /// <summary>
        /// Registers user and conversation state objects with the adapter. These objects will be available via the turn context's
        /// <see cref="TurnContext.TurnState"/>.<see cref="TurnContextStateCollection.Get{T}()"/> method.
        /// </summary>
        /// <param name="botAdapter">The <see cref="BotAdapter"/> on which to register the storage object.</param>
        /// <param name="userState">The <see cref="UserState"/> object to register.</param>
        /// <param name="conversationState">The <see cref="ConversationState"/> object to register.</param>
        /// <param name="auto">`true` to automatically persist state each turn; otherwise, `false`.
        /// When false, it is your responsibility to persist state each turn.</param>
        /// <returns>The updated adapter.</returns>
        /// <remarks>
        /// This adds <see cref="IMiddleware"/> to register the user and conversation state management objects.
        /// If <paramref name="auto"/> is true, this also adds middleware to automatically persist state before each turn ends.
        /// </remarks>
        [Obsolete("This method is deprecated in 4.9.  You should use the method .UseBotState() instead.")]
        public static BotAdapter UseState(this BotAdapter botAdapter, UserState userState, ConversationState conversationState, bool auto = true)
        {
            if (botAdapter == null)
            {
                throw new ArgumentNullException(nameof(botAdapter));
            }

            if (userState == null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            if (conversationState == null)
            {
                throw new ArgumentNullException(nameof(conversationState));
            }

            botAdapter.Use(new RegisterClassMiddleware<UserState>(userState));
            botAdapter.Use(new RegisterClassMiddleware<ConversationState>(conversationState));

            if (auto)
            {
                return botAdapter.Use(new AutoSaveStateMiddleware(userState, conversationState));
            }

            return botAdapter;
        }
    }
}
