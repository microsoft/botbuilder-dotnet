// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Azure
{
    public class CosmosDbSqlStorage : IStorage
    {
        private readonly string databaseId;
        private readonly string collectionId;
        private readonly DocumentClient client;

        private static readonly ConnectionPolicy connectionPolicy = new ConnectionPolicy
        {
            UserAgentSuffix = "Microsoft-BotFramework 4.0.0"
        };

        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public CosmosDbSqlStorage(Uri serviceEndpoint, string authKey, string databaseId, string collectionId)
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

            this.databaseId = databaseId;
            this.collectionId = collectionId;

            this.client = new DocumentClient(serviceEndpoint, authKey, connectionPolicy);
            this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId })
                .Wait();

            // TODO: Should specify OfferThroughput in RequestOptions?
            this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection { Id = collectionId })
                .Wait();
        }

        public async Task Delete(params string[] keys)
        {
            var tasks = keys.Select(key =>
                this.client.DeleteDocumentAsync(
                    UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, SanitizeKey(key))));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task<StoreItems> Read(params string[] keys)
        {
            var storeItems = new StoreItems();
            foreach (string key in keys)
            {
                try
                {
                    var uri = UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, SanitizeKey(key));
                    var response = (await this.client.ReadDocumentAsync<DocumentStoreItem>(uri).ConfigureAwait(false));
                    var doc = response.Document;
                    var item = doc.Item.ToObject(typeof(object), JsonSerializer.Create(serializationSettings));
                    if (item is IStoreItem storeItem)
                    {
                        storeItem.eTag = response.ResponseHeaders["etag"];
                    }

                    storeItems[key] = item;
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
            }

            return storeItems;
        }

        public async Task Write(StoreItems changes)
        {
            foreach (var change in changes)
            {
                var documentChange = new DocumentStoreItem
                {
                    Id = SanitizeKey(change.Key),
                    Item = JObject.FromObject(change.Value, JsonSerializer.Create(serializationSettings))
                };


                string eTag = (change.Value as IStoreItem)?.eTag;
                if (eTag == null || eTag == "*")
                {
                    var uri = UriFactory.CreateDocumentCollectionUri(this.databaseId, this.collectionId);
                    await this.client.UpsertDocumentAsync(uri, documentChange, disableAutomaticIdGeneration: true).ConfigureAwait(false);
                }
                else if (eTag.Length > 0)
                {
                    // Optimistic Update
                    var uri = UriFactory.CreateDocumentUri(this.databaseId, this.collectionId, documentChange.Id);
                    var ac = new AccessCondition { Condition = eTag, Type = AccessConditionType.IfMatch };
                    await this.client.ReplaceDocumentAsync(uri, documentChange, new RequestOptions { AccessCondition = ac }).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("etag empty");
                }
            }
        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
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
                if (badChars.Value.TryGetValue(ch, out string val))
                    sb.Append(val);
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        protected class DocumentStoreItem
        {

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("item")]
            public JObject Item { get; set; }
        }
    }
}
