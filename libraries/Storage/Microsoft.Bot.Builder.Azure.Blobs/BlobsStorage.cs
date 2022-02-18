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
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Azure.Blobs
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
    public class BlobsStorage : IStorage
    {
        private readonly BlobsStorageOptions _options;
        private readonly BlobContainerClient _containerClient;
        private int _checkForContainerExistence;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsStorage"/> class.
        /// </summary>
        /// <param name="dataConnectionString">Azure Storage connection string.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.None.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public BlobsStorage(string dataConnectionString, string containerName, JsonSerializer jsonSerializer = null)
            : this(new BlobsStorageOptions(dataConnectionString, containerName, default, jsonSerializer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsStorage"/> class.
        /// </summary>
        /// <param name="options"><see cref="BlobsStorageOptions"/> to use for this <see cref="BlobsStorage"/>.</param>
        public BlobsStorage(BlobsStorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;

            // Triggers a check for the existence of the container
            _checkForContainerExistence = 1;

            _containerClient = new BlobContainerClient(options.DataConnectionString, options.ContainerName);
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
            if (Interlocked.CompareExchange(ref _checkForContainerExistence, 0, 1) == 1)
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
            if (Interlocked.CompareExchange(ref _checkForContainerExistence, 0, 1) == 1)
            {
                await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            foreach (var keyValuePair in changes)
            {
                var newValue = keyValuePair.Value;

                string eTag = _options.EnforceEtag ? ETagHelper.GetETagOrNull(newValue) : "*";

                // Empty string etag is not allowed, for consistency among IStorage implementations
                if (_options.EnforceEtag && eTag?.Length == 0)
                {
                    throw new State.StoreItemETagException("etag empty");
                }

                // "*" eTag in IStoreItem converts to null condition for AccessCondition
                var accessCondition = (!string.IsNullOrEmpty(eTag) && eTag != "*")
                    ? new BlobRequestConditions() { IfMatch = new ETag(eTag) }
                    : null;

                var blobName = GetBlobName(keyValuePair.Key);
                var blobReference = _containerClient.GetBlobClient(blobName);
                
                try
                {
                    using var memoryStream = new MemoryStream();
                    using var streamWriter = new StreamWriter(memoryStream);
                    _options.JsonSerializer.Serialize(streamWriter, newValue);
                    await streamWriter.FlushAsync().ConfigureAwait(false);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    Response<BlobContentInfo> result;
                    try
                    {
                        result = await blobReference.UploadAsync(memoryStream, conditions: accessCondition, transferOptions: _options.StorageTransferOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    catch (RequestFailedException ex)
                        when (ex.Status == (int)HttpStatusCode.PreconditionFailed
                        && ex.ErrorCode == BlobErrorCode.ConditionNotMet)
                    {
                        // If we received an etag violation (ConditionNotMet), ensure the record actually exists since blobs storage will not create
                        // the record if it does not exist and an etag was provided.
                        if (!string.IsNullOrEmpty(eTag) && eTag != "*")
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            result = await blobReference.UploadAsync(memoryStream, conditions: new BlobRequestConditions() { IfNoneMatch = new ETag("*") }, transferOptions: _options.StorageTransferOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    if (_options.EnforceEtag)
                    {
                        ETagHelper.UpdateETag(newValue, result.Value.ETag.ToString());
                    }
                }
                catch (RequestFailedException ex)
                    when (ex.Status == (int)HttpStatusCode.BadRequest
                    && ex.ErrorCode == BlobErrorCode.InvalidBlockList)
                {
                    throw new InvalidOperationException(
                        $"An error occurred while trying to write an object. The underlying '{BlobErrorCode.InvalidBlockList}' error is commonly caused due to concurrently uploading an object larger than 128MB in size.",
                        ex);
                }
                catch (RequestFailedException ex)
                   when (ex.Status == (int)HttpStatusCode.Conflict
                   && ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
                {
                    throw new State.StoreItemETagException("Etag conflict.", ex);
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
                            var obj = _options.JsonSerializer.Deserialize(jsonReader);

                            if (_options.EnforceEtag)
                            {
                                if (obj is IStoreItem storeItem)
                                {
                                    storeItem.ETag = (await blobReference.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false))?.Value?.ETag.ToString();
                                }
                                else if (obj is JObject asJobject)
                                {
                                    asJobject["ETag"] = (await blobReference.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false))?.Value?.ETag.ToString();
                                }
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
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
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
