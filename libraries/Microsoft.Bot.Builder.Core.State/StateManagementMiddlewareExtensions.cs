using System;

namespace Microsoft.Bot.Builder.Core.State
{
    public static class StateManagementMiddlewareExtensions
    {
        public static StateManagementMiddleware UseDefaultStorageProvider(this StateManagementMiddleware stateManagementMiddleware, IStateStorageProvider provider) => stateManagementMiddleware.UseStorageProvider(StateManagementMiddleware.DefaultStateStoreName, provider);

        public static StateManagementMiddleware UseState<TStateManager>(this StateManagementMiddleware stateManagementMiddleware, string storeName = null) where TStateManager : class, IStateManager =>
            stateManagementMiddleware.UseState<TStateManager>(storeName == null ? (Action<StateManagerConfigurationBuilder>)null : cb => cb.UseStorageProvider(storeName));

        public static StateManagementMiddleware UseState<TStateManager>(this StateManagementMiddleware stateManagementMiddleware, Action<StateManagerConfigurationBuilder> configure = null) where TStateManager : class, IStateManager =>
            stateManagementMiddleware.UseState(typeof(TStateManager), configure);

        public static StateManagementMiddleware UseUserState(this StateManagementMiddleware stateManagementMiddleware, string storeName)
        {
            if (storeName == null)
            {
                throw new ArgumentNullException(nameof(storeName));
            }

            return stateManagementMiddleware.UseUserState(cb =>
            {
                cb.UseStorageProvider(storeName);
            });
        }

        public static StateManagementMiddleware UseUserState(this StateManagementMiddleware stateManagementMiddleware, Action<StateManagerConfigurationBuilder> configure = null) =>
            stateManagementMiddleware.UseState<IUserStateManager>(cb =>
            {
                configure?.Invoke(cb);

                cb.UseFactory((tc, ss) => new UserStateManager(tc.Activity.From.Id, ss));
            });

        public static StateManagementMiddleware UseConversationState(this StateManagementMiddleware stateManagementMiddleware, string storeName)
        {
            if (storeName == null)
            {
                throw new ArgumentNullException(nameof(storeName));
            }

            return stateManagementMiddleware.UseConversationState(cb =>
            {
                cb.UseStorageProvider(storeName);
            });
        }

        public static StateManagementMiddleware UseConversationState(this StateManagementMiddleware stateManagementMiddleware, Action<StateManagerConfigurationBuilder> configure = null) =>
            stateManagementMiddleware.UseState<IConversationStateManager>(cb =>
            {
                configure?.Invoke(cb);

                cb.UseFactory((tc, ss) => new ConversationStateManager(tc.Activity.ChannelId, tc.Activity.Conversation.Id, ss));
            });
    }


}
