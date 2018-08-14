// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static readonly char[] IllegalKeyCharacters = new char[] { '\\', '?', '/', '#', ' ' };
        private static readonly Lazy<Dictionary<char, string>> IllegalKeyCharacterReplacementMap = new Lazy<Dictionary<char, string>>(() => IllegalKeyCharacters.ToDictionary(c => c, c => '*' + ((int)c).ToString("x2")));

        private static JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly DocumentClient _client;
        private string _collectionLink = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbStorage"/> class.
        /// using the provided CosmosDB credentials, database ID, and collection ID.
        /// </summary>
        /// <param name="cosmosDbStorageOptions">Cosmos DB storage configuration options.</param>
        public CosmosDbStorage(CosmosDbStorageOptions cosmosDbStorageOptions)
        {
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

            // Inject BotBuilder version to CosmosDB Requests
            var version = GetType().Assembly.GetName().Version;
            var connectionPolicy = new ConnectionPolicy { UserAgentSuffix = $"Microsoft-BotFramework {version}" };

            // Invoke CollectionPolicy delegate to further customize settings
            cosmosDbStorageOptions.ConnectionPolicyConfigurator?.Invoke(connectionPolicy);
            _client = new DocumentClient(cosmosDbStorageOptions.CosmosDBEndpoint, cosmosDbStorageOptions.AuthKey, connectionPolicy);
        }

        /// <summary>
        /// Converts the key into a DocumentID that can be used safely with CosmosDB.
        /// The following characters are restricted and cannot be used in the Id property: '/', '\', '?', '#'
        /// More information at https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.resource.id?view=azure-dotnet#remarks.
        /// </summary>
        /// <param name="key">The key to sanitize.</param>
        /// <returns>A sanitized key that can be used safely with CosmosDB.</returns>
        public static string SanitizeKey(string key)
        {
            var firstIllegalCharIndex = key.IndexOfAny(IllegalKeyCharacters);

            // If there are no illegal characters return immediately and avoid any further processing/allocations
            if (firstIllegalCharIndex == -1)
            {
                return key;
            }

            // Allocate a builder that assumes that all remaining characters might be replaced to avoid any extra allocations
            var sanitizedKeyBuilder = new StringBuilder(key.Length + ((key.Length - firstIllegalCharIndex + 1) * 3));

            // Add all good characters up to the first bad character to the builder first
            for (var index = 0; index < firstIllegalCharIndex; index++)
            {
                sanitizedKeyBuilder.Append(key[index]);
            }

            var illegalCharacterReplacementMap = IllegalKeyCharacterReplacementMap.Value;

            // Now walk the remaining characters, starting at the first known bad character, replacing any bad ones with their designated replacement value from the map
            for (var index = firstIllegalCharIndex; index < key.Length; index++)
            {
                var ch = key[index];

                // Check if this next character is considered illegal and, if so, append its replacement; otherwise just append the good character as is
                if (illegalCharacterReplacementMap.TryGetValue(ch, out var replacement))
                {
                    sanitizedKeyBuilder.Append(replacement);
                }
                else
                {
                    sanitizedKeyBuilder.Append(ch);
                }
            }

            return sanitizedKeyBuilder.ToString();
        }

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

            // Ensure collection exists
            var collectionLink = await GetCollectionLink();

            // Parallelize deletion
            var tasks = keys.Select(key =>
                _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, SanitizeKey(key))));

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
                throw new ArgumentException("Please provide at least one key to read from storage", nameof(keys));
            }

            var storeItems = new Dictionary<string, object>(keys.Length);

            // Ensure collection exists
            var collectionLink = await GetCollectionLink();

            var parameterSequence = string.Join(",", Enumerable.Range(0, keys.Length).Select(i => $"@id{i}"));
            var parameterValues = keys.Select((key, ix) => new SqlParameter($"@id{ix}", SanitizeKey(key)));
            var querySpec = new SqlQuerySpec
            {
                QueryText = $"SELECT c.id, c.realId, c.document, c._etag FROM c WHERE c.id in ({parameterSequence})",
                Parameters = new SqlParameterCollection(parameterValues),
            };

            var query = _client.CreateDocumentQuery<DocumentStoreItem>(collectionLink, querySpec).AsDocumentQuery();
            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ExecuteNextAsync<DocumentStoreItem>().ConfigureAwait(false))
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
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes), "Please provide a StoreItems with changes to persist.");
            }

            var collectionLink = await GetCollectionLink();
            foreach (var change in changes)
            {
                var json = JObject.FromObject(change.Value, _jsonSerializer);

                // Remove etag from JSON object that was copied from IStoreItem.
                // The ETag information is updated as an _etag attribute in the document metadata.
                json.Remove("eTag");

                var documentChange = new DocumentStoreItem
                {
                    Id = SanitizeKey(change.Key),
                    ReadlId = change.Key,
                    Document = json,
                };

                var etag = (change.Value as IStoreItem)?.ETag;
                if (etag == null || etag == "*")
                {
                    // if new item or * then insert or replace unconditionaly
                    await _client.UpsertDocumentAsync(collectionLink, documentChange, disableAutomaticIdGeneration: true).ConfigureAwait(false);
                }
                else if (etag.Length > 0)
                {
                    // if we have an etag, do opt. concurrency replace
                    var uri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentChange.Id);
                    var ac = new AccessCondition { Condition = etag, Type = AccessConditionType.IfMatch };
                    await _client.ReplaceDocumentAsync(uri, documentChange, new RequestOptions { AccessCondition = ac }).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("etag empty");
                }
            }
        }

        /// <summary>
        /// Delayed Database and Collection creation if they do not exist.
        /// </summary>
        private async ValueTask<string> GetCollectionLink()
        {
            if (_collectionLink == null)
            {
                await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId }).ConfigureAwait(false);

                var response = await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_databaseId), new DocumentCollection { Id = _collectionId }).ConfigureAwait(false);
                _collectionLink = response.Resource.SelfLink;
            }

            return _collectionLink;
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
