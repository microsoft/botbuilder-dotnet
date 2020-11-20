// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Implements an CosmosDB based storage provider using partitioning for a bot.
    /// </summary>
    public class CosmosDbPartitionedStorage : IStorage, IDisposable
    {
        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        private Container _container;
        private readonly CosmosDbPartitionedStorageOptions _cosmosDbStorageOptions;
        private CosmosClient _client;
        private bool _compatibilityModePartitionKey;

        // To detect redundant calls to dispose
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbPartitionedStorage"/> class.
        /// using the provided CosmosDB credentials, database ID, and container ID.
        /// </summary>
        /// <param name="cosmosDbStorageOptions">Cosmos DB partitioned storage configuration options.</param>
        public CosmosDbPartitionedStorage(CosmosDbPartitionedStorageOptions cosmosDbStorageOptions)
        {
            if (cosmosDbStorageOptions == null)
            {
                throw new ArgumentNullException(nameof(cosmosDbStorageOptions));
            }

            if (cosmosDbStorageOptions.CosmosDbEndpoint == null)
            {
                throw new ArgumentException($"Service EndPoint for CosmosDB is required.", nameof(cosmosDbStorageOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.AuthKey))
            {
                throw new ArgumentException("AuthKey for CosmosDB is required.", nameof(cosmosDbStorageOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.DatabaseId))
            {
                throw new ArgumentException("DatabaseId is required.", nameof(cosmosDbStorageOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.ContainerId))
            {
                throw new ArgumentException("ContainerId is required.", nameof(cosmosDbStorageOptions));
            }

            if (!string.IsNullOrWhiteSpace(cosmosDbStorageOptions.KeySuffix))
            {
                if (cosmosDbStorageOptions.CompatibilityMode)
                {
                    throw new ArgumentException($"CompatibilityMode cannot be 'true' while using a KeySuffix.", nameof(cosmosDbStorageOptions));
                }

                // In order to reduce key complexity, we do not allow invalid characters in a KeySuffix
                // If the KeySuffix has invalid characters, the EscapeKey will not match
                var suffixEscaped = CosmosDbKeyEscape.EscapeKey(cosmosDbStorageOptions.KeySuffix);
                if (!cosmosDbStorageOptions.KeySuffix.Equals(suffixEscaped, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Cannot use invalid Row Key characters: {cosmosDbStorageOptions.KeySuffix}", nameof(cosmosDbStorageOptions));
                }
            }

            _cosmosDbStorageOptions = cosmosDbStorageOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbPartitionedStorage"/> class.
        /// using the provided CosmosDB credentials, database ID, and collection ID.
        /// </summary>
        /// <param name="cosmosDbStorageOptions">Cosmos DB partitioned storage configuration options.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.All.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public CosmosDbPartitionedStorage(CosmosDbPartitionedStorageOptions cosmosDbStorageOptions, JsonSerializer jsonSerializer)
            : this(cosmosDbStorageOptions)
        {
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        /// <summary>
        /// Reads one or more items with matching keys from the Cosmos DB container.
        /// </summary>
        /// <param name="keys">A collection of Ids for each item to be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A dictionary containing the retrieved items.</returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the array of keys (Ids for the items to be retrieved) is null.</exception>
        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0)
            {
                // No keys passed in, no result to return.
                return new Dictionary<string, object>();
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            var storeItems = new Dictionary<string, object>(keys.Length);

            foreach (var key in keys)
            {
                try
                {
                    var escapedKey = CosmosDbKeyEscape.EscapeKey(key, _cosmosDbStorageOptions.KeySuffix, _cosmosDbStorageOptions.CompatibilityMode);

                    var readItemResponse = await _container.ReadItemAsync<DocumentStoreItem>(
                            escapedKey,
                            GetPartitionKey(escapedKey),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    var documentStoreItem = readItemResponse.Resource;
                    var item = documentStoreItem.Document.ToObject(typeof(object), _jsonSerializer);

                    if (item is IStoreItem storeItem)
                    {
                        storeItem.ETag = documentStoreItem.ETag;
                        storeItems.Add(documentStoreItem.RealId, storeItem);
                    }
                    else
                    {
                        storeItems.Add(documentStoreItem.RealId, item);
                    }
                }
                catch (CosmosException exception)
                {
                    // When an item is not found a CosmosException is thrown, but we want to
                    // return an empty collection so in this instance we catch and do not rethrow.
                    // Throw for any other exception.
                    if (exception.StatusCode == HttpStatusCode.NotFound)
                    {
                        break;
                    }

                    throw;
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Inserts or updates one or more items into the Cosmos DB container. 
        /// </summary>
        /// <param name="changes">A dictionary of items to be inserted or updated. The dictionary item key
        /// is used as the ID for the inserted / updated item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the changes dictionary is null.</exception>
        /// <exception cref="Exception">Exception thrown is the etag is empty on any of the items within the changes dictionary.</exception>
        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            if (changes.Count == 0)
            {
                return;
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            foreach (var change in changes)
            {
                var json = JObject.FromObject(change.Value, _jsonSerializer);

                // Remove etag from JSON object that was copied from IStoreItem.
                // The ETag information is updated as an _etag attribute in the document metadata.
                json.Remove("eTag");

                var documentChange = new DocumentStoreItem
                {
                    Id = CosmosDbKeyEscape.EscapeKey(change.Key, _cosmosDbStorageOptions.KeySuffix, _cosmosDbStorageOptions.CompatibilityMode),
                    RealId = change.Key,
                    Document = json,
                };

                var etag = (change.Value as IStoreItem)?.ETag;
                if (etag == null || etag == "*")
                {
                    // if new item or * then insert or replace unconditionally
                    await _container.UpsertItemAsync(
                            documentChange,
                            GetPartitionKey(documentChange.PartitionKey),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (etag.Length > 0)
                {
                    // if we have an etag, do opt. concurrency replace
                    await _container.UpsertItemAsync(
                            documentChange,
                            GetPartitionKey(documentChange.PartitionKey),
                            new ItemRequestOptions() { IfMatchEtag = etag, },
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException("etag empty");
                }
            }
        }

        /// <summary>
        /// Deletes one or more items from the Cosmos DB container.
        /// </summary>
        /// <param name="keys">An array of Ids for the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the array of Ids to be deleted is null.</exception>
        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            await InitializeAsync().ConfigureAwait(false);

            foreach (var key in keys)
            {
                var escapedKey = CosmosDbKeyEscape.EscapeKey(key, _cosmosDbStorageOptions.KeySuffix, _cosmosDbStorageOptions.CompatibilityMode);

                try
                {
                    await _container.DeleteItemAsync<DocumentStoreItem>(
                            partitionKey: GetPartitionKey(escapedKey),
                            id: escapedKey,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (CosmosException exception)
                {
                    // If we get a 404 status then the item we tried to delete was not found
                    // To maintain consistency with other storage providers, we ignore this and return.
                    // Any other exceptions are thrown.
                    if (exception.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Disposes the object instance and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes objects used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                _client?.Dispose();
            }

            _disposed = true;
        }

        private PartitionKey GetPartitionKey(string key)
        {
            if (_compatibilityModePartitionKey)
            {
                return PartitionKey.None;
            }

            return new PartitionKey(key);
        }

        /// <summary>
        /// Connects to the CosmosDB database and creates / gets the container.
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_container == null)
            {
                var cosmosClientOptions = _cosmosDbStorageOptions.CosmosClientOptions ?? new CosmosClientOptions();

                if (_client == null)
                {
                    _client = new CosmosClient(
                        _cosmosDbStorageOptions.CosmosDbEndpoint,
                        _cosmosDbStorageOptions.AuthKey,
                        cosmosClientOptions);
                }

                if (_container == null)
                {
                    if (!_cosmosDbStorageOptions.CompatibilityMode)
                    {
                        await CreateContainerIfNotExistsAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        try
                        {
                            _container = _client.GetContainer(_cosmosDbStorageOptions.DatabaseId, _cosmosDbStorageOptions.ContainerId);
                            
                            // This will throw if the container does not exist. 
                            var readContainer = await _container.ReadContainerAsync().ConfigureAwait(false);

                            // Containers created with CosmosDbStorage had no partition key set, so the default was '/_partitionKey'.
                            var partitionKeyPath = readContainer.Resource.PartitionKeyPath;
                            if (partitionKeyPath == "/_partitionKey")
                            {
                                _compatibilityModePartitionKey = true;
                            }
                            else if (partitionKeyPath != DocumentStoreItem.PartitionKeyPath)
                            {
                                // We are not supporting custom Partition Key Paths
                                throw new InvalidOperationException($"Custom Partition Key Paths are not supported. {_cosmosDbStorageOptions.ContainerId} has a custom Partition Key Path of {partitionKeyPath}.");
                            }
                        }
                        catch (CosmosException)
                        {
                            await CreateContainerIfNotExistsAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private async Task CreateContainerIfNotExistsAsync()
        {
            var containerResponse = await _client
                .GetDatabase(_cosmosDbStorageOptions.DatabaseId)
                .DefineContainer(_cosmosDbStorageOptions.ContainerId, DocumentStoreItem.PartitionKeyPath)
                .WithIndexingPolicy().WithAutomaticIndexing(false).WithIndexingMode(IndexingMode.None).Attach()
                .CreateIfNotExistsAsync(_cosmosDbStorageOptions.ContainerThroughput)
                .ConfigureAwait(false);

            _container = containerResponse.Container;
        }

        /// <summary>
        /// Internal data structure for storing items in a CosmosDB Collection.
        /// </summary>
        private class DocumentStoreItem : IStoreItem
        {
            /// <summary>
            /// Gets the PartitionKey path to be used for this document type.
            /// </summary>
            public static string PartitionKeyPath => "/id";

            /// <summary>
            /// Gets or sets the sanitized Id/Key used as PrimaryKey.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the un-sanitized Id/Key.
            /// </summary>
            [JsonProperty("realId")]
            public string RealId { get; internal set; }

            /// <summary>
            /// Gets or sets the persisted object.
            /// </summary>
            [JsonProperty("document")]
            public JObject Document { get; set; }

            /// <summary>
            /// Gets or sets the ETag information for handling optimistic concurrency updates.
            /// </summary>
            [JsonProperty("_etag")]
            public string ETag { get; set; }

            /// <summary>
            /// Gets the PartitionKey value for the document.
            /// </summary>
            public string PartitionKey => this.Id;
        }
    }
}
