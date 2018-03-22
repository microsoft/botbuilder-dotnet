using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.State
{
    public interface IStateManager
    {
        string Namespace { get; }

        Task<TState> Get<TState>(string key) where TState : class, new();

        void Set<TState>(string key, TState state) where TState : class;

        void Delete(string key);

        Task LoadAll();

        Task Load(IEnumerable<string> keys);

        Task SaveChanges();
    }

    public static class StateManagerExtensions
    {
        public static Task<TState> Get<TState>(this IStateManager stateManager) where TState : class, new() =>
            stateManager.Get<TState>(typeof(TState).Name);

        public static Task<TState> GetOrCreate<TState>(this IStateManager stateManager) where TState : class, new() =>
            stateManager.Get<TState>().ContinueWith(t => t.Result ?? new TState());

        public static Task<TState> GetOrCreate<TState>(this IStateManager stateManager, Func<TState> stateFactory) where TState : class, new() =>
            stateManager.Get<TState>().ContinueWith(t => t.Result ?? stateFactory());

        public static void Set<TState>(this IStateManager stateManager, TState state) where TState : class =>
            stateManager.Set(typeof(TState).Name, state);

    }
}
