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
    /// Middleware that implements an Azure Table based storage provider for a bot.
    /// </summary>
    public class AzureTableStorage : IStorage
    {
        /// <summary>
        /// Map of already initialized tables.
        /// </summary>
        private static HashSet<string> _checkedTables = new HashSet<string>();

        /// <summary>
        /// Underlying Azure Table.
        /// </summary>
        public CloudTable Table { get; private set; }

        /// <summary>
        /// Creates a new instance of the storage provider.
        /// </summary>
        /// <param name="dataConnectionString">The Azure Storage Connection string</param>
        /// <param name="tableName">Name of the table to use for storage. Check table name rules: https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model?redirectedfrom=MSDN#table-names </param>
        public AzureTableStorage(string dataConnectionString, string tableName)
            : this(CloudStorageAccount.Parse(dataConnectionString), tableName)
        {
        }

        /// <summary>
        /// Creates a new instance of the storage provider.
        /// </summary>
        /// <param name="storageAccount">CloudStorageAccount information.</param>
        /// <param name="tableName">Name of the table to use for storage. Check table name rules: https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model?redirectedfrom=MSDN#table-names </param>
        public AzureTableStorage(CloudStorageAccount storageAccount, string tableName)
        {
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));

            // Checks if table name is valid
            NameValidator.ValidateTableName(tableName);

            var tableClient = storageAccount.CreateCloudTableClient();
            Table = tableClient.GetTableReference(tableName);

            if (_checkedTables.Add($"{storageAccount.TableStorageUri.PrimaryUri.Host}-{tableName}"))
                Table.CreateIfNotExistsAsync().Wait();
        }

        /// <summary>
        /// Removes store items from storage.
        /// </summary>
        /// <param name="keys">Array of item keys to remove from the store.</param>
        public async Task Delete(string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys.Select(k => GetEntityKey(k)))
            {
                await Table.ExecuteAsync(TableOperation.Delete(new TableEntity(key.PartitionKey, key.RowKey) { ETag = "*" })).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads store items from storage.
        /// </summary>
        /// <param name="keys">Array of item keys to read from the store.</param>
        public async Task<StoreItems> Read(string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Please provide at least one key to read from storage.", nameof(keys));
            }

            var storeItems = new StoreItems();
            foreach (string key in keys)
            {
                var entityKey = GetEntityKey(key);
                var result = await Table.ExecuteAsync(TableOperation.Retrieve<StoreItemEntity>(entityKey.PartitionKey, entityKey.RowKey)).ConfigureAwait(false);
                if ((HttpStatusCode)result.HttpStatusCode == HttpStatusCode.OK)
                {
                    var value = ((StoreItemEntity)result.Result).AsObject();
                    var valueStoreItem = value as IStoreItem;
                    if (valueStoreItem != null)
                    {
                        valueStoreItem.eTag = result.Etag;
                    }
                    storeItems.Add(new KeyValuePair<string, object>(key, value));
                }
            }
            return storeItems;
        }

        /// <summary>
        /// Saves store items to storage.
        /// </summary>
        /// <param name="changes">Map of items to write to storage.</param>
        /// <returns></returns>
        public async Task Write(StoreItems changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            foreach (var change in changes)
            {
                var entityKey = GetEntityKey(change.Key);
                var newValue = change.Value;
                StoreItemEntity entity = new StoreItemEntity(entityKey, newValue);
                if (entity.ETag == null || entity.ETag == "*")
                {
                    var result = await Table.ExecuteAsync(TableOperation.InsertOrReplace(entity)).ConfigureAwait(false);
                }
                else if (entity.ETag.Length > 0)
                {
                    var result = await Table.ExecuteAsync(TableOperation.Replace(entity)).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception("etag empty");
                }
            }
        }

        /// <summary>
        /// Maps the property key into a PartitionKey and RowKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private EntityKey GetEntityKey(string key)
        {
            return new EntityKey() { PartitionKey = SanitizeKey(key), RowKey = "0" };
        }

        /// <summary>
        /// Internal data structure for storing items in Azure Tables.
        /// </summary>
        private class StoreItemEntity : TableEntity
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
                ETag = (obj as IStoreItem)?.eTag;
                Json = JsonConvert.SerializeObject(obj, Formatting.None, serializationSettings);
            }

            public string Json { get; set; }

            public object AsObject()
            {
                var obj = JsonConvert.DeserializeObject(Json, serializationSettings);
                IStoreItem storeItem = obj as IStoreItem;
                if (storeItem != null)
                    storeItem.eTag = ETag;
                return obj;
            }
        }

        /// <summary>
        /// Entity that maps property to PartitionKey and RowKey
        /// </summary>
        private class EntityKey
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

        /// <summary>
        /// Escapes a property key into a PartitionKey that can be used with Azure Tables.
        /// More information at https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model?redirectedfrom=MSDN#table-names
        /// </summary>
        /// <param name="key">The Property Key</param>
        /// <returns>Sanitized key that can be used as PartitionKey</returns>
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
