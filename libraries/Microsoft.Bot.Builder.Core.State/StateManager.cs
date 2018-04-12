using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.State
{
    public class StateManager : IStateManager
    {
        private readonly Dictionary<string, StateEntry> _state = new Dictionary<string, StateEntry>();

        public StateManager(string stateNamespace, IStateStorageProvider storageProvider)
        {
            Namespace = stateNamespace ?? throw new ArgumentNullException(nameof(stateNamespace));
            StorageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        }

        public string Namespace { get; }

        public IStateStorageProvider StorageProvider { get; }

        public async Task<TState> Get<TState>(string key) where TState : class, new()
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_state.TryGetValue(key, out var stateEntry))
            {
                var stateStorageEntry = await StorageProvider.Load(Namespace, key);

                if (stateStorageEntry == default(IStateStorageEntry))
                {
                    return default(TState);
                }

                _state.Add(key, new StateEntry(stateStorageEntry));

                return stateStorageEntry.GetValue<TState>();
            }

            return stateEntry.IsPendingDeletion ? default(TState) : stateEntry.StateStorageEntry.GetValue<TState>();
        }

        public void Set<TState>(string key, TState state) where TState : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_state.TryGetValue(key, out var stateEntry))
            {
                var stateStorageEntry = StorageProvider.CreateNewEntry(Namespace, key);

                stateEntry = new StateEntry(stateStorageEntry, isDirty: true);

                _state.Add(key, stateEntry);
            }
            else
            {
                stateEntry.IsDirty = true;
                stateEntry.IsPendingDeletion = false;
            }

            stateEntry.StateStorageEntry.SetValue(state);
        }

        public async Task LoadAll()
        {
            var allStateStorageEntries = await StorageProvider.Load(Namespace);

            foreach (var stateStorageEntry in allStateStorageEntries)
            {
                if (!_state.ContainsKey(stateStorageEntry.Key))
                {
                    _state.Add(stateStorageEntry.Key, new StateEntry(stateStorageEntry));
                }
            }
        }

        public async Task Load(IEnumerable<string> keys)
        {
            var stateStorageEntries = await StorageProvider.Load(Namespace, keys);

            foreach (var stateStorageEntry in stateStorageEntries)
            {
                if (!_state.ContainsKey(stateStorageEntry.Key))
                {
                    _state.Add(stateStorageEntry.Key, new StateEntry(stateStorageEntry));
                }
            }
        }

        public void Delete(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_state.TryGetValue(key, out var stateEntry))
            {
                stateEntry.IsPendingDeletion = true;
            }
        }

        public async Task SaveChanges()
        {
            var dirtyStateEntries = _state.Values.Where(se => se.IsDirty);

            await StorageProvider.Save(dirtyStateEntries.Select(se => se.StateStorageEntry));

            foreach (var dirtyStateEntry in dirtyStateEntries)
            {
                dirtyStateEntry.IsDirty = false;
            }

            var stateEntryKeysPendingDeletion = _state.Values.Where(se => se.IsPendingDeletion).Select(se => se.StateStorageEntry.Key).ToList();

            await StorageProvider.Delete(Namespace, stateEntryKeysPendingDeletion);

            foreach (var key in stateEntryKeysPendingDeletion)
            {
                _state.Remove(key);
            }
        }

        private sealed class StateEntry
        {
            public StateEntry(IStateStorageEntry stateStorageEntry, bool isDirty = false)
            {
                StateStorageEntry = stateStorageEntry;
                IsDirty = isDirty;
            }

            public IStateStorageEntry StateStorageEntry;
            public bool IsDirty;
            public bool IsPendingDeletion;
        }
    }
}
