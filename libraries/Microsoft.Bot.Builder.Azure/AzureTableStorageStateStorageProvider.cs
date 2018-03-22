// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.State;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class AzureTableStorageStateStorageProvider : IStateStorageProvider
    {
        private const string BotPropertyPrefix = "__Bot_";
        private const string BotRawStateNamespaceTableStoragePropertyName = BotPropertyPrefix + "RawStateNamespace";
        private const string BotRawKeyTableStoragePropertyName = BotPropertyPrefix + "RawKey";

        private static readonly HashSet<string> CheckedTables = new HashSet<string>();
        private static readonly Regex TableStorageKeyInvalidCharactersRegex = new Regex("[!@#$%^&*\\(\\)~/\\\\><,.?';\"\u0000-\u001F\u007F-\u009F]", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly CloudTable _table;

        public AzureTableStorageStateStorageProvider(string dataConnectionString, string tableName) : this(CloudStorageAccount.Parse(dataConnectionString), tableName)
        {
        }

        public AzureTableStorageStateStorageProvider(CloudStorageAccount storageAccount, string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(tableName);

            if (CheckedTables.Add($"{storageAccount.TableStorageUri.PrimaryUri.Host}-{tableName}"))
            {
                _table.CreateIfNotExistsAsync().Wait();
            }
        }

        public CloudTable Table => _table;

        public IStateStorageEntry CreateNewEntry(string stateNamespace, string key)
        {
            var tableEntity = new DynamicTableEntity(NormalizeKeyValueForTableStorage(stateNamespace), NormalizeKeyValueForTableStorage(key));
            tableEntity.Properties[BotRawStateNamespaceTableStoragePropertyName] = new EntityProperty(stateNamespace);
            tableEntity.Properties[BotRawKeyTableStoragePropertyName] = new EntityProperty(key);

            return new AzureTableStorageEntityStateStorageEntry(tableEntity);
        }

        public async Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace)
        {
            var queryContinuationToken = default(TableContinuationToken);
            var query = new TableQuery()
                            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, NormalizeKeyValueForTableStorage(stateNamespace)));


            var results = new List<IStateStorageEntry>();

            do
            {
                var entitySegment = await _table.ExecuteQuerySegmentedAsync(query, queryContinuationToken).ConfigureAwait(false);

                results.AddRange(entitySegment.Results.Select(entity => new AzureTableStorageEntityStateStorageEntry(entity)));

                queryContinuationToken = entitySegment.ContinuationToken;
            } while (queryContinuationToken != default(TableContinuationToken));

            return results;
        }

        public async Task<IStateStorageEntry> Load(string stateNamespace, string key)
        {
            var tableResult = await _table.ExecuteAsync(TableOperation.Retrieve(NormalizeKeyValueForTableStorage(stateNamespace), NormalizeKeyValueForTableStorage(key))).ConfigureAwait(false);

            return tableResult.Result != null ? new AzureTableStorageEntityStateStorageEntry((DynamicTableEntity)tableResult.Result) : default(IStateStorageEntry);
        }

        public async Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace, IEnumerable<string> keys)
        {
            var loadTasks = new List<Task<IStateStorageEntry>>();

            foreach (var key in keys)
            {
                loadTasks.Add(Load(stateNamespace, key));
            }

            await Task.WhenAll(loadTasks);

            var results = new List<IStateStorageEntry>(loadTasks.Count);

            foreach (var loadTask in loadTasks)
            {
                results.Add(loadTask.Result);
            }

            return results;
        }

        public async Task Save(IEnumerable<IStateStorageEntry> stateStoreEntries)
        {
            try
            {
                foreach (var stateNamespaceBatch in stateStoreEntries.GroupBy(stateStoreEntry => stateStoreEntry.Namespace))
                {
                    var partitionBatchOperation = new TableBatchOperation();

                    foreach (var stateStoreEntry in stateNamespaceBatch)
                    {
                        if (!(stateStoreEntry is AzureTableStorageEntityStateStorageEntry azureTableStorageStateStoreEntry))
                        {
                            throw new InvalidOperationException($"{nameof(AzureTableStorageStateStorageProvider)} only supports instances of type {nameof(AzureTableStorageEntityStateStorageEntry)}.");
                        }

                        var value = azureTableStorageStateStoreEntry.RawValue;
                        var tableEntity = azureTableStorageStateStoreEntry.TableEntity;

                        var operation = default(TableOperation);

                        if (value != null)
                        {
                            var properties = TableEntity.Flatten(value, new OperationContext());

                            properties.Add(BotRawStateNamespaceTableStoragePropertyName, tableEntity.Properties[BotRawStateNamespaceTableStoragePropertyName]);
                            properties.Add(BotRawKeyTableStoragePropertyName, tableEntity.Properties[BotRawKeyTableStoragePropertyName]);

                            tableEntity.Properties = properties;
                        }

                        operation = tableEntity.ETag == null ? TableOperation.Insert(tableEntity) : TableOperation.Replace(tableEntity);

                        partitionBatchOperation.Add(operation);

                        if (partitionBatchOperation.Count == 100)
                        {
                            await _table.ExecuteBatchAsync(partitionBatchOperation).ConfigureAwait(false);

                            partitionBatchOperation.Clear();
                        }
                    }

                    if (partitionBatchOperation.Count > 0)
                    {
                        await _table.ExecuteBatchAsync(partitionBatchOperation).ConfigureAwait(false);
                    }
                }
            }
            catch(StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == 412)
            {
                if(storageException.RequestInformation.ExtendedErrorInformation?.ErrorCode == "UpdateConditionNotSatisfied")
                {
                    throw new StateOptimisticConcurrencyViolationException("One or more entities could not be saved because they are outdated. Please check the inner exception for more details.", storageException);
                }

                throw;
            }
        }

        public async Task Delete(string stateNamespace)
        {
            var queryContinuationToken = default(TableContinuationToken);
            var query = new TableQuery()
                            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, NormalizeKeyValueForTableStorage(stateNamespace)));


            var deleteTasks = new List<Task>();

            do
            {
                var entitySegment = await _table.ExecuteQuerySegmentedAsync(query, queryContinuationToken).ConfigureAwait(false);

                foreach(var entity in entitySegment.Results)
                {
                    deleteTasks.Add(_table.ExecuteAsync(TableOperation.Delete(entity)));
                }

                await Task.WhenAll(deleteTasks);

                deleteTasks.Clear();

                queryContinuationToken = entitySegment.ContinuationToken;
            } while (queryContinuationToken != default(TableContinuationToken));
        }

        public Task Delete(string stateNamespace, string key) => _table.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(NormalizeKeyValueForTableStorage(stateNamespace), NormalizeKeyValueForTableStorage(key)) { ETag = "*" }));

        public async Task Delete(string stateNamespace, IEnumerable<string> keys)
        {
            var deleteTasks = new List<Task>();

            foreach (var key in keys)
            {
                deleteTasks.Add(Delete(stateNamespace, key));
            }

            await Task.WhenAll(deleteTasks);
        }

        private string NormalizeKeyValueForTableStorage(string value) => TableStorageKeyInvalidCharactersRegex.Replace(value, "`");

        private sealed class AzureTableStorageEntityStateStorageEntry : DeferredValueStateStorageEntry
        {
            private readonly DynamicTableEntity _tableEntity;

            public AzureTableStorageEntityStateStorageEntry(DynamicTableEntity tableEntity) : base(tableEntity.Properties[BotRawStateNamespaceTableStoragePropertyName].StringValue, tableEntity.Properties[BotRawKeyTableStoragePropertyName].StringValue, tableEntity.ETag)
            {
                _tableEntity = tableEntity ?? throw new ArgumentNullException(nameof(tableEntity));
            }

            internal DynamicTableEntity TableEntity => _tableEntity;

            protected override T MaterializeValue<T>() => EntityPropertyConverter.ConvertBack<T>(new BotPropertyFilteringPropertiesDictionary(_tableEntity.Properties), new OperationContext());

            private sealed class BotPropertyFilteringPropertiesDictionary : IDictionary<string, EntityProperty>
            {
                private readonly IDictionary<string, EntityProperty> _innerProperties;

                public BotPropertyFilteringPropertiesDictionary(IDictionary<string, EntityProperty> innerProperties)
                {
                    _innerProperties = innerProperties;
                }

                public EntityProperty this[string key] { get => _innerProperties[key]; set => throw new NotImplementedException(); }

                public ICollection<string> Keys => _innerProperties.Keys.Where(k => !k.StartsWith(BotPropertyPrefix)).ToList();

                public ICollection<EntityProperty> Values => _innerProperties.Where(kvp => !kvp.Key.StartsWith(BotPropertyPrefix)).Select(kvp => kvp.Value).ToList();

                public int Count => Keys.Count;

                public bool IsReadOnly => true;

                public void Add(string key, EntityProperty value) => throw new NotSupportedException();

                public void Add(KeyValuePair<string, EntityProperty> item) => throw new NotSupportedException();

                public void Clear() => throw new NotSupportedException();

                public bool Contains(KeyValuePair<string, EntityProperty> item) => !item.Key.StartsWith(BotPropertyPrefix) && _innerProperties.Contains(item);

                public bool ContainsKey(string key) => !key.StartsWith(BotPropertyPrefix) && _innerProperties.ContainsKey(key);

                public void CopyTo(KeyValuePair<string, EntityProperty>[] array, int arrayIndex) => throw new NotImplementedException();

                public IEnumerator<KeyValuePair<string, EntityProperty>> GetEnumerator()
                {
                    foreach(var kvp in _innerProperties)
                    {
                        if(!kvp.Key.StartsWith(BotPropertyPrefix))
                        {
                            yield return kvp;
                        }
                    }
                }

                public bool Remove(string key)  => throw new NotSupportedException();

                public bool Remove(KeyValuePair<string, EntityProperty> item) => throw new NotSupportedException();

                public bool TryGetValue(string key, out EntityProperty value) => _innerProperties.TryGetValue(key, out value) && !key.StartsWith(BotPropertyPrefix);

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }
        }
    }
}
