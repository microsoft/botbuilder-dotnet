// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
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

            var resultSetIterator = _container.GetItemQueryIterator<DocumentStoreItem>(
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(keys[0]),
                });

            var documentStoreItems = new List<DocumentStoreItem>(keys.Length);
            while (resultSetIterator.HasMoreResults)
            {
                documentStoreItems.AddRange(await resultSetIterator.ReadNextAsync(cancellationToken).ConfigureAwait(false));
            }

            var storeItems = new Dictionary<string, object>(keys.Length);

            foreach (var documentStoreItem in documentStoreItems)
            {
                if (documentStoreItem is IStoreItem storeItem)
                {
                    storeItem.ETag = documentStoreItem.ETag;
                    storeItems.Add(documentStoreItem.RealId, storeItem);
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
                    Id = CosmosDbKeyEscape.EscapeKey(change.Key),
                    RealId = change.Key,
                    Document = json,
                };

                var etag = (change.Value as IStoreItem)?.ETag;
                if (etag == null || etag == "*")
                {
                    // if new item or * then insert or replace unconditionally
                    await _container.UpsertItemAsync(
                            documentChange,
                            new PartitionKey(documentChange.PartitionKey),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                else if (etag.Length > 0)
                {
                    // if we have an etag, do opt. concurrency replace
                    await _container.UpsertItemAsync(
                            documentChange,
                            new PartitionKey(documentChange.PartitionKey),
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
            foreach (var key in keys)
            {
                await _container.DeleteItemAsync<DocumentStoreItem>(
                    partitionKey: new PartitionKey(key),
                    id: CosmosDbKeyEscape.EscapeKey(key),
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Connects to the CosmosDB database and creates / gets the container.
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_container == null)
            {
                var cosmosClientOptions = _cosmosDbStorageOptions.CosmosClientOptions ?? new CosmosClientOptions();

                _client = new CosmosClient(
                    _cosmosDbStorageOptions.CosmosDbEndpoint,
                    _cosmosDbStorageOptions.AuthKey,
                    cosmosClientOptions);

                _container = await _client
                        .GetDatabase(_cosmosDbStorageOptions.DatabaseId)
                        .CreateContainerIfNotExistsAsync(
                            _cosmosDbStorageOptions.ContainerId,
                            DocumentStoreItem.PartitionKeyPath,
                            _cosmosDbStorageOptions.ContainerThroughput)
                        .ConfigureAwait(false);
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
            public static string PartitionKeyPath => "/realId";

            /// <summary>
            /// Gets or sets the sanitized Id/Key used as PrimaryKey.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the un-sanitized Id/Key.
            /// </summary>
            /// <remarks>
            /// Note: There is a Typo in the property name ("RealId"), that can't be changed due to compatability concerns. The
            /// Json is correct due to the JsonProperty field, but the Typo needs to stay.
            /// </remarks>
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
            public string PartitionKey => this.RealId;
        }
    }
}
