// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

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

            await Task.WhenAll(
                keys.Select(k => new EntityKey(k))
                    .Select(ek => Table.ExecuteAsync(TableOperation.Delete(new TableEntity(ek.PartitionKey, ek.RowKey) { ETag = "*" }))))
                        .ConfigureAwait(false);
        }

        /// <summary>
        /// Loads store items from storage.
        /// </summary>
        /// <param name="keys">Array of item keys to read from the store.</param>
        public async Task<IEnumerable<KeyValuePair<string, object>>> Read(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Please provide at least one key to read from storage.", nameof(keys));
            }

            var readTasks = keys.Select(async key =>
            {
                var ek = new EntityKey(key);
                var tableEntity = await Table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(ek.PartitionKey, ek.RowKey)).ConfigureAwait(false);

                if (tableEntity.HttpStatusCode == (int)HttpStatusCode.OK)
                {
                    // re-create expected object
                    StoreItemEntity storeItem = StoreItemEntity.AsStoreItemEntity(tableEntity);
                    return new KeyValuePair<string, object>(key, storeItem.Entity);
                }

                return new KeyValuePair<string, object>();
            });

            return (await Task.WhenAll(readTasks).ConfigureAwait(false))
                .Where(kv => kv.Key != null);
        }

        /// <summary>
        /// Saves store items to storage.
        /// </summary>
        /// <param name="changes">Map of items to write to storage.</param>
        /// <returns></returns>
        public async Task Write(IEnumerable<KeyValuePair<string, object>> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            var storeItems = changes.Select(kv => new StoreItemEntity(new EntityKey(kv.Key), kv.Value));
            var bogusEtagKeys = storeItems.Where(item => item.ETag != null && item.ETag.Length == 0);
            if (bogusEtagKeys.Any())
            {
                throw new ArgumentException("Bogus etag in items with key: " + string.Join(", ", bogusEtagKeys.Select(o => o.Key)));
            }

            var writeTasks = changes.Select(kv =>
            {
                var storeEntity = new StoreItemEntity(new EntityKey(kv.Key), kv.Value);

                // Re-create object as table entity
                var tableEntity = storeEntity.AsTableEntity();

                if (storeEntity.ETag == null || storeEntity.ETag == "*")
                {
                    // New item or etag=* then insert or replace unconditionaly
                    return Table.ExecuteAsync(TableOperation.InsertOrReplace(tableEntity));
                }


                // Optimistic Update
                return Table.ExecuteAsync(TableOperation.Replace(tableEntity));
            });

            await Task.WhenAll(writeTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Internal data structure for storing items in Azure Tables.
        /// </summary>
        private class StoreItemEntity
        {
            public object Entity { get; private set; }
            public EntityKey Key { get; private set; }
            public string ETag
            {
                get
                {
                    return (Entity as IStoreItem)?.eTag;
                }
            }

            public StoreItemEntity(EntityKey key, object entity)
            {
                Key = key;
                Entity = entity;
            }

            public DynamicTableEntity AsTableEntity()
            {
                // Flatten properties
                var properties = new Dictionary<string, EntityProperty>();
                Flatten(properties, Entity);

                // Add Type information
                var type = Entity.GetType();
                var typeQualifiedName = type.AssemblyQualifiedName;
                properties.Add("__type", EntityProperty.GeneratePropertyForString(typeQualifiedName));

                return new DynamicTableEntity(Key.PartitionKey, Key.RowKey)
                {
                    ETag = ETag,
                    Properties = properties
                };
            }

            public static StoreItemEntity AsStoreItemEntity(TableResult tableEntity)
            {
                // Create instance of proper type
                var dynamicTableEntity = (DynamicTableEntity)tableEntity.Result;
                var type = Type.GetType(dynamicTableEntity.Properties["__type"].StringValue);
                var properties = dynamicTableEntity.Properties;
                //.Where(kv => !kv.Key.EndsWith("__type")).ToDictionary(kv => kv.Key, kv => kv.Value);
                var value = CreateInstaceOf(type);

                // Set object properties
                /*if (value is IStoreItem storeItem)
                {
                    // Apply properties to StoreItem (FlexObject)
                    value = ApplyProperties(properties, storeItem);
                }
                else */if (IsAnonymousType(type))
                {
                    // Anonymous Type not supported - Anonymous Types may not have setters and values are passed in the constructor
                    // Convert to dynamic/ExpandoObject instead
                    value = ApplyProperties(properties, new ExpandoObject()) as dynamic;
                }
                else
                {
                    // POCO entity
                    TableEntity.ReadUserObject(value, properties, new OperationContext());
                }

                // IStoreItem? apply Etag
                if (value is IStoreItem iStoreItem)
                {
                    iStoreItem.eTag = tableEntity.Etag;
                }

                return new StoreItemEntity(new EntityKey(dynamicTableEntity.PartitionKey, dynamicTableEntity.RowKey), value);
            }

            private static dynamic ApplyProperties(IDictionary<string, EntityProperty> properties, dynamic item)
            {
                foreach (var prop in properties)
                {
                    var key = prop.Key;
                    var value = prop.Value.PropertyAsObject;

                    // Set the value to its property
                    /*if (item is StoreItem)
                    {
                        item.Add(key, value);
                    }
                    else */if (item is IDictionary<string, Object> expando)
                    {
                        expando[key] = value;
                    }
                }

                return item;
            }

            private static object CreateInstaceOf(Type type)
            {
                var ctor = type
                    .GetConstructors()
                    .FirstOrDefault(c => c.GetParameters().Length > 0);

                return ctor != null
                    // Possible anonymous type
                    ? ctor.Invoke
                        (ctor.GetParameters()
                            .Select(p =>
                                p.HasDefaultValue ? p.DefaultValue :
                                p.ParameterType.IsValueType && Nullable.GetUnderlyingType(p.ParameterType) == null
                                    ? Activator.CreateInstance(p.ParameterType)
                                    : null
                            ).ToArray()
                        )
                    : Activator.CreateInstance(type);
            }

            private static void Flatten(IDictionary<string, EntityProperty> properties, object entity, string prefix = "")
            {
                /*if (entity is StoreItem storeItem)
                {
                    // StoreItem
                    foreach (var prop in storeItem)
                    {
                        var propValue = prop.Value;
                        if (propValue == null) return;

                        if (IsSimple(propValue.GetType()))
                        {
                            properties.Add(prefix + prop.Key, EntityProperty.CreateEntityPropertyFromObject(propValue));
                        }
                        else
                        {
                            var typeQualifiedName = propValue.GetType().AssemblyQualifiedName;
                            properties.Add(prop.Key + "___type", EntityProperty.GeneratePropertyForString(typeQualifiedName));
                            Flatten(properties, propValue, prop.Key + "_");
                        }
                    }

                    return;
                }
                else */if (IsAnonymousType(entity.GetType()))
                {
                    // Anonymous Object
                    var entityProps = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead);
                    foreach (var prop in entityProps)
                    {
                        var propValue = prop.GetValue(entity);
                        if (propValue == null) return;

                        if (IsSimple(propValue.GetType()))
                        {
                            properties.Add(prefix + prop.Name, EntityProperty.CreateEntityPropertyFromObject(propValue));
                        }
                        else
                        {
                            var typeQualifiedName = propValue.GetType().AssemblyQualifiedName;
                            properties.Add(prop.Name + "___type", EntityProperty.GeneratePropertyForString(typeQualifiedName));
                            Flatten(properties, propValue, prop.Name + "_");
                        }
                    }

                    return;
                }
                else
                {
                    // POCO objects
                    var childProperties = EntityPropertyConverter.Flatten(entity, new OperationContext());
                    foreach (var childProp in childProperties)
                    {
                        properties.Add(prefix + childProp.Key, childProp.Value);
                    }
                }
            }

            private static bool IsSimple(Type type)
            {
                return type.IsPrimitive
                  || type.IsEnum
                  || type.Equals(typeof(string))
                  || type.Equals(typeof(decimal));
            }

            private static bool IsAnonymousType(Type type)
            {
                Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Count() > 0;
                Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
                return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            }
        }

        /// <summary>
        /// Entity that maps property to PartitionKey and RowKey
        /// </summary>
        private class EntityKey
        {
            public string PartitionKey { get; private set; }
            public string RowKey { get; private set; }

            public EntityKey(string propertyKey)
            {
                PartitionKey = SanitizeKey(propertyKey);
                RowKey = string.Empty;
            }

            public EntityKey(string partitionKey, string rowKey)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
            }

            /// <summary>
            /// Escapes a property key into a PartitionKey that can be used with Azure Tables.
            /// More information at https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model?redirectedfrom=MSDN#table-names
            /// </summary>
            /// <param name="key">The Property Key</param>
            /// <returns>Sanitized key that can be used as PartitionKey</returns>
            public static string SanitizeKey(string key)
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

            private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
            {
                char[] badChars = new char[] { '\\', '?', '/', '#', '\t', '\n', '\r' };
                var dict = new Dictionary<char, string>();
                foreach (var badChar in badChars)
                    dict[badChar] = '%' + ((int)badChar).ToString("x2");
                return dict;
            });
        }
    }
}
