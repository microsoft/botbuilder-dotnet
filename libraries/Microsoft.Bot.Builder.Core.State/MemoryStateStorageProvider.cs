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

        public IStateStorageEntry CreateNewEntry(string partitionKey, string key) => new StateStorageEntry(partitionKey, key);

        public Task<IEnumerable<IStateStorageEntry>> Load(string partitionKey)
        {
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            if (_store.TryGetValue(partitionKey, out var entriesForPartion))
            {
                return Task.FromResult(entriesForPartion.Select(kvp => kvp.Value));
            }

            return Task.FromResult(Enumerable.Empty<IStateStorageEntry>());
        }

        public Task<IStateStorageEntry> Load(string partitionKey, string key)
        {
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_store.TryGetValue(partitionKey, out var entriesForPartion))
            {
                if (entriesForPartion.TryGetValue(key, out var entry))
                {
                    return Task.FromResult(entry);
                }
            }

            return Task.FromResult(default(IStateStorageEntry));
        }

        public Task<IEnumerable<IStateStorageEntry>> Load(string partitionKey, IEnumerable<string> keys)
        {
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            if (_store.TryGetValue(partitionKey, out var entriesForPartion))
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

        public Task Save(string partitionKey, IEnumerable<KeyValuePair<string, object>> values)
        {
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            var entriesForPartition = _store.GetOrAdd(partitionKey, pk => new ConcurrentDictionary<string, IStateStorageEntry>());

            foreach (var entry in values)
            {
                var newStateStorageEntry = new StateStorageEntry(partitionKey, entry.Key, entry.Value);

                entriesForPartition.AddOrUpdate(
                    entry.Key,
                    newStateStorageEntry,
                    (key, existingStateStorageEntry) =>
                    {
                        ThrowIfStateStorageEntryETagMismatch(newStateStorageEntry, existingStateStorageEntry);

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

                    if (entriesForPartition.TryGetValue(stateStorageEntry.Key, out var existingEntry))
                    {
                        ThrowIfStateStorageEntryETagMismatch(stateStorageEntry, existingEntry);
                    }

                    entriesForPartition[stateStorageEntry.Key] = new StateStorageEntry(stateStorageEntry.Namespace, stateStorageEntry.Key, Guid.NewGuid().ToString("N"), stateStorageEntry.RawValue);
                }
            }

            return Task.CompletedTask;
        }

        public Task Delete(string partitionKey)
        {
            _store.TryRemove(partitionKey, out _);

            return Task.CompletedTask;
        }

        public Task Delete(string partitionKey, string key)
        {
            if (_store.TryGetValue(partitionKey, out var entriesForPartition))
            {
                entriesForPartition.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }

        public Task Delete(string partitionKey, IEnumerable<string> keys)
        {
            if (_store.TryGetValue(partitionKey, out var entriesForPartition))
            {
                foreach (string key in keys)
                {
                    entriesForPartition.TryRemove(key, out _);
                }
            }

            return Task.CompletedTask;
        }

        private static void ThrowIfStateStorageEntryETagMismatch(IStateStorageEntry newEntry, IStateStorageEntry existingEntry)
        {
            if (!Object.ReferenceEquals(newEntry, existingEntry))
            {
                if (newEntry.ETag != existingEntry.ETag)
                {
                    throw new StateOptimisticConcurrencyViolationException($"An optimistic concurrency violation occurred when trying to save new state for: PartitionKey={newEntry.Namespace};Key={newEntry.Key}. The original ETag value was {newEntry.ETag}, but the current ETag value is {existingEntry.ETag}.");
                }
            }
        }

    }
}