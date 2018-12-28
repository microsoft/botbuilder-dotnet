// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Implements an CosmosDB based storage provider for a bot.
    /// </summary>
    public class CosmosDbStorage : IStorage
    {
        private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        // When setting up the database, calls are made to CosmosDB. If multiple calls are made, we'll end up setting the
        // collectionLink member variable more than once. The semaphore is for making sure the initialization of the
        // database is done only once.
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly RequestOptions _documentCollectionCreationRequestOptions = null;
        private readonly RequestOptions _databaseCreationRequestOptions = null;
        private readonly IDocumentClient _client;
        private string _collectionLink = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorage"/> class.
        /// using the provided CosmosDB credentials, database ID, and collection ID.
        /// </summary>
        /// <param name="cosmosDbStorageOptions">Cosmos DB storage configuration options.</param>
        public CosmosDbStorage(CosmosDbStorageOptions cosmosDbStorageOptions)
        {
            if (cosmosDbStorageOptions == null)
            {
                throw new ArgumentNullException(nameof(cosmosDbStorageOptions));
            }

            if (cosmosDbStorageOptions.CosmosDBEndpoint == null)
            {
                throw new ArgumentNullException(nameof(cosmosDbStorageOptions.CosmosDBEndpoint), "Service EndPoint for CosmosDB is required.");
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.AuthKey))
            {
                throw new ArgumentException("AuthKey for CosmosDB is required.", nameof(cosmosDbStorageOptions.AuthKey));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.DatabaseId))
            {
                throw new ArgumentException("DatabaseId is required.", nameof(cosmosDbStorageOptions.DatabaseId));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.CollectionId))
            {
                throw new ArgumentException("CollectionId is required.", nameof(cosmosDbStorageOptions.CollectionId));
            }

            _databaseId = cosmosDbStorageOptions.DatabaseId;
            _collectionId = cosmosDbStorageOptions.CollectionId;
            _documentCollectionCreationRequestOptions = cosmosDbStorageOptions.DocumentCollectionRequestOptions;
            _databaseCreationRequestOptions = cosmosDbStorageOptions.DatabaseCreationRequestOptions;

            // Inject BotBuilder version to CosmosDB Requests
            var version = GetType().Assembly.GetName().Version;
            var connectionPolicy = new ConnectionPolicy { UserAgentSuffix = $"Microsoft-BotFramework {version}" };

            // Invoke CollectionPolicy delegate to further customize settings
            cosmosDbStorageOptions.ConnectionPolicyConfigurator?.Invoke(connectionPolicy);
            _client = new DocumentClient(cosmosDbStorageOptions.CosmosDBEndpoint, cosmosDbStorageOptions.AuthKey, connectionPolicy);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorage"/> class.
        /// This constructor should only be used if the default behavior of the DocumentClient needs to be changed.
        /// The <see cref="CosmosDbStorage(CosmosDbStorageOptions)"/> constructor is preferer for most cases.
        /// </summary>
        /// <param name="documentClient">The custom implementation of IDocumentClient.</param>
        /// <param name="cosmosDbCustomClientOptions">Custom client configuration options.</param>
        public CosmosDbStorage(IDocumentClient documentClient, CosmosDbCustomClientOptions cosmosDbCustomClientOptions)
        {
            if (cosmosDbCustomClientOptions == null)
            {
                throw new ArgumentNullException(nameof(cosmosDbCustomClientOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbCustomClientOptions.DatabaseId))
            {
                throw new ArgumentException("DatabaseId is required.", nameof(cosmosDbCustomClientOptions.DatabaseId));
            }

            if (string.IsNullOrEmpty(cosmosDbCustomClientOptions.CollectionId))
            {
                throw new ArgumentException("CollectionId is required.", nameof(cosmosDbCustomClientOptions.CollectionId));
            }

            _client = documentClient ?? throw new ArgumentNullException(nameof(documentClient), "An implementation of IDocumentClient for CosmosDB is required.");
            _databaseId = cosmosDbCustomClientOptions.DatabaseId;
            _collectionId = cosmosDbCustomClientOptions.CollectionId;
            _documentCollectionCreationRequestOptions = cosmosDbCustomClientOptions.DocumentCollectionRequestOptions;
            _databaseCreationRequestOptions = cosmosDbCustomClientOptions.DatabaseCreationRequestOptions;

            // Inject BotBuilder version to CosmosDB Requests
            var version = GetType().Assembly.GetName().Version;
            _client.ConnectionPolicy.UserAgentSuffix = $"Microsoft-BotFramework {version}";
        }

        [Obsolete("Replaced by CosmosDBKeyEscape.EscapeKey.")]
        public static string SanitizeKey(string key) => CosmosDbKeyEscape.EscapeKey(key);

        /// <summary>
        /// Deletes storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to remove from the store.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null || keys.Length == 0)
            {
                return;
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            // Parallelize deletion
            var tasks = keys.Select(key =>
                _client.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(_databaseId, _collectionId, CosmosDbKeyEscape.EscapeKey(key)),
                    cancellationToken: cancellationToken));

            // await to deletion tasks to complete
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads storage items from storage.
        /// </summary>
        /// <param name="keys">keys of the <see cref="IStoreItem"/> objects to read from the store.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the activities are successfully sent, the task result contains
        /// the items read, indexed by key.</remarks>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken)
        {
            if (keys == null || keys.Length == 0)
            {
                // No keys passed in, no result to return.
                return new Dictionary<string, object>();
            }

            // Ensure Initialization has been run
            await InitializeAsync().ConfigureAwait(false);

            var storeItems = new Dictionary<string, object>(keys.Length);

            var parameterSequence = string.Join(",", Enumerable.Range(0, keys.Length).Select(i => $"@id{i}"));
            var parameterValues = keys.Select((key, ix) => new SqlParameter($"@id{ix}", CosmosDbKeyEscape.EscapeKey(key)));
            var querySpec = new SqlQuerySpec
            {
                QueryText = $"SELECT c.id, c.realId, c.document, c._etag FROM c WHERE c.id in ({parameterSequence})",
                Parameters = new SqlParameterCollection(parameterValues),
            };

            var query = _client.CreateDocumentQuery<DocumentStoreItem>(_collectionLink, querySpec).AsDocumentQuery();
            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ExecuteNextAsync<DocumentStoreItem>(cancellationToken).ConfigureAwait(false))
                {
                    var item = doc.Document.ToObject(typeof(object), _jsonSerializer);
                    if (item is IStoreItem storeItem)
                    {
                        storeItem.ETag = doc.ETag;
                    }

                    // doc.Id cannot be used since it is escaped, read it from RealId property instead
                    storeItems.Add(doc.ReadlId, item);
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Writes storage items to storage.
        /// </summary>
        /// <param name="changes">The items to write to storage, indexed by key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="DeleteAsync(string[], CancellationToken)"/>
        /// <seealso cref="ReadAsync(string[], CancellationToken)"/>
        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken)
        {
            if (changes == null || changes.Count == 0)
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
                    ReadlId = change.Key,
                    Document = json,
                };

                var etag = (change.Value as IStoreItem)?.ETag;
                if (etag == null || etag == "*")
                {
                    // if new item or * then insert or replace unconditionaly
                    await _client.UpsertDocumentAsync(
                        _collectionLink,
                        documentChange,
                        disableAutomaticIdGeneration: true,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else if (etag.Length > 0)
                {
                    // if we have an etag, do opt. concurrency replace
                    var uri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentChange.Id);
                    var ac = new AccessCondition { Condition = etag, Type = AccessConditionType.IfMatch };
                    await _client.ReplaceDocumentAsync(
                        uri,
                        documentChange,
                        new RequestOptions { AccessCondition = ac },
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("etag empty");
                }
            }
        }

        /// <summary>
        /// Creates the CosmosDB Database and populates the _collectionLink member variable.
        /// </summary>
        /// <remarks>
        /// This method is idempotent, and thread safe.
        /// </remarks>
        private async Task InitializeAsync()
        {
            // In the steady-state case, we'll already have a connection string to the
            // database setup. If so, no need to enter locks or call CosmosDB to get one.
            if (_collectionLink == null)
            {
                // We don't (probably) have a database link yet. Enter the lock,
                // then check again (aka: Double-Check Lock pattern).
                await _semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (_collectionLink == null)
                    {
                        // We don't have a database link. Create one. Note that we're inside a semaphore at this point
                        // so other threads may be blocked on us.
                        await _client.CreateDatabaseIfNotExistsAsync(
                            new Database { Id = _databaseId },
                            _databaseCreationRequestOptions) // pass in any user set database creation flags
                            .ConfigureAwait(false);

                        var documentCollection = new DocumentCollection
                        {
                            Id = _collectionId,
                        };

                        var response = await _client.CreateDocumentCollectionIfNotExistsAsync(
                            UriFactory.CreateDatabaseUri(_databaseId),
                            documentCollection,
                            _documentCollectionCreationRequestOptions) // pass in any user set collection creation flags
                            .ConfigureAwait(false);

                        _collectionLink = response.Resource.SelfLink;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        /// <summary>
        /// Internal data structure for storing items in a CosmosDB Collection.
        /// </summary>
        private class DocumentStoreItem
        {
            /// <summary>
            /// Gets or sets the sanitized Id/Key used as PrimaryKey.
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the un-sanitized Id/Key.
            /// </summary>
            [JsonProperty("realId")]
            public string ReadlId { get; internal set; }

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
        }
    }
}
