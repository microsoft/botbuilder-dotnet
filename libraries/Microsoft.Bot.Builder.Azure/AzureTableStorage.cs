// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class AzureTableStorage : IStorage
    {
        private static HashSet<string> _checkedTables = new HashSet<string>();

        public CloudTable Table { get; private set; }

        public AzureTableStorage(string dataConnectionString, string tableName)
            : this(CloudStorageAccount.Parse(dataConnectionString), tableName)
        {
        }

        public AzureTableStorage(CloudStorageAccount storageAccount, string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            this.Table = tableClient.GetTableReference(tableName);

            if (_checkedTables.Add($"{storageAccount.TableStorageUri.PrimaryUri.Host}-{tableName}"))
                this.Table.CreateIfNotExistsAsync().Wait();
        }

        protected EntityKey GetEntityKey(string key)
        {
            return new EntityKey() { PartitionKey = SanitizeKey(key), RowKey = "0" };
        }

        public async Task Delete(string[] keys)
        {
            foreach (var key in keys.Select(k => GetEntityKey(k)))
            {
                await this.Table.ExecuteAsync(TableOperation.Delete(new TableEntity(key.PartitionKey, key.RowKey) { ETag = "*" })).ConfigureAwait(false);
            }
        }

        public async Task<StoreItems> Read(string[] keys)
        {
            var storeItems = new StoreItems();
            foreach (string key in keys)
            {
                var entityKey = GetEntityKey(key);
                var result = await this.Table.ExecuteAsync(TableOperation.Retrieve<StoreItemEntity>(entityKey.PartitionKey, entityKey.RowKey)).ConfigureAwait(false);
                if ((HttpStatusCode)result.HttpStatusCode == HttpStatusCode.OK)
                {
                    var value = ((StoreItemEntity)result.Result).AsObject();
                    IStoreItem valueStoreItem = value as IStoreItem;
                    if (valueStoreItem != null)
                        valueStoreItem.eTag = result.Etag;
                    storeItems[key] = value;
                }
            }
            return storeItems;
        }


        public async Task Write(StoreItems changes)
        {
            foreach (var change in changes)
            {
                var entityKey = GetEntityKey(change.Key);
                var newValue = change.Value;
                StoreItemEntity entity = new StoreItemEntity(entityKey, newValue);
                if (entity.ETag == null || entity.ETag == "*")
                {
                    var result = await this.Table.ExecuteAsync(TableOperation.InsertOrReplace(entity)).ConfigureAwait(false);
                }
                else if (entity.ETag.Length > 0)
                {
                    var result = await this.Table.ExecuteAsync(TableOperation.Replace(entity)).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("etag empty");
                }
            }
        }

        protected class StoreItemEntity : TableEntity
        {
            private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
            {
                // we use all so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
                TypeNameHandling = TypeNameHandling.All
            };

            public StoreItemEntity() { }

            public StoreItemEntity(EntityKey key, object obj)
                : this(key.PartitionKey, key.RowKey, obj)
            { }

            public StoreItemEntity(string partitionKey, string rowKey, object obj)
                : base(partitionKey, rowKey)
            {
                this.ETag = (obj as IStoreItem)?.eTag;
                this.Json = JsonConvert.SerializeObject(obj, Formatting.None, serializationSettings);
            }

            public string Json { get; set; }

            public object AsObject()
            {
                var obj = JsonConvert.DeserializeObject(Json, serializationSettings);
                IStoreItem storeItem = obj as IStoreItem;
                if (storeItem != null)
                    storeItem.eTag = this.ETag;
                return obj;
            }
        }

        protected class EntityKey
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }

            public override string ToString() { return $"{PartitionKey}~{RowKey}"; }

            public static EntityKey FromString(string key)
            {
                var parts = key.Split('~');
                return new EntityKey() { PartitionKey = parts[0], RowKey = parts[1] };
            }
        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
        {
            char[] badChars = new char[] { '\\', '?', '/', '#', '\t', '\n', '\r' };
            var dict = new Dictionary<char, string>();
            foreach (var badChar in badChars)
                dict[badChar] = '%' + ((int)badChar).ToString("x2");
            return dict;
        });

        private string SanitizeKey(string key)
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
    }
}
