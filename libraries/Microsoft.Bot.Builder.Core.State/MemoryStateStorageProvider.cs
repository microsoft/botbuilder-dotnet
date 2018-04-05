using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.State
{
    public class MemoryStateStorageProvider : IStateStorageProvider
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, IStateStorageEntry>> _store = new ConcurrentDictionary<string, ConcurrentDictionary<string, IStateStorageEntry>>();

        public MemoryStateStorageProvider()
        {
        }

        public IStateStorageEntry CreateNewEntry(string stateNamespace, string key) => new StateStorageEntry(stateNamespace, key);

        public Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace)
        {
            if (string.IsNullOrEmpty(stateNamespace))
            {
                throw new ArgumentException("Expected a non-null/empty value.", nameof(stateNamespace));
            }

            if (_store.TryGetValue(stateNamespace, out var entriesForPartion))
            {
                return Task.FromResult(entriesForPartion.Select(kvp => kvp.Value));
            }

            return Task.FromResult(Enumerable.Empty<IStateStorageEntry>());
        }

        public Task<IStateStorageEntry> Load(string stateNamespace, string key)
        {
            if (stateNamespace == null)
            {
                throw new ArgumentNullException(nameof(stateNamespace));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_store.TryGetValue(stateNamespace, out var entriesForPartion))
            {
                if (entriesForPartion.TryGetValue(key, out var entry))
                {
                    return Task.FromResult(entry);
                }
            }

            return Task.FromResult(default(IStateStorageEntry));
        }

        public Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace, IEnumerable<string> keys)
        {
            if (stateNamespace == null)
            {
                throw new ArgumentNullException(nameof(stateNamespace));
            }

            if (_store.TryGetValue(stateNamespace, out var entriesForPartion))
            {
                var results = new List<IStateStorageEntry>(entriesForPartion.Count);

                foreach (var key in keys)
                {
                    if (entriesForPartion.TryGetValue(key, out var entry))
                    {
                        results.Add(entry);
                    }
                }

                return Task.FromResult<IEnumerable<IStateStorageEntry>>(results);
            }

            return Task.FromResult(Enumerable.Empty<IStateStorageEntry>());
        }

        public Task Save(string stateNamespace, IEnumerable<KeyValuePair<string, object>> values)
        {
            if (stateNamespace == null)
            {
                throw new ArgumentNullException(nameof(stateNamespace));
            }

            var entriesForPartition = _store.GetOrAdd(stateNamespace, pk => new ConcurrentDictionary<string, IStateStorageEntry>());

            foreach (var entry in values)
            {
                var newStateStorageEntry = new StateStorageEntry(stateNamespace, entry.Key, entry.Value);

                entriesForPartition.AddOrUpdate(
                    entry.Key,
                    newStateStorageEntry,
                    (key, existingStateStorageEntry) =>
                    {
                        return newStateStorageEntry;
                    });
            }

            return Task.CompletedTask;
        }

        public Task Save(IEnumerable<IStateStorageEntry> entries)
        {
            foreach (var entityGroup in entries.GroupBy(e => e.Namespace))
            {
                var entriesForPartition = _store.GetOrAdd(entityGroup.Key, pk => new ConcurrentDictionary<string, IStateStorageEntry>());

                foreach (var entry in entries)
                {
                    if (!(entry is StateStorageEntry stateStorageEntry))
                    {
                        throw new InvalidOperationException($"Only instances of {nameof(StateStorageEntry)} are supported by {nameof(MemoryStateStorageProvider)}.");
                    }

                    entriesForPartition[stateStorageEntry.Key] = new StateStorageEntry(stateStorageEntry.Namespace, stateStorageEntry.Key, stateStorageEntry.RawValue);
                }
            }

            return Task.CompletedTask;
        }

        public Task Delete(string stateNamespace)
        {
            _store.TryRemove(stateNamespace, out _);

            return Task.CompletedTask;
        }

        public Task Delete(string stateNamespace, string key)
        {
            if (_store.TryGetValue(stateNamespace, out var entriesForPartition))
            {
                entriesForPartition.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }

        public Task Delete(string stateNamespace, IEnumerable<string> keys)
        {
            if (_store.TryGetValue(stateNamespace, out var entriesForPartition))
            {
                foreach (string key in keys)
                {
                    entriesForPartition.TryRemove(key, out _);
                }
            }

            return Task.CompletedTask;
        }
    }
}