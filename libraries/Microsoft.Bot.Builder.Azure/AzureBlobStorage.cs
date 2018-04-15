// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Concurrency is managed in a per entity (e.g. per blob) basis. If an entity implement IStoreItem
    /// its eTag property value will be set with the blob's ETag upon Read. Afterward an AccessCondition
    /// with the ETag value will be generated during Write. New entities will simple have an null ETag.
    /// </remarks>
    public class AzureBlobStorage : IStorage
    {
        private readonly static JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            // we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All
        });

        private readonly CloudStorageAccount _storageAccount;
        private readonly string _containerName;
        private CloudBlobContainer _container;

        /// <summary>
        /// The Azure Storage Blob Container where entities will be stored
        /// </summary>

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
            _storageAccount = storageAccount ?? throw new ArgumentNullException(nameof(storageAccount));
            _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
            
            // Checks if a container name is valid
            NameValidator.ValidateContainerName(containerName);
        }

        /// <summary>
        /// Deletes entity blobs from the configured container
        /// </summary>
        /// <param name="keys">An array of entity keys</param>
        /// <returns></returns>
        public async Task Delete(string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var blobContainer = await GetBlobContainer().ConfigureAwait(false);
            await Task.WhenAll(
                keys.Select(key =>
                {
                    var blobName = GetBlobName(key);
                    var blobReference = blobContainer.GetBlobReference(blobName);
                    return blobReference.DeleteIfExistsAsync();
                }));
        }

        /// <summary>
        /// Retrieve entities from the configured blob container
        /// </summary>
        /// <param name="keys">An array of entity keys</param>
        /// <returns></returns>
        public async Task<IEnumerable<KeyValuePair<string, object>>> Read(params string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var blobContainer = await GetBlobContainer().ConfigureAwait(false);

            var readTasks = new List<Task<KeyValuePair<string, object>>>();

            foreach(var key in keys)
            {
                readTasks.Add(ReadIndividualKey(key));
            }

            await Task.WhenAll(readTasks);

            // Project back the entries that were read, filtering out any entries that were not found
            return readTasks.Select(readTask => readTask.Result).Where(kvp => kvp.Key != null);

            async Task<KeyValuePair<string, object>> ReadIndividualKey(string key)
            {
                var blobName = GetBlobName(key);
                var blobReference = blobContainer.GetBlobReference(blobName);

                try
                {
                    using (var blobStream = await blobReference.OpenReadAsync())
                    using (var jsonReader = new JsonTextReader(new StreamReader(blobStream)))
                    {
                        var obj = JsonSerializer.Deserialize(jsonReader);

                        if (obj is IStoreItem storeItem)
                        {
                            storeItem.eTag = blobReference.Properties.ETag;
                        }

                        return new KeyValuePair<string, object>(key, obj);
                    }
                }
                catch (StorageException ex)
                    when ((HttpStatusCode)ex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return new KeyValuePair<string, object>();
                }
                catch (AggregateException ex)
                    when (ex.InnerException is StorageException iex
                    && (HttpStatusCode)iex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return new KeyValuePair<string, object>();
                }
            }
        }

        /// <summary>
        /// Stores a new entity in the configured blob container
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        public async Task Write(IEnumerable<KeyValuePair<string, object>> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            var blobContainer = await GetBlobContainer().ConfigureAwait(false);

            await Task.WhenAll(
                changes.Select(async (keyValuePair) =>
                {
                    var newValue = keyValuePair.Value;
                    var storeItem = newValue as IStoreItem;
                    // "*" eTag in IStoreItem converts to null condition for AccessCondition
                    var calculatedETag = storeItem?.eTag == "*" ? null : storeItem?.eTag;

                    var blobName = GetBlobName(keyValuePair.Key);
                    var blobReference = blobContainer.GetBlockBlobReference(blobName);
                    using (var blobStream = await blobReference.OpenWriteAsync(
                        AccessCondition.GenerateIfMatchCondition(calculatedETag),
                        new BlobRequestOptions(),
                        new OperationContext()))
                    using (var jsonWriter = new JsonTextWriter(new StreamWriter(blobStream)))
                    {
                        JsonSerializer.Serialize(jsonWriter, newValue);
                    }
                }));
        }

        private static string GetBlobName(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var blobName = HttpUtility.UrlEncode(key);
            NameValidator.ValidateBlobName(blobName);
            return blobName;
        }

        private ValueTask<CloudBlobContainer> GetBlobContainer()
        {
            if(_container != null)
            {
                return new ValueTask<CloudBlobContainer>(_container);
            }

            return new ValueTask<CloudBlobContainer>(EnsureBlobContainerExists());

            async Task<CloudBlobContainer> EnsureBlobContainerExists()
            {
                var blobClient = _storageAccount.CreateCloudBlobClient();
                _container = blobClient.GetContainerReference(_containerName);

                await _container.CreateIfNotExistsAsync().ConfigureAwait(false);

                return _container;
            }
        }
    }
}