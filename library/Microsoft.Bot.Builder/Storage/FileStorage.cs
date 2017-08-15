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

        public async Task<StoreItems> Read(string[] keys)
        {
            var storeItems = new StoreItems();
            foreach (var key in keys)
            {
                var item = await ReadStoreItem(key);
                if (item != null)
                    storeItems[key] = item;
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

        public async Task Write(StoreItems changes)
        {
            foreach (var change in changes)
            {
                StoreItem newValue = change.Value as StoreItem;
                StoreItem oldValue = await this.ReadStoreItem(change.Key);
                if (oldValue == null ||
                    newValue.eTag == "*" ||
                    oldValue.eTag == newValue.eTag)
                {
                    string path = Path.Combine(this.folder, change.Key);
                    var oldTag = newValue.eTag;
                    newValue.eTag = (this.eTag++).ToString();
                    var json = JsonConvert.SerializeObject(newValue);
                    newValue.eTag = oldTag;
                    File.WriteAllText(path, json);
                }
                else
                {
                    throw new Exception($"etag conflict key={change}");
                }
            }
        }
    }
}
