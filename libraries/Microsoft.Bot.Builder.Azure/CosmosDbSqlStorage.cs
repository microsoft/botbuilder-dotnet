// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Models IStorage around CosmosDB SQL (DocumentDB)
    /// </summary>
    public class CosmosDbSqlStorage : IStorage
    {
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly Lazy<string> _collectionLink;
        private readonly DocumentClient _client;

        private static JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        });

        /// <summary>
        /// Initializes a new instance of <see cref="CosmosDbSqlStorage"/> class,
        /// using the provided CosmosDB credentials, DatabaseId and CollectionId.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint Uri for the service endpoint from the Azure Cosmos DB service.</param>
        /// <param name="authKey">The AuthKey used by the client from the Azure Cosmos DB service.</param>
        /// <param name="databaseId">The Database ID.</param>
        /// <param name="collectionId">The Collection ID.</param>
        public CosmosDbSqlStorage(Uri serviceEndpoint, string authKey, string databaseId, string collectionId, Action<ConnectionPolicy> connectionPolicyConfigurator = null)
        {
            if (serviceEndpoint == null)
            {
                throw new ArgumentNullException(nameof(serviceEndpoint), "Service EndPoint for CosmosDB is required.");
            }

            if (string.IsNullOrEmpty(authKey))
            {
                throw new ArgumentException("AuthKey for CosmosDB is required.", nameof(authKey));
            }

            if (string.IsNullOrEmpty(databaseId))
            {
                throw new ArgumentException("DatabaseId is required.", nameof(databaseId));
            }

            if (string.IsNullOrEmpty(collectionId))
            {
                throw new ArgumentException("CollectionId is required.", nameof(collectionId));
            }

            _databaseId = databaseId;
            _collectionId = collectionId;

            var version = GetType().Assembly.GetName().Version;
            var connectionPolicy = new ConnectionPolicy()
            {
                UserAgentSuffix = $"Microsoft-BotFramework {version}"
            };

            connectionPolicyConfigurator?.Invoke(connectionPolicy);
            _client = new DocumentClient(serviceEndpoint, authKey, connectionPolicy);

            _collectionLink = new Lazy<string>(EnsureCollectionExist);
        }

        /// <summary>
        /// Deletes the specified keys from storage.
        /// </summary>
        /// <param name="keys">List of keys to remove.</param>
        /// <returns></returns>
        public async Task Delete(params string[] keys)
        {
            // Ensure collection exists
            var collectionLink = _collectionLink.Value;

            var tasks = keys.Select(key =>
                _client.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(_databaseId, _collectionId, SanitizeKey(key))));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns an instance of <see cref="StoreItems"/> bag with the values for the specified  keys.
        /// </summary>
        /// <param name="keys">List of keys to retrieve.</param>
        /// <returns>A <see cref="StoreItems"/> instance.</returns>
        public async Task<StoreItems> Read(params string[] keys)
        {
            if (keys.Length == 0)
            {
                throw new ArgumentException("Please provide at least one key to read from storage", nameof(keys));
            }

            var collectionLink = _collectionLink.Value;
            var storeItems = new StoreItems();

            var parameterSequence = string.Join(",", Enumerable.Range(0, keys.Length).Select(i => $"@id{i}"));
            var parameterValues = keys.Select((key, ix) => new SqlParameter($"@id{ix}", SanitizeKey(key)));
            var querySpec = new SqlQuerySpec
            {
                QueryText = $"SELECT c.id, c.realId, c.document, c._etag FROM c WHERE c.id in ({parameterSequence})",
                Parameters = new SqlParameterCollection(parameterValues)
            };

            var query = _client.CreateDocumentQuery<DocumentStoreItem>(collectionLink, querySpec).AsDocumentQuery();
            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ExecuteNextAsync<DocumentStoreItem>())
                {
                    var item = doc.Document.ToObject(typeof(object), _jsonSerializer);
                    if (item is IStoreItem storeItem)
                    {
                        storeItem.eTag = doc.ETag;
                    }

                    // doc.Id cannot be used since it is escaped, read it from RealId property instead
                    storeItems[doc.ReadlId] = item;
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Persist the specified StoreItems into the CosmosDB Collection.
        /// </summary>
        /// <param name="changes">Items to persist.</param>
        /// <returns></returns>
        public async Task Write(StoreItems changes)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes), "Please provide a StoreItems with changes to persist.");
            }

            var collectionLink = _collectionLink.Value;
            foreach (var change in changes)
            {
                var json = JObject.FromObject(change.Value, _jsonSerializer);
                json.Remove("eTag");
                var documentChange = new DocumentStoreItem
                {
                    Id = SanitizeKey(change.Key),
                    ReadlId = change.Key,
                    Document = json
                };

                string eTag = (change.Value as IStoreItem)?.eTag;
                if (eTag == null || eTag == "*")
                {
                    // if new item or * then insert or replace unconditionaly
                    await _client.UpsertDocumentAsync(collectionLink, documentChange, disableAutomaticIdGeneration: true).ConfigureAwait(false);
                }
                else if (eTag.Length > 0)
                {
                    // if we have an etag, do opt. concurrency replace
                    var uri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentChange.Id);
                    var ac = new AccessCondition { Condition = eTag, Type = AccessConditionType.IfMatch };
                    await _client.ReplaceDocumentAsync(uri, documentChange, new RequestOptions { AccessCondition = ac }).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("etag empty");
                }
            }
        }

        private string EnsureCollectionExist()
        {
            _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId })
                .Wait();

            var task = _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_databaseId), new DocumentCollection { Id = _collectionId });
            task.Wait();

            return task.Result.Resource.SelfLink;
        }

        private static Lazy<Dictionary<char, string>> _badChars = new Lazy<Dictionary<char, string>>(() =>
        {
            char[] badChars = new char[] { '\\', '?', '/', '#', ' ' };
            var dict = new Dictionary<char, string>();
            foreach (var badChar in badChars)
                dict[badChar] = '*' + ((int)badChar).ToString("x2");
            return dict;
        });

        private static string SanitizeKey(string key)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in key)
            {
                if (_badChars.Value.TryGetValue(ch, out string val))
                    sb.Append(val);
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        protected class DocumentStoreItem
        {
            /// <summary>
            /// Sanitized Id/Key an used as PrimaryKey
            /// </summary>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// Un-sanitized Id/Key
            /// </summary>
            [JsonProperty("realId")]
            public string ReadlId { get; internal set; }

            [JsonProperty("document")]
            public JObject Document { get; set; }

            [JsonProperty("_etag")]
            public string ETag { get; set; }
        }
    }
}
