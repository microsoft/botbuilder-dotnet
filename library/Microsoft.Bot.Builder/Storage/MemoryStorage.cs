using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Storage
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class DictionaryStorage : IStorage, IContextCreated
    {
        protected StoreItems memory;
        protected int eTag = 0;

        public DictionaryStorage(StoreItems dictionary = null)
        {
            this.memory = dictionary ?? new StoreItems();
        }

        public Task ContextCreated(BotContext context, CancellationToken token)
        {
            context.Storage = this;
            return Task.CompletedTask;
        }

        public Task Delete(string[] keys)
        {
            foreach (var key in keys)
            {
                this.memory.Remove(key);
            }
            return Task.CompletedTask;
        }

        public Task<StoreItems> Read(string[] keys)
        {
            var storeItems = new StoreItems();
            foreach (var key in keys)
            {
                object value;
                if (this.memory.TryGetValue(key, out value))
                    storeItems[key] = ((ICloneable)value).Clone() as StoreItem;
            }
            return Task.FromResult(storeItems);
        }


        public Task Write(StoreItems changes)
        {
            foreach (var change in changes)
            {
                StoreItem newValue = change.Value as StoreItem;
                StoreItem oldValue = null;
                object x;
                if (this.memory.TryGetValue(change.Key, out x))
                    oldValue = x as StoreItem;
                if (oldValue == null ||
                    newValue.eTag == "*" ||
                    oldValue.eTag == newValue.eTag)
                {
                    // clone and set etag
                    newValue = newValue.Clone() as StoreItem;
                    newValue.eTag = (this.eTag++).ToString();
                    this.memory[change.Key] = newValue;
                }
                else
                {
                    throw new Exception("etag conflict");
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
