// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
            Table = tableClient.GetTableReference(tableName);

            if (_checkedTables.Add($"{storageAccount.TableStorageUri.PrimaryUri.Host}-{tableName}"))
                Table.CreateIfNotExistsAsync().Wait();
        }

        public async Task Delete(string[] keys)
        {
            foreach (var key in keys)
            {
                // Retrieve all RowKeys for this PartitionKey
                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SanitizeKey(key));
                var query = new TableQuery<DynamicTableEntity>()
                {
                    SelectColumns = new List<string>() { "RowKey" },
                    FilterString = filter
                };

                var rowKeys = await ExecuteQueryAsync(Table, query);

                // Delete rows
                var tasks = rowKeys.ToList().Select(async e =>
                {
                    await Table.ExecuteAsync(TableOperation.Delete(new TableEntity(e.PartitionKey, e.RowKey) { ETag = "*" })).ConfigureAwait(false);
                });

                await Task.WhenAll(tasks);
            }
        }

        public async Task<StoreItems> Read(string[] keys)
        {
            var storeItems = new StoreItems();
            foreach (string key in keys)
            {
                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SanitizeKey(key));
                var query = new TableQuery<StoreItemEntity>().Where(filter);
                var results = await ExecuteQueryAsync(Table, query);
                if (results.Any())
                {
                    var value = StoreItemContainer.Join(results).Object;
                    storeItems[key] = value;
                }
            }

            return storeItems;
        }

        public async Task Write(StoreItems changes)
        {
            // Split entity into smaller chunks that fit within column max size and update (1)
            // Then proceed to delete any remaining chunks from a previous version (2)

            var storeItems = AsStoreItemContainers(changes);
            var writeTasks = storeItems.Select(async entity =>
            {
                // (1)
                var chunks = entity.Split();
                foreach (var chunk in chunks)
                {
                    // When replacing with optimistic update, only check for the first chunk and replace the others directly
                    if (entity.ETag == null || entity.ETag == "*" || chunk.RowKey != "0")
                    {
                        await Table.ExecuteAsync(TableOperation.InsertOrReplace(chunk)).ConfigureAwait(false);
                    }
                    else if (entity.ETag.Length > 0)
                    {
                        // Optimistic Update (first chunk only)
                        await Table.ExecuteAsync(TableOperation.Replace(chunk)).ConfigureAwait(false);
                    }
                }

                // (2) Delete any remaining chunks from a previous obj version
                var maxRowKey = chunks.Count();
                var filter = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SanitizeKey(entity.Key)),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, maxRowKey.ToString()));
                var query = new TableQuery<StoreItemEntity>().Where(filter);

                var rowKeys = await ExecuteQueryAsync(Table, query);

                // Retrieve and delete each row
                var deleleTasks = rowKeys.ToList().Select(e =>
                    Table.ExecuteAsync(TableOperation.Delete(new TableEntity(e.PartitionKey, e.RowKey) { ETag = "*" })));

                await Task.WhenAll(deleleTasks).ConfigureAwait(false);
            });

            await Task.WhenAll(writeTasks).ConfigureAwait(false);
        }

        private static IEnumerable<StoreItemContainer> AsStoreItemContainers(StoreItems changes)
        {
            foreach (var change in changes)
            {
                var entity = new StoreItemContainer(change.Key, change.Value);
                if (entity.ETag != null && entity.ETag != "*" && entity.ETag.Length == 0)
                {
                    throw new ArgumentException($"Etag for {change.Key} is empty.");
                }

                yield return entity;
            }
        }

        private static async Task<IEnumerable<T>> ExecuteQueryAsync<T>(CloudTable table, TableQuery<T> query, CancellationToken ct = default(CancellationToken)) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
        {
            char[] badChars = new char[] { '\\', '?', '/', '#', '\t', '\n', '\r' };
            var dict = new Dictionary<char, string>();
            foreach (var badChar in badChars)
                dict[badChar] = '%' + ((int)badChar).ToString("x2");
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

        /// <summary>
        /// Wrapper for the item to store.
        /// Handles returning as many StoreItemEntity instances (TableEntity) as needed to make the object fit within the table's row limit size (64kb)
        /// </summary>
        public class StoreItemContainer
        {
            private const int chunkMaxSize = 32 * 1024;     // 32768 unicode chars

            public string Key { get; private set; }
            public object Object { get; private set; }
            public string ETag { get; private set; }

            private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
            {
                // we use all so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
                TypeNameHandling = TypeNameHandling.All
            };

            public StoreItemContainer(string key, object obj)
            {
                ETag = (obj as IStoreItem)?.eTag;
                Key = key;
                Object = obj;
            }

            public IEnumerable<StoreItemEntity> Split()
            {
                // Serializa to JSON
                var json = JsonConvert.SerializeObject(Object, Formatting.None, serializationSettings);

                // Split JSON into smaller strings
                var chunks = SplitString(json, chunkMaxSize);

                // Create TableEntities with incrementing RowKey
                return chunks.Select((chunk, ix) => new StoreItemEntity(SanitizeKey(Key), ix.ToString(), Key, chunk)
                {
                    ETag = ETag
                });
            }

            public static StoreItemContainer Join(IEnumerable<StoreItemEntity> chunks)
            {
                if (chunks == null || chunks.Count() == 0)
                {
                    throw new ArgumentException("Please provide items to join", nameof(chunks));
                }

                var orderedChunks = chunks.OrderBy(o => int.Parse(o.RowKey));
                var key = orderedChunks.First().RealKey;
                var json = string.Join(string.Empty, orderedChunks.Select(o => o.Json));

                var obj = JsonConvert.DeserializeObject(json, serializationSettings);
                if (obj is IStoreItem storeItem)
                {
                    storeItem.eTag = orderedChunks.First().ETag;
                }

                return new StoreItemContainer(key, obj);
            }

            private static IEnumerable<string> SplitString(string str, int maxChunkSize)
            {
                for (int i = 0; i < str.Length; i += maxChunkSize)
                {
                    yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
                }
            }
        }

        public class StoreItemEntity : TableEntity
        {
            public StoreItemEntity() { }

            public StoreItemEntity(string partitionKey, string rowKey, string realKey, string jsonChunk) : base(partitionKey, rowKey)
            {
                RealKey = realKey;
                Json = jsonChunk;
            }

            public string RealKey { get; set; }

            public string Json { get; set; }
        }
    }
}
