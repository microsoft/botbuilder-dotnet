using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Core.State
{
    public class MemoryStateStorageProvider : IStateStorageProvider
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, JObject>> _store = new ConcurrentDictionary<string, ConcurrentDictionary<string, JObject>>();

        public MemoryStateStorageProvider()
        {
        }

        public IStateStorageEntry CreateNewEntry(string stateNamespace, string key) => new MemoryStateStorageEntry(stateNamespace, key);

        public Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace)
        {
            if (string.IsNullOrEmpty(stateNamespace))
            {
                throw new ArgumentException("Expected a non-null/empty value.", nameof(stateNamespace));
            }

            if (_store.TryGetValue(stateNamespace, out var statesForPartition))
            {
                return Task.FromResult<IEnumerable<IStateStorageEntry>>(statesForPartition.Select(kvp => new MemoryStateStorageEntry(stateNamespace, kvp.Key, kvp.Value)));
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
                if (entriesForPartion.TryGetValue(key, out var state))
                {
                    return Task.FromResult<IStateStorageEntry>(new MemoryStateStorageEntry(stateNamespace, key, state));
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
                        results.Add(new MemoryStateStorageEntry(stateNamespace, key, entry));
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

            var entriesForPartition = _store.GetOrAdd(stateNamespace, pk => new ConcurrentDictionary<string, JObject>());

            foreach (var entry in values)
            {
                var newStateValue = JObject.FromObject(entry.Value);

                entriesForPartition.AddOrUpdate(
                    entry.Key,
                    newStateValue,
                    (key, existingStateStorageEntry) => newStateValue);
            }

            return Task.CompletedTask;
        }

        public Task Save(IEnumerable<IStateStorageEntry> entries)
        {
            foreach (var entriesByNamespace in entries.GroupBy(e => e.Namespace))
            {
                var statesForNamespace = _store.GetOrAdd(entriesByNamespace.Key, pk => new ConcurrentDictionary<string, JObject>());

                foreach (var entry in entriesByNamespace)
                {
                    if (!(entry is MemoryStateStorageEntry stateStorageEntry))
                    {
                        throw new InvalidOperationException($"Only instances of {nameof(MemoryStateStorageEntry)} are supported by {nameof(MemoryStateStorageProvider)}.");
                    }

                    var newStateValue = JObject.FromObject(stateStorageEntry.RawValue);

                    statesForNamespace[stateStorageEntry.Key] = newStateValue;
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

        internal sealed class MemoryStateStorageEntry : DeferredValueStateStorageEntry
        {
            private readonly JObject _serializedState;

            public MemoryStateStorageEntry(string stateNamespace, string key) : base(stateNamespace, key)
            {
            }

            public MemoryStateStorageEntry(string stateNamespace, string key, JObject serializedState) : base(stateNamespace, key)
            {
                _serializedState = serializedState;
            }

            protected override T MaterializeValue<T>() => _serializedState?.ToObject<T>();
        }
    }
}