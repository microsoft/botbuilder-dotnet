// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Storage
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class DictionaryStorage : IStorage
    {
        private readonly StoreItems _memory;
        private int _eTag = 0;
        private object _syncroot = new object();

        public DictionaryStorage(StoreItems dictionary = null)
        {
            _memory = dictionary ?? new StoreItems();
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

        public Task<StoreItems> Read(string[] keys)
        {
            var storeItems = new StoreItems();
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    if (_memory.TryGetValue(key, out object value))
                    {
                        if (value != null)
                            storeItems[key] = FlexObject.Clone<IStoreItem>(value);
                        else 
                            storeItems[key] = null;
                    }
                }
            }
            return Task.FromResult(storeItems);
        }


        public Task Write(StoreItems changes)
        {
            lock (_syncroot)
            {
                foreach (var change in changes)
                {
                    IStoreItem newValue = change.Value as IStoreItem;
                    IStoreItem oldValue = null;

                    if (_memory.TryGetValue(change.Key, out object x))
                        oldValue = x as IStoreItem;
                    if (oldValue == null ||
                        newValue.eTag == "*" ||
                        oldValue.eTag == newValue.eTag)
                    {
                        // clone and set etag
                        newValue = FlexObject.Clone<IStoreItem>(newValue);
                        newValue.eTag = (_eTag++).ToString();
                        _memory[change.Key] = newValue;
                    }
                    else
                    {
                        throw new Exception("etag conflict");
                    }
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
