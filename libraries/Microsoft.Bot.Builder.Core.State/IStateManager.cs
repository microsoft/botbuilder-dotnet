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
}
