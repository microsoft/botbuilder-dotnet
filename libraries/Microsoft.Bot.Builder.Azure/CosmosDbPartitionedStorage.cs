﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    public class CosmosDbPartitionedStorage : IStorage, IDisposable
    {
        private const int MaxDepthAllowed = 127;

        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, // lgtm [cs/unsafe-type-name-handling]
            MaxDepth = null
        });

        private Container _container;
        private readonly CosmosDbPartitionedStorageOptions _cosmosDbStorageOptions;
        private CosmosClient _client;
        private bool _compatibilityModePartitionKey;

        // To detect redundant calls to dispose
        private bool _disposed;

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
                throw new ArgumentException($"Service EndPoint for CosmosDB is required.", nameof(cosmosDbStorageOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.AuthKey))
            {
                throw new ArgumentException("AuthKey for CosmosDB is required.", nameof(cosmosDbStorageOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.DatabaseId))
            {
                throw new ArgumentException("DatabaseId is required.", nameof(cosmosDbStorageOptions));
            }

            if (string.IsNullOrEmpty(cosmosDbStorageOptions.ContainerId))
            {
                throw new ArgumentException("ContainerId is required.", nameof(cosmosDbStorageOptions));
            }

            if (!string.IsNullOrWhiteSpace(cosmosDbStorageOptions.KeySuffix))
            {
                if (cosmosDbStorageOptions.CompatibilityMode)
                {
                    throw new ArgumentException($"CompatibilityMode cannot be 'true' while using a KeySuffix.", nameof(cosmosDbStorageOptions));
                }

                // In order to reduce key complexity, we do not allow invalid characters in a KeySuffix
                // If the KeySuffix has invalid characters, the EscapeKey will not match
                var suffixEscaped = CosmosDbKeyEscape.EscapeKey(cosmosDbStorageOptions.KeySuffix);
                if (!cosmosDbStorageOptions.KeySuffix.Equals(suffixEscaped, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Cannot use invalid Row Key characters: {cosmosDbStorageOptions.KeySuffix}", nameof(cosmosDbStorageOptions));
                }
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
        /// <para>jsonSerializer.SerializationBinder = new AllowedTypesSerializationBinder().</para>
        /// </param>
        public CosmosDbPartitionedStorage(CosmosDbPartitionedStorageOptions cosmosDbStorageOptions, JsonSerializer jsonSerializer)
            : this(cosmosDbStorageOptions)
        {
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _jsonSerializer.MaxDepth = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbPartitionedStorage"/> class.
        /// using the provided CosmosDB credentials, database ID, and collection ID.
        /// </summary>
        /// <param name="client">The custom implementation of CosmosClient.</param>
        /// <param name="cosmosDbStorageOptions">Cosmos DB partitioned storage configuration options.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.All.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// <para>jsonSerializer.SerializationBinder = new AllowedTypesSerializationBinder().</para>
        /// </param>
        internal CosmosDbPartitionedStorage(CosmosClient client, CosmosDbPartitionedStorageOptions cosmosDbStorageOptions, JsonSerializer jsonSerializer = default)
            : this(cosmosDbStorageOptions)
        {
            _client = client;
            if (jsonSerializer != null)
            {
                _jsonSerializer = jsonSerializer;
            }
        }

        /// <summary>
        /// Reads one or more items with matching keys from the Cosmos DB container.
        /// </summary>
        /// <param name="keys">A collection of Ids for each item to be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A dictionary containing the retrieved items.</returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the array of keys (Ids for the items to be retrieved) is null.</exception>
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

            var storeItems = new Dictionary<string, object>(keys.Length);

            foreach (var key in keys)
            {
                try
                {
                    var escapedKey = CosmosDbKeyEscape.EscapeKey(key, _cosmosDbStorageOptions.KeySuffix, _cosmosDbStorageOptions.CompatibilityMode);

                    var readItemResponse = await _container.ReadItemAsync<DocumentStoreItem>(
                            escapedKey,
                            GetPartitionKey(escapedKey),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    var documentStoreItem = readItemResponse.Resource;
                    var item = documentStoreItem.Document.ToObject(typeof(object), _jsonSerializer);
                    if (_jsonSerializer.SerializationBinder is AllowedTypesSerializationBinder allowedTypesBinder)
                    {
                        allowedTypesBinder.Verify();
                    }

                    if (item is IStoreItem storeItem)
                    {
                        storeItem.ETag = documentStoreItem.ETag;
                        storeItems.Add(documentStoreItem.RealId, storeItem);
                    }
                    else
                    {
                        storeItems.Add(documentStoreItem.RealId, item);
                    }
                }
                catch (CosmosException exception)
                {
                    // When an item is not found a CosmosException is thrown, but we want to
                    // return an empty collection so in this instance we catch and do not rethrow.
                    // Throw for any other exception.
                    if (exception.StatusCode == HttpStatusCode.NotFound)
                    {
                        break;
                    }

                    throw;
                }
            }

            return storeItems;
        }

        /// <summary>
        /// Inserts or updates one or more items into the Cosmos DB container. 
        /// </summary>
        /// <param name="changes">A dictionary of items to be inserted or updated. The dictionary item key
        /// is used as the ID for the inserted / updated item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the changes dictionary is null.</exception>
        /// <exception cref="Exception">Exception thrown is the etag is empty on any of the items within the changes dictionary.</exception>
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
                if (_jsonSerializer.SerializationBinder is AllowedTypesSerializationBinder allowedTypesBinder)
                {
                    allowedTypesBinder.Verify();
                }

                // Remove etag from JSON object that was copied from IStoreItem.
                // The ETag information is updated as an _etag attribute in the document metadata.
                json.Remove("eTag");

                var documentChange = new DocumentStoreItem
                {
                    Id = CosmosDbKeyEscape.EscapeKey(change.Key, _cosmosDbStorageOptions.KeySuffix, _cosmosDbStorageOptions.CompatibilityMode),
                    RealId = change.Key,
                    Document = json,
                };

                var etag = (change.Value as IStoreItem)?.ETag;

                try
                {
                    if (etag == null || etag == "*")
                    {
                        // if new item or * then insert or replace unconditionally
                        await _container.UpsertItemAsync(
                                documentChange,
                                GetPartitionKey(documentChange.PartitionKey),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                    else if (etag.Length > 0)
                    {
                        // if we have an etag, do opt. concurrency replace
                        await _container.UpsertItemAsync(
                                documentChange,
                                GetPartitionKey(documentChange.PartitionKey),
                                new ItemRequestOptions() { IfMatchEtag = etag, },
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        throw new ArgumentException("etag empty");
                    }
                }
                catch (CosmosException ex)
                {
                    // This check could potentially be performed before even attempting to upsert the item
                    // so that a request wouldn't be made to Cosmos if it's expected to fail.
                    // However, performing the check here ensures that this custom exception is only thrown
                    // if Cosmos returns an error first.
                    // This way, the nesting limit is not imposed on the Bot Framework side
                    // and no exception will be thrown if the limit is eventually changed on the Cosmos side.
                    if (IsNestingError(json, out var message))
                    {
                        throw new InvalidOperationException(message, ex);
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes one or more items from the Cosmos DB container.
        /// </summary>
        /// <param name="keys">An array of Ids for the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the array of Ids to be deleted is null.</exception>
        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            await InitializeAsync().ConfigureAwait(false);

            foreach (var key in keys)
            {
                var escapedKey = CosmosDbKeyEscape.EscapeKey(key, _cosmosDbStorageOptions.KeySuffix, _cosmosDbStorageOptions.CompatibilityMode);

                try
                {
                    await _container.DeleteItemAsync<DocumentStoreItem>(
                            partitionKey: GetPartitionKey(escapedKey),
                            id: escapedKey,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (CosmosException exception)
                {
                    // If we get a 404 status then the item we tried to delete was not found
                    // To maintain consistency with other storage providers, we ignore this and return.
                    // Any other exceptions are thrown.
                    if (exception.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Disposes the object instance and releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes objects used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                _client?.Dispose();
            }

            _disposed = true;
        }

        private PartitionKey GetPartitionKey(string key)
        {
            if (_compatibilityModePartitionKey)
            {
                return PartitionKey.None;
            }

            return new PartitionKey(key);
        }

        /// <summary>
        /// Connects to the CosmosDB database and creates / gets the container.
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_container == null)
            {
                var cosmosClientOptions = _cosmosDbStorageOptions.CosmosClientOptions ?? new CosmosClientOptions();
                cosmosClientOptions.Serializer = new CosmosJsonSerializer(new JsonSerializerSettings { MaxDepth = null });

                if (_client == null)
                {
                    var assemblyName = this.GetType().Assembly.GetName();
                    cosmosClientOptions.ApplicationName = string.Concat(assemblyName.Name, " ", assemblyName.Version.ToString());

                    _client = new CosmosClient(
                        _cosmosDbStorageOptions.CosmosDbEndpoint,
                        _cosmosDbStorageOptions.AuthKey,
                        cosmosClientOptions);
                }

                if (_container == null)
                {
                    if (!_cosmosDbStorageOptions.CompatibilityMode)
                    {
                        await CreateContainerIfNotExistsAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        try
                        {
                            _container = _client.GetContainer(_cosmosDbStorageOptions.DatabaseId, _cosmosDbStorageOptions.ContainerId);

                            // This will throw if the container does not exist. 
                            var readContainer = await _container.ReadContainerAsync().ConfigureAwait(false);

                            // Containers created with CosmosDbStorage had no partition key set, so the default was '/_partitionKey'.
                            var partitionKeyPath = readContainer.Resource.PartitionKeyPath;
                            if (partitionKeyPath == "/_partitionKey")
                            {
                                _compatibilityModePartitionKey = true;
                            }
                            else if (partitionKeyPath != DocumentStoreItem.PartitionKeyPath)
                            {
                                // We are not supporting custom Partition Key Paths
                                throw new InvalidOperationException($"Custom Partition Key Paths are not supported. {_cosmosDbStorageOptions.ContainerId} has a custom Partition Key Path of {partitionKeyPath}.");
                            }
                        }
                        catch (CosmosException)
                        {
                            await CreateContainerIfNotExistsAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private async Task CreateContainerIfNotExistsAsync()
        {
            var containerResponse = await _client
                .GetDatabase(_cosmosDbStorageOptions.DatabaseId)
                .DefineContainer(_cosmosDbStorageOptions.ContainerId, DocumentStoreItem.PartitionKeyPath)
                .WithIndexingPolicy().WithAutomaticIndexing(false).WithIndexingMode(IndexingMode.None).Attach()
                .CreateIfNotExistsAsync(_cosmosDbStorageOptions.ContainerThroughput)
                .ConfigureAwait(false);

            _container = containerResponse.Container;
        }

        private bool IsNestingError(JObject json, out string errorMessage)
        {
            using var reader = new JTokenReader(json);
            reader.MaxDepth = null;

            while (reader.Read())
            {
                if (reader.Depth > MaxDepthAllowed)
                {
                    errorMessage = $"Maximum nesting depth of {MaxDepthAllowed} exceeded.";

                    if (IsInDialogState(json.SelectToken(reader.Path)))
                    {
                        errorMessage += " This is most likely caused by recursive component dialogs."
                            + " Try reworking your dialog code to make sure it does not keep dialogs on the stack that it's not using."
                            + " For example, consider using ReplaceDialogAsync instead of BeginDialogAsync.";
                    }
                    else
                    {
                        errorMessage += " Please check your data for signs of unintended recursion.";
                    }

                    return true;
                }
            }

            errorMessage = null;

            return false;
        }

        private bool IsInDialogState(JToken jToken) => jToken
            .Ancestors()
            .Where(ancestor => ancestor is JProperty prop && prop.Name == "dialogStack")
            .Any(dialogStackProperty => dialogStackProperty?
                .Parent["$type"]?
                .ToString()
                .StartsWith(
                    "Microsoft.Bot.Builder.Dialogs.DialogState",
                    StringComparison.OrdinalIgnoreCase) is true);

        /// <summary>
        /// Azure Cosmos DB does not expose a default implementation of CosmosSerializer that is required to set the custom JSON serializer settings.
        /// To fix this, we have to create our own implementation.
        /// <remarks>
        /// See: https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos/src/Serializer/CosmosJsonDotNetSerializer.cs
        /// </remarks>
        /// </summary>
        internal class CosmosJsonSerializer : CosmosSerializer
        {
            private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
            private readonly JsonSerializerSettings _serializerSettings;

            /// <summary>
            /// Initializes a new instance of the <see cref="CosmosJsonSerializer"/> class that uses the JSON.net serializer.
            /// </summary>
            /// <param name="jsonSerializerSettings">The JSON.net serializer.</param>
            public CosmosJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
            {
                _serializerSettings = jsonSerializerSettings ??
                      throw new ArgumentNullException(nameof(jsonSerializerSettings));
            }

            /// <summary>
            /// Convert a Stream to the passed in type.
            /// </summary>
            /// <typeparam name="T">The type of object that should be deserialized.</typeparam>
            /// <param name="stream">An open stream that is readable that contains JSON.</param>
            /// <returns>The object representing the deserialized stream.</returns>
            public override T FromStream<T>(Stream stream)
            {
                using (stream)
                {
                    if (typeof(Stream).IsAssignableFrom(typeof(T)))
                    {
                        return (T)(object)stream;
                    }

                    using (var sr = new StreamReader(stream))
                    {
                        using (var jsonTextReader = new JsonTextReader(sr) { MaxDepth = null })
                        {
                            var jsonSerializer = GetSerializer();
                            return jsonSerializer.Deserialize<T>(jsonTextReader);
                        }
                    }
                }
            }

            /// <summary>
            /// Converts an object to a open readable stream.
            /// </summary>
            /// <typeparam name="T">The type of object being serialized.</typeparam>
            /// <param name="input">The object to be serialized.</param>
            /// <returns>An open readable stream containing the JSON of the serialized object.</returns>
            public override Stream ToStream<T>(T input)
            {
                var streamPayload = new MemoryStream();
                using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
                {
                    using (JsonWriter writer = new JsonTextWriter(streamWriter))
                    {
                        writer.Formatting = Formatting.None;
                        var jsonSerializer = GetSerializer();
                        jsonSerializer.Serialize(writer, input);
                        writer.Flush();
                        streamWriter.Flush();
                    }
                }

                streamPayload.Position = 0;
                return streamPayload;
            }

            /// <summary>
            /// JsonSerializer has hit a race conditions with custom settings that cause null reference exception.
            /// To avoid the race condition a new JsonSerializer is created for each call.
            /// </summary>
            private JsonSerializer GetSerializer()
            {
                return JsonSerializer.Create(_serializerSettings);
            }
        }

        /// <summary>
        /// Internal data structure for storing items in a CosmosDB Collection.
        /// </summary>
        internal class DocumentStoreItem : IStoreItem
        {
            /// <summary>
            /// Gets the PartitionKey path to be used for this document type.
            /// </summary>
            /// <value>
            /// The PartitionKey path to be used for this document type.
            /// </value>
            public static string PartitionKeyPath => "/id";

            /// <summary>
            /// Gets or sets the sanitized Id/Key used as PrimaryKey.
            /// </summary>
            /// <value>
            /// The sanitized Id/Key used as PrimaryKey.
            /// </value>
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the un-sanitized Id/Key.
            /// </summary>
            /// <value>
            /// The un-sanitized Id/Key.
            /// </value>
            [JsonProperty("realId")]
            public string RealId { get; internal set; }

            /// <summary>
            /// Gets or sets the persisted object.
            /// </summary>
            /// <value>
            /// The persisted object.
            /// </value>
            [JsonProperty("document")]
            public JObject Document { get; set; }

            /// <summary>
            /// Gets or sets the ETag information for handling optimistic concurrency updates.
            /// </summary>
            /// <value>
            /// The ETag information for handling optimistic concurrency updates.
            /// </value>
            [JsonProperty("_etag")]
            public string ETag { get; set; }

            /// <summary>
            /// Gets the PartitionKey value for the document.
            /// </summary>
            /// <value>
            /// The PartitionKey value for the document.
            /// </value>
            public string PartitionKey => this.Id;
        }
    }
}
