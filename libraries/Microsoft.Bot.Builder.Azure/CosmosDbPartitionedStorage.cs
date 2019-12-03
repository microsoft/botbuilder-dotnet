﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    public class CosmosDbPartitionedStorage : IStorage
    {
        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        private Container _container;
        private readonly CosmosDbPartitionedStorageOptions _cosmosDbStorageOptions;
        private CosmosClient _client;

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
                throw new ArgumentNullException(nameof(cosmosDbStorageOptions.CosmosDbEndpoint), "Service EndPoint for CosmosDB is required.");
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.AuthKey))
            {
                throw new ArgumentException("AuthKey for CosmosDB is required.", nameof(cosmosDbStorageOptions.AuthKey));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.DatabaseId))
            {
                throw new ArgumentException("DatabaseId is required.", nameof(cosmosDbStorageOptions.DatabaseId));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.ContainerId))
            {
                throw new ArgumentException("ContainerId is required.", nameof(cosmosDbStorageOptions.ContainerId));
            }

            if (!string.IsNullOrWhiteSpace(cosmosDbStorageOptions.KeySuffix))
            {
                if (cosmosDbStorageOptions.CompatibilityMode)
                {
                    throw new ArgumentException($"CompatibilityMode cannot be 'true' while using a KeySuffix.", nameof(cosmosDbStorageOptions.CompatibilityMode));
                }

                // In order to reduce key complexity, we do not allow invalid characters in a KeySuffix
                // If the KeySuffix has invalid characters, the EscapeKey will not match
                var suffixEscaped = CosmosDbKeyEscape.EscapeKey(cosmosDbStorageOptions.KeySuffix);
                if (!cosmosDbStorageOptions.KeySuffix.Equals(suffixEscaped, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Cannot use invalid Row Key characters: {cosmosDbStorageOptions.KeySuffix}", nameof(cosmosDbStorageOptions.KeySuffix));
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
                    throw new Exception("etag empty");
                }
            }
        }

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

        private PartitionKey GetPartitionKey(string key)
        {
            if (_cosmosDbStorageOptions.CompatibilityMode)
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
                    if (_cosmosDbStorageOptions.CompatibilityMode)
                    {
                        // This will throw if the container or db does not exist, which is what we
                        // want for CompatibilityMode. (It is expected that users are utilizing CompatibilityMode
                        // for legacy containers, and not for creating new containers.)
                        _container = _client.GetContainer(_cosmosDbStorageOptions.DatabaseId, _cosmosDbStorageOptions.ContainerId);
                        var readContainer = await _container.ReadContainerAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        var containerResponse = await _client
                            .GetDatabase(_cosmosDbStorageOptions.DatabaseId)
                            .DefineContainer(_cosmosDbStorageOptions.ContainerId, DocumentStoreItem.PartitionKeyPath)
                            .WithIndexingPolicy().WithAutomaticIndexing(false).WithIndexingMode(IndexingMode.None).Attach()
                            .CreateIfNotExistsAsync(_cosmosDbStorageOptions.ContainerThroughput)
                            .ConfigureAwait(false);

                        _container = containerResponse.Container;
                    }
                }
            }
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
