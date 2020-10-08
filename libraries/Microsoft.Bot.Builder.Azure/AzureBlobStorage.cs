// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Implements <see cref="IStorage"/> using Azure Blob Storage.
    /// </summary>
    /// <remarks>
    /// This class uses a single Azure Storage Blob Container.
    /// Each entity or <see cref="IStoreItem"/> is serialized into a JSON string and stored in an individual text blob.
    /// Each blob is named after the store item key,  which is encoded so that it conforms a valid blob name.
    /// If an entity is an <see cref="IStoreItem"/>, the storage object will set the entity's <see cref="IStoreItem.ETag"/>
    /// property value to the blob's ETag upon read. Afterward, an <see cref="AccessCondition"/> with the ETag value
    /// will be generated during Write. New entities start with a null ETag.
    /// </remarks>
    [Obsolete("This class is deprecated. Please use BlobsStorage from Microsoft.Bot.Builder.Azure.Blobs instead.")]
    public class AzureBlobStorage : IStorage
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            // we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All,
        });

        // If a JsonSerializer is not provided during construction, this will be the default static JsonSerializer.
        private readonly JsonSerializer _jsonSerializer;
        private readonly CloudStorageAccount _storageAccount;
        private readonly string _containerName;
        private int _checkforContainerExistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorage"/> class.
        /// </summary>
        /// <param name="storageAccount">Azure CloudStorageAccount instance.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.All.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public AzureBlobStorage(CloudStorageAccount storageAccount, string containerName, JsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _storageAccount = storageAccount ?? throw new ArgumentNullException(nameof(storageAccount));
            _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));

            // Checks if a container name is valid
            NameValidator.ValidateContainerName(containerName);

            // Triggers a check for the existance of the container
            _checkforContainerExistance = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorage"/> class.
        /// </summary>
        /// <param name="dataConnectionstring">Azure Storage connection string.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        public AzureBlobStorage(string dataConnectionstring, string containerName)
            : this(CloudStorageAccount.Parse(dataConnectionstring), containerName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorage"/> class.
        /// </summary>
        /// <param name="storageAccount">Azure CloudStorageAccount instance.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        public AzureBlobStorage(CloudStorageAccount storageAccount, string containerName)
            : this(storageAccount, containerName, JsonSerializer)
        {
        }

        /// <summary>
        /// Deletes entity blobs from the configured container.
        /// </summary>
        /// <param name="keys">An array of entity keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            var blobClient = _storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_containerName);
            foreach (var key in keys)
            {
                var blobName = GetBlobName(key);
                var blobReference = blobContainer.GetBlobReference(blobName);
                await blobReference.DeleteIfExistsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieve entities from the configured blob container.
        /// </summary>
        /// <param name="keys">An array of entity keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            var blobClient = _storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_containerName);

            var items = new Dictionary<string, object>();

            foreach (var key in keys)
            {
                var blobName = GetBlobName(key);
                var blobReference = blobContainer.GetBlobReference(blobName);

                try
                {
                    items.Add(key, await InnerReadBlobAsync(blobReference, cancellationToken).ConfigureAwait(false));
                }
                catch (StorageException ex)
                    when ((HttpStatusCode)ex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }
                catch (AggregateException ex)
                    when (ex.InnerException is StorageException iex
                    && (HttpStatusCode)iex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }
            }

            return items;
        }

        /// <summary>
        /// Stores a new entity in the configured blob container.
        /// </summary>
        /// <param name="changes">The changes to write to storage.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            var blobClient = _storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_containerName);

            // this should only happen once - assuming this is a singleton
            if (Interlocked.CompareExchange(ref _checkforContainerExistance, 0, 1) == 1)
            {
                await blobContainer.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
            }

            var blobRequestOptions = new BlobRequestOptions();
            var operationContext = new OperationContext();

            foreach (var keyValuePair in changes)
            {
                var newValue = keyValuePair.Value;
                var storeItem = newValue as IStoreItem;

                // "*" eTag in IStoreItem converts to null condition for AccessCondition
                var accessCondition = storeItem?.ETag != "*"
                    ? AccessCondition.GenerateIfMatchCondition(storeItem?.ETag)
                    : AccessCondition.GenerateEmptyCondition();

                var blobName = GetBlobName(keyValuePair.Key);
                var blobReference = blobContainer.GetBlockBlobReference(blobName);

                try
                {
                    using (var memoryStream = new MultiBufferMemoryStream(blobReference.ServiceClient.BufferManager))
                    using (var streamWriter = new StreamWriter(memoryStream))
                    {
                        _jsonSerializer.Serialize(streamWriter, newValue);
                        streamWriter.Flush();
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await blobReference.UploadFromStreamAsync(memoryStream, accessCondition, blobRequestOptions, operationContext, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (StorageException ex)
                when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadRequest
                && ex.RequestInformation.ErrorCode == BlobErrorCodeStrings.InvalidBlockList)
                {
                    throw new Exception(
                        $"An error ocurred while trying to write an object. The underlying '{BlobErrorCodeStrings.InvalidBlockList}' error is commonly caused due to concurrently uploading an object larger than 128MB in size.",
                        ex);
                }
            }
        }

        private static string GetBlobName(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var blobName = HttpUtility.UrlEncode(key);
            NameValidator.ValidateBlobName(blobName);
            return blobName;
        }

        private async Task<object> InnerReadBlobAsync(CloudBlob blobReference, CancellationToken cancellationToken)
        {
            var i = 0;
            while (true)
            {
                try
                {
                    // add request options to retry on timeouts and server errors
                    var options = new BlobRequestOptions { RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(20), 4) };

                    using (var blobStream = await blobReference.OpenReadAsync(null, options, new OperationContext(), cancellationToken).ConfigureAwait(false))
                    using (var jsonReader = new JsonTextReader(new StreamReader(blobStream)))
                    {
                        var obj = _jsonSerializer.Deserialize(jsonReader);

                        if (obj is IStoreItem storeItem)
                        {
                            storeItem.ETag = blobReference.Properties.ETag;
                        }

                        return obj;
                    }
                }
                catch (StorageException ex)
                    when ((HttpStatusCode)ex.RequestInformation.HttpStatusCode == HttpStatusCode.PreconditionFailed)
                {
                    // additional retry logic, even though this is a read operation blob storage can return 412 if there is contention
                    if (i++ < 8)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
