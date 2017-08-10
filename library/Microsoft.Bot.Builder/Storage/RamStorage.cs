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
    public class DictionaryStorage : IStorage, IContextInitializer
    {
        protected Dictionary<string, StoreItem> memory;
        protected int eTag = 0;

        public DictionaryStorage(Dictionary<string, StoreItem> dictionary = null)
        {
            this.memory = dictionary ?? new Dictionary<string, StoreItem>();
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

        public Task<Dictionary<string, StoreItem>> Read(string[] keys)
        {
            var storeItems = new Dictionary<string, StoreItem>();
            foreach (var key in keys)
            {
                StoreItem value;
                if (this.memory.TryGetValue(key, out value))
                    storeItems[key] = value;
            }
            return Task.FromResult(storeItems);
        }

        public Task Write(Dictionary<string, StoreItem> changes)
        {
            foreach (var key in changes.Keys)
            {
                StoreItem old = this.memory[key];
                if (old == null ||
                    changes[key].eTag == "*" ||
                    old.eTag == changes[key].eTag)
                {
                    this.memory[key] = JsonConvert.DeserializeObject<StoreItem>(JsonConvert.SerializeObject(changes[key]));
                    this.memory[key].eTag = (this.eTag++).ToString();
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
    public class RamStorage : DictionaryStorage
    {
        public RamStorage() : base(null)
        {
        }
    }
}
