using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Storage
{
    /// <summary>
    /// Models IStorage around a File System
    /// </summary>
    public class FileStorage : IStorage, IContextCreated
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        protected string folder;
        protected int eTag = 0;

        public FileStorage(string folder)
        {
            this.folder = folder;
        }

        public Task ContextCreated(BotContext context)
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
                var item = await ReadStoreItem(key).ConfigureAwait(false);
                if (item != null)
                    storeItems[key] = item;
            }
            return storeItems;
        }

        private async Task<StoreItem> ReadStoreItem(string key)
        {
            // The funky threading in here is due to concurrency and async methods. 
            // When this method is called, it may happen (in parallel) from any number of
            // thread. 
            //
            // If a write operation is in progress, the "OpenRead" will fail with an 
            // IOException. If this happens,the best thing to do is simply wait a moment
            // and retry. From the Docs:
            //      This method is equivalent to the FileStream(String, FileMode, 
            //      FileAccess, FileShare) constructor overload with a FileMode value 
            //      of Open, a FileAccess value of Read and a FileShare value of Read.

            key = SanitizeKey(key);
            string path = Path.Combine(this.folder, key);
            string json;
            DateTime start = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    using (TextReader file = new StreamReader(File.OpenRead(path)))
                    {
                        json = await file.ReadToEndAsync().ConfigureAwait(false);
                    }

                    return JsonConvert.DeserializeObject<StoreItem>(json, serializationSettings);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (IOException)
                {
                    if ((DateTime.UtcNow - start).TotalSeconds < 5)
                        await Task.Delay(0).ConfigureAwait(false);
                    else
                        throw;
                }
            }
        }

        public async Task Write(StoreItems changes)
        {
            // Similar to the Read method, the funky threading in here is due to 
            // concurrency and async methods. 
            // 
            // When this method is called, it may happen (in parallel) from any number of
            // thread. 
            //
            // If an operation is in progress, the Open will fail with an  
            // IOException. If this happens,the best thing to do is simply wait a moment
            // and retry. The Retry MUST go through the eTag processing again. 
            //
            // Alternate approach in here would be to use a SemaphoreSlim and use the async/await
            // constructs.

            foreach (var change in changes)
            {
                DateTime start = DateTime.UtcNow;
                while (true)
                {
                    try
                    {
                        StoreItem newValue = change.Value as StoreItem;
                        StoreItem oldValue = await this.ReadStoreItem(change.Key).ConfigureAwait(false);
                        if (oldValue == null ||
                            newValue.eTag == "*" ||
                            oldValue.eTag == newValue.eTag)
                        {
                            string key = SanitizeKey(change.Key);
                            string path = Path.Combine(this.folder, key);
                            var oldTag = newValue.eTag;
                            newValue.eTag = (this.eTag++).ToString();
                            var json = JsonConvert.SerializeObject(newValue, serializationSettings);
                            newValue.eTag = oldTag;
                            using (TextWriter file = new StreamWriter(path))
                            {
                                await file.WriteAsync(json).ConfigureAwait(false);
                                break;
                            }
                        }
                        else
                        {
                            throw new Exception($"etag conflict key={change}");
                        }
                    }
                    catch (IOException)
                    {
                        if ((DateTime.UtcNow - start).TotalSeconds < 5)
                            await Task.Delay(0).ConfigureAwait(false);
                        else
                            throw;
                    }
                }
            }

        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
        {
            char[] badChars = Path.GetInvalidFileNameChars();
            var dict = new Dictionary<char, string>();
            foreach (var badChar in badChars)
                dict[badChar] = '%' + ((int)badChar).ToString("x2");
            return dict;
        });

        private string SanitizeKey(string key)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in key)
            {
                if (badChars.Value.TryGetValue(ch, out string val))
                    sb.Append(val);
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

    }
}
