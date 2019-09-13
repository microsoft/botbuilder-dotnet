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

            if (userState == null || conversationState == null)
            {
                var storageMiddleware = botAdapter.MiddlewareSet.Where(mw => mw is RegisterClassMiddleware<IStorage>).Cast<RegisterClassMiddleware<IStorage>>().FirstOrDefault();
                if (storageMiddleware == null)
                {
                    throw new ArgumentNullException("There is no IStorage registered in the middleware");
                }

                if (userState == null)
                {
                    var userStateMiddleware = botAdapter.MiddlewareSet.Where(mw => mw is RegisterClassMiddleware<UserState>).Cast<RegisterClassMiddleware<UserState>>().FirstOrDefault();
                    if (userStateMiddleware != null)
                    {
                        userState = userStateMiddleware.Service;
                    }
                    else
                    {
                        userState = new UserState(storageMiddleware.Service);
                        botAdapter.Use(new RegisterClassMiddleware<UserState>(userState));
                    }
                }

                if (conversationState == null)
                {
                    var conversationStateMiddleware = botAdapter.MiddlewareSet.Where(mw => mw is RegisterClassMiddleware<ConversationState>).Cast<RegisterClassMiddleware<ConversationState>>().FirstOrDefault();
                    if (conversationStateMiddleware != null)
                    {
                        conversationState = conversationStateMiddleware.Service;
                    }
                    else
                    {
                        conversationState = new ConversationState(storageMiddleware.Service);
                        botAdapter.Use(new RegisterClassMiddleware<ConversationState>(conversationState));
                    }
                }
            }

            if (auto)
            {
                return botAdapter.Use(new AutoSaveStateMiddleware(userState, conversationState));
            }

            return botAdapter;
        }
    }
}
