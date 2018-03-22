using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Core.State
{
    public static class StateManagementTurnContextExtensions
    {
        /// <summary>
        /// Retrieves the <see cref="IConversationStateManager"/> for the <paramref name="turnContext">context</paramref>.
        /// </summary>
        /// <param name="turnContext">The <see cref="ITurnContext"/> whose <see cref="ITurnContext.Activity">activity</see> will be used as the source for channel and conversation identifiers.</param>
        /// <returns></returns>
        public static IConversationStateManager ConversationState(this ITurnContext turnContext) =>
            EnsureStateManagerServiceResolver(turnContext).ResolveStateManager<IConversationStateManager>();

        public static IUserStateManager UserState(this ITurnContext turnContext) =>
            EnsureStateManagerServiceResolver(turnContext).ResolveStateManager<IUserStateManager>();

        public static TStateManager State<TStateManager>(this ITurnContext turnContext) where TStateManager : class, IStateManager =>
            EnsureStateManagerServiceResolver(turnContext).ResolveStateManager<TStateManager>();

        public static IStateManager State(this ITurnContext turnContext, string stateNamespace) =>
            EnsureStateManagerServiceResolver(turnContext).ResolveStateManager(stateNamespace);

        private static IStateManagerServiceResolver EnsureStateManagerServiceResolver(ITurnContext turnContext)
        {
            var stateManagerResolver = turnContext.Services.Get<IStateManagerServiceResolver>();

            if(stateManagerResolver == default(IStateManagerServiceResolver))
            {
                throw new InvalidOperationException($"No {nameof(IStateManagerServiceResolver)} was found. Please check to make sure the {nameof(StateManagementMiddleware)} has been configured.");
            }

            return stateManagerResolver;
        }
    }
}
