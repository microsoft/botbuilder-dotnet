// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Storage
{
    /// <summary>
    /// Implements <see cref="IStorage"/> using Azure Storage Blobs.
    /// </summary>
    /// <remarks>
    /// This class uses a single Azure Storage Blob Container.
    /// Each entity or <see cref="IStoreItem"/> is serialized into a JSON string and stored in an individual text blob.
    /// Each blob is named after the store item key,  which is encoded so that it conforms a valid blob name.
    /// If an entity is an <see cref="IStoreItem"/>, the storage object will set the entity's <see cref="IStoreItem.ETag"/>
    /// property value to the blob's ETag upon read. Afterward, an <see cref="BlobRequestConditions"/> with the ETag value
    /// will be generated during Write. New entities start with a null ETag.
    /// </remarks>
    public class Blobs : IStorage
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            // we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All,
        });

        // If a JsonSerializer is not provided during construction, this will be the default static JsonSerializer.
        private readonly JsonSerializer _jsonSerializer;
        private readonly BlobContainerClient _containerClient;
        private int _checkforContainerExistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Blobs"/> class.
        /// </summary>
        /// <param name="dataConnectionstring">Azure Storage connection string.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.All.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public Blobs(string dataConnectionstring, string containerName, JsonSerializer jsonSerializer = null)
        {
            if (string.IsNullOrEmpty(dataConnectionstring))
            { 
                throw new ArgumentNullException(nameof(dataConnectionstring)); 
            }
            
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            _jsonSerializer = jsonSerializer ?? JsonSerializer;

            // Triggers a check for the existance of the container
            _checkforContainerExistance = 1;

            _containerClient = new BlobContainerClient(dataConnectionstring, containerName);
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

            foreach (var key in keys)
            {
                var blobName = GetBlobName(key);
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);   
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

            // this should only happen once - assuming this is a singleton
            if (Interlocked.CompareExchange(ref _checkforContainerExistance, 0, 1) == 1)
            {
                await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var items = new Dictionary<string, object>();

            foreach (var key in keys)
            {
                var blobName = GetBlobName(key);
                var blobClient = _containerClient.GetBlobClient(blobName);
                try
                {
                    items.Add(key, await InnerReadBlobAsync(blobClient, cancellationToken).ConfigureAwait(false));
                }
                catch (RequestFailedException ex)
                    when ((HttpStatusCode)ex.Status == HttpStatusCode.NotFound)
                {
                    continue;
                }
                catch (AggregateException ex)
                    when (ex.InnerException is RequestFailedException iex
                    && (HttpStatusCode)iex.Status == HttpStatusCode.NotFound)
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

            // this should only happen once - assuming this is a singleton
            if (Interlocked.CompareExchange(ref _checkforContainerExistance, 0, 1) == 1)
            {
                await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            foreach (var keyValuePair in changes)
            {
                var newValue = keyValuePair.Value;
                var storeItem = newValue as IStoreItem;

                // "*" eTag in IStoreItem converts to null condition for AccessCondition
                var accessCondition = (!string.IsNullOrEmpty(storeItem?.ETag) && storeItem?.ETag != "*")
                    ? new BlobRequestConditions() { IfMatch = new ETag(storeItem?.ETag) }
                    : null;
                
                var blobName = GetBlobName(keyValuePair.Key);
                var blobReference = _containerClient.GetBlobClient(blobName);
                try
                {
                    using (var memoryStream = new MemoryStream())
                    using (var streamWriter = new StreamWriter(memoryStream))
                    {
                        _jsonSerializer.Serialize(streamWriter, newValue);
                        streamWriter.Flush();
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await blobReference.UploadAsync(memoryStream, conditions: accessCondition, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (RequestFailedException ex)
                when (ex.Status == (int)HttpStatusCode.BadRequest
                && ex.ErrorCode == BlobErrorCode.InvalidBlockList)
                {
                    throw new Exception(
                        $"An error ocurred while trying to write an object. The underlying '{BlobErrorCode.InvalidBlockList}' error is commonly caused due to concurrently uploading an object larger than 128MB in size.",
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

            return HttpUtility.UrlEncode(key);
        }

        private async Task<object> InnerReadBlobAsync(BlobClient blobReference, CancellationToken cancellationToken)
        {
            var i = 0;
            while (true)
            {
                try
                {
                    using (BlobDownloadInfo download = await blobReference.DownloadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        using (var jsonReader = new JsonTextReader(new StreamReader(download.Content)))
                        {
                            var obj = _jsonSerializer.Deserialize(jsonReader);

                            if (obj is IStoreItem storeItem)
                            {
                                storeItem.ETag = (await blobReference.GetPropertiesAsync().ConfigureAwait(false))?.Value?.ETag.ToString();
                            }

                            return obj;
                        }
                    }
                }
                catch (RequestFailedException ex)
                    when ((HttpStatusCode)ex.Status == HttpStatusCode.PreconditionFailed)
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
