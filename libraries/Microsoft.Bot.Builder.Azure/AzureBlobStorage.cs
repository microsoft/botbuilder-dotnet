// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class AzureBlobStorage : IStorage
    {
        private readonly static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            // we use all so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All
        };

        public Lazy<CloudBlobContainer> Container { get; private set; }

        public AzureBlobStorage(string dataConnectionString, string containerName)
            : this(CloudStorageAccount.Parse(dataConnectionString), containerName)
        {
        }

        public AzureBlobStorage(CloudStorageAccount storageAccount, string containerName)
        {
            this.Container = new Lazy<CloudBlobContainer>(() =>
              {
                  var blobClient = storageAccount.CreateCloudBlobClient();
                  var container = blobClient.GetContainerReference(containerName);
                  container.CreateIfNotExistsAsync().Wait();
                  return container;
              });
        }

        protected string GetKey(string key)
        {
            return HttpUtility.UrlEncode(key);
        }

        public async Task Delete(string[] keys)
        {
            foreach (var key in keys.Select(k => GetKey(k)))
            {
                var blobReference = this.Container.Value.GetBlobReference(key);
                await blobReference.DeleteAsync(
                    DeleteSnapshotsOption.None, new AccessCondition { IfMatchETag = "*" }, new BlobRequestOptions(), new OperationContext()).ConfigureAwait(false);
            }
        }

        public async Task<StoreItems> Read(string[] keys)
        {
            var storeItems = new StoreItems();
            foreach (string key in keys.Select(k => GetKey(k)))
            {
                using (var memoryStream = new MemoryStream())
                {
                    var blobReference = this.Container.Value.GetBlobReference(key);
                    await blobReference.DownloadToStreamAsync(memoryStream).ConfigureAwait(false);
                    using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
                    {
                        var json = streamReader.ReadToEnd();
                        var obj = JsonConvert.DeserializeObject(json, serializationSettings);
                        IStoreItem storeItem = obj as IStoreItem;
                        if (storeItem != null)
                        {
                            storeItem.eTag = blobReference.Properties.ETag;
                        }

                        storeItems[key] = storeItem;
                    }
                }
            }

            return storeItems;
        }


        public async Task Write(StoreItems changes)
        {
            foreach (var change in changes)
            {
                var key = GetKey(change.Key);
                var newValue = change.Value as IStoreItem;
                var json = JsonConvert.SerializeObject(newValue, Formatting.None, serializationSettings);
                var blobReference = this.Container.Value.GetBlockBlobReference(key);

                await blobReference.UploadTextAsync(json, new AccessCondition { IfMatchETag = newValue.eTag ?? "*" }, new BlobRequestOptions(), new OperationContext()).ConfigureAwait(false);
            }
        }
    }
}