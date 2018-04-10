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
    /// Models IStorage using Azure Storge Blobs
    /// </summary>
    /// <remarks>
    /// The AzureBlobStorage implements State's IStorage using a single Azure Storage Blob Container.
    /// Each entity or StoreItem is serialized into a JSON string and stored in an individual text blob.
    /// Each blob is named after the StoreItem key which is encoded and ensure it conforms a valid blob name.
    /// </remarks>
    public class AzureBlobStorage : IStorage
    {
        private readonly static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            // we use all so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All
        };

        /// <summary>
        /// The Azure Storage Blob Container where entities will be stored
        /// </summary>
        public Lazy<CloudBlobContainer> Container { get; private set; }

        /// <summary>
        /// Creates the AzureBlobStorage instance
        /// </summary>
        /// <param name="dataConnectionString">Azure Storage connection string</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored</param>
        public AzureBlobStorage(string dataConnectionString, string containerName)
            : this(CloudStorageAccount.Parse(dataConnectionString), containerName)
        {
        }

        /// <summary>
        /// Creates the AzureBlobStorage instance
        /// </summary>
        /// <param name="storageAccount">Azure CloudStorageAccount instance</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored</param>
        public AzureBlobStorage(CloudStorageAccount storageAccount, string containerName)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            // Checks if a container name is valid
            NameValidator.ValidateContainerName(containerName);

            this.Container = new Lazy<CloudBlobContainer>(() =>
              {
                  var blobClient = storageAccount.CreateCloudBlobClient();
                  var container = blobClient.GetContainerReference(containerName);
                  container.CreateIfNotExistsAsync().Wait();
                  return container;
              });
        }

        /// <summary>
        /// Get a blob name validated representation of an entity
        /// </summary>
        /// <param name="key">The key used to identify the entity</param>
        /// <returns></returns>
        private static string GetBlobName(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var blobName = HttpUtility.UrlEncode(key);
            NameValidator.ValidateBlobName(blobName);
            return blobName;
        }

        /// <summary>
        /// Deletes entity blobs from the configured container
        /// </summary>
        /// <param name="keys">An array of entitiy keys</param>
        /// <returns></returns>
        public async Task Delete(string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (var blobName in keys.Select(GetBlobName))
            {
                var blobReference = this.Container.Value.GetBlobReference(blobName);
                await blobReference.DeleteAsync(
                    DeleteSnapshotsOption.None, new AccessCondition { IfMatchETag = "*" }, new BlobRequestOptions(), new OperationContext()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieve entities from the configured blob container
        /// </summary>
        /// <param name="keys">An array of entitiy keys</param>
        /// <returns></returns>
        public async Task<StoreItems> Read(string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var storeItems = new StoreItems();
            foreach (string blobName in keys.Select(GetBlobName))
            {
                using (var memoryStream = new MemoryStream())
                {
                    var blobReference = this.Container.Value.GetBlobReference(blobName);
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

                        storeItems[blobName] = storeItem;
                    }
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Stores a new entity in the configured blob container
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        public async Task Write(StoreItems changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            foreach (var change in changes)
            {
                var blobName = GetBlobName(change.Key);
                var newValue = change.Value as IStoreItem;
                var json = JsonConvert.SerializeObject(newValue, Formatting.None, serializationSettings);
                var blobReference = this.Container.Value.GetBlockBlobReference(blobName);

                await blobReference.UploadTextAsync(json, new AccessCondition { IfMatchETag = newValue.eTag ?? "*" }, new BlobRequestOptions(), new OperationContext()).ConfigureAwait(false);
            }
        }
    }
}