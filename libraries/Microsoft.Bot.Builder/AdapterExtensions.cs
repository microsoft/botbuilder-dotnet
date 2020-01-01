using System;
using System.Linq;

namespace Microsoft.Bot.Builder
{
    public static class AdapterExtensions
    {
        /// <summary>
        /// Register storage with the adapter so that is available via TurnContext.Get&lt;IStorage&gt;().
        /// </summary>
        /// <param name="botAdapter">bot adapter to register storage with.</param>
        /// <param name="storage">IStorage implementation to register.</param>
        /// <returns>bot adapter.</returns>
        public static BotAdapter UseStorage(this BotAdapter botAdapter, IStorage storage)
        {
            return botAdapter.Use(new RegisterClassMiddleware<IStorage>(storage));
        }

        /// <summary>
        /// Register UserState and ConversationState objects so they are accessible via TurnContext.Get&lt;UserState&gt;() and add auto save middleware.
        /// </summary>
        /// <param name="botAdapter">bot adapater to add save state to.</param>
        /// <param name="userState">UserState to use (default is new UserState(registered storage)).</param>
        /// <param name="conversationState">ConversationState to use (default is new ConversationState (registered storage)).</param>
        /// <param name="auto">automatically manage state (default is true), if set to false, it is your responsibility to load and save state.</param>
        /// <returns>Botadapter.</returns>
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
