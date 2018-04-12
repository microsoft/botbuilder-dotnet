using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.State
{
    public interface IStateStorageProvider
    {
        IStateStorageEntry CreateNewEntry(string stateNamespace, string key);

        /// <summary>
        /// Loads all state entries under the specified <paramref name="stateNamespace">partition</paramref>.
        /// </summary>
        /// <param name="stateNamespace"></param>
        /// <returns></returns>
        Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace);

        /// <summary>
        /// Loads a single piece of state, identified by its <paramref name="key"/> from the given <paramref name="stateNamespace">partition</paramref>.
        /// </summary>
        /// <param name="stateNamespace"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<IStateStorageEntry> Load(string stateNamespace, string key);

        Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace, IEnumerable<string> keys);

        Task Save(IEnumerable<IStateStorageEntry> stateStorageEntries);

        Task Delete(string stateNamespace);

        Task Delete(string stateNamespace, IEnumerable<string> keys);
    }

    public static class StateStorageProviderExtensions
    {
        public static Task Save(this IStateStorageProvider stateStore, params IStateStorageEntry[] stateStorageEntries) => stateStore.Save((IEnumerable<IStateStorageEntry>)stateStorageEntries);

        public static Task Delete(this IStateStorageProvider stateStore, string stateNamespace, params string[] keys) => stateStore.Delete(stateNamespace, (IEnumerable<string>)keys);
    }
}