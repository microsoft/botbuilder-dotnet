using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.State;

namespace Microsoft.Bot.Builder
{
    public static class StateManagerExtensions
    {
        public static Task<TState> Get<TState>(this IStateManager stateManager) where TState : class, new() =>
            stateManager.Get<TState>(typeof(TState).Name);

        public static async Task<TState> GetOrCreate<TState>(this IStateManager stateManager) where TState : class, new() =>
            await stateManager.Get<TState>() ?? new TState();

        public static async Task<TState> GetOrCreate<TState>(this IStateManager stateManager, Func<TState> stateFactory) where TState : class, new() =>
            await stateManager.Get<TState>() ?? stateFactory();

        public static void Set<TState>(this IStateManager stateManager, TState state) where TState : class =>
            stateManager.Set(typeof(TState).Name, state);

    }
}
