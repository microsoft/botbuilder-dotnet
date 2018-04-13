// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class DictionaryStorage : IStorage
    {
        private static readonly JsonSerializer StateJsonSerializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Objects };

        private readonly Dictionary<string, JObject> _memory;
        private readonly object _syncroot = new object();
        private int _eTag = 0;

        public DictionaryStorage(Dictionary<string, JObject> dictionary = null)
        {
            _memory = dictionary ?? new Dictionary<string, JObject>();
        }
                
        public Task Delete(string[] keys)
        {
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    _memory.Remove(key);
                }
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<KeyValuePair<string, object>>> Read(string[] keys)
        {
            var storeItems = new List<KeyValuePair<string, object>>(keys.Length);
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    if (_memory.TryGetValue(key, out var state))
                    {
                        if (state != null)
                        {
                            storeItems.Add(new KeyValuePair<string, object>(key, state.ToObject<object>(StateJsonSerializer)));
                        }
                    }
                }
            }

            return Task.FromResult<IEnumerable<KeyValuePair<string, object>>>(storeItems);
        }


        public Task Write(IEnumerable<KeyValuePair<string, object>> changes)
        {
            lock (_syncroot)
            {
                foreach (var change in changes)
                {
                    var newValue = change.Value;

                    var oldStateETag = default(string);

                    if(_memory.TryGetValue(change.Key, out var oldState))
                    {
                        if (oldState.TryGetValue("eTag", out var eTagToken))
                        {
                            oldStateETag = eTagToken.Value<string>();
                        }
                    }
                    
                    var newState = JObject.FromObject(newValue, StateJsonSerializer);

                    // Set ETag if applicable
                    if (newValue is IStoreItem newStoreItem)
                    {
                        if(oldStateETag != null
                                &&
                           newStoreItem.eTag != "*"
                                &&
                           newStoreItem.eTag != oldStateETag)
                        {
                            throw new Exception($"Etag conflict.\r\n\r\nOriginal: {newStoreItem.eTag}\r\nCurrent: {oldStateETag}");
                        }

                        newState["eTag"] = (_eTag++).ToString();
                    }

                    _memory[change.Key] = newState;
                }
            }

            return Task.CompletedTask;
        }

    }

    /// <summary>
    /// RamStorage stores data in volative dictionary
    /// </summary>
    public class MemoryStorage : DictionaryStorage
    {
        public MemoryStorage() : base(null)
        {
        }
    }
}
