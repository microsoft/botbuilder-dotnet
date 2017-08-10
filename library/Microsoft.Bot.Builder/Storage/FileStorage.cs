using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Storage
{
    /// <summary>
    /// Models IStorage around a File System
    /// </summary>
    public class FileStorage : IStorage, IContextInitializer
    {
        protected string folder;
        protected int eTag = 0;

        public FileStorage(string folder)
        {
            this.folder = folder;
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
                File.Delete(Path.Combine(folder, key));
            }
            return Task.CompletedTask;
        }

        public async Task<Dictionary<string, StoreItem>> Read(string[] keys)
        {
            var storeItems = new Dictionary<string, StoreItem>();
            foreach (var key in keys)
            {
                storeItems[key] = await ReadStoreItem(key);
            }
            return storeItems;
        }

        private async Task<StoreItem> ReadStoreItem(string key)
        {
            try
            {
                string path = Path.Combine(this.folder, key);
                using (TextReader file = File.OpenText(path))
                {
                    string json = await file.ReadToEndAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<StoreItem>(json);
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public async Task Write(Dictionary<string, StoreItem> changes)
        {
            foreach (var key in changes.Keys)
            {
                StoreItem old = await this.ReadStoreItem(key);
                if (old == null ||
                    changes[key].eTag == "*" ||
                    old.eTag == changes[key].eTag)
                {
                    string path = Path.Combine(this.folder, key);
                    var oldTag = changes[key].eTag;
                    changes[key].eTag = (this.eTag++).ToString();
                    var json = JsonConvert.SerializeObject(changes[key]);
                    changes[key].eTag = oldTag;
                    File.WriteAllText(path, json);
                }
                else
                {
                    throw new Exception($"etag conflict key={key}");
                }
            }
        }
    }
}
