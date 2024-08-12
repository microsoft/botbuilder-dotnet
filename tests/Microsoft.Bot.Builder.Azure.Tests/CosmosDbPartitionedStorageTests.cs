// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder.Dialogs;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - CosmosDB Partitioned")]
    public class CosmosDbPartitionedStorageTests
    {
        private CosmosDbPartitionedStorage _storage;
        private readonly Mock<Container> _container = new Mock<Container>();
        
        [Fact]
        public void ConstructorValidation()
        {
            // Should work.
            _ = new CosmosDbPartitionedStorage(
                cosmosDbStorageOptions: new CosmosDbPartitionedStorageOptions
                {
                    CosmosDbEndpoint = "CosmosDbEndpoint",
                    AuthKey = "AuthKey",
                    DatabaseId = "DatabaseId",
                    ContainerId = "ContainerId",
                },
                jsonSerializer: JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

            // No Options. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbPartitionedStorage(null));

            // No Endpoint. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                CosmosDbEndpoint = null,
            }));

            // No Auth Key or TokenCredential. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                CosmosDbEndpoint = "CosmosDbEndpoint",
                AuthKey = null,
                TokenCredential = null
            }));

            // No Database Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                CosmosDbEndpoint = "CosmosDbEndpoint",
                AuthKey = "AuthKey",
                DatabaseId = null,
            }));

            // No Container Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                CosmosDbEndpoint = "CosmosDbEndpoint",
                AuthKey = "AuthKey",
                DatabaseId = "DatabaseId",
                ContainerId = null,
            }));

            // No JsonSerializer. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbPartitionedStorage(
                new CosmosDbPartitionedStorageOptions()
                {
                    CosmosDbEndpoint = "CosmosDbEndpoint",
                    AuthKey = "AuthKey",
                    DatabaseId = "DatabaseId",
                    ContainerId = "ContainerId",
                }, null));

            // KeySuffix with CompatibilityMode == "true". Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                CosmosDbEndpoint = "CosmosDbEndpoint",
                AuthKey = "AuthKey",
                DatabaseId = "DatabaseId",
                ContainerId = "ContainerId",
                KeySuffix = "KeySuffix",
                CompatibilityMode = true
            }));

            // KeySuffix with CompatibilityMode == "false" and invalid characters. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                CosmosDbEndpoint = "CosmosDbEndpoint",
                AuthKey = "AuthKey",
                DatabaseId = "DatabaseId",
                ContainerId = "ContainerId",
                KeySuffix = "?#*test",
                CompatibilityMode = false
            }));
        }
        
        [Fact]
        public async Task ReadAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.ReadAsync(null));

            // Empty keys. Should return empty.
            var empty = await _storage.ReadAsync(new string[] { });
            Assert.Empty(empty);
        }

        [Fact]
        public async Task ReadAsync()
        {
            InitStorage();

            var resource = new CosmosDbPartitionedStorage.DocumentStoreItem
            {
                RealId = "RealId",
                ETag = "ETag1",
                Document = JObject.Parse("{ \"ETag\":\"ETag2\" }")
            };
            var itemResponse = new DocumentStoreItemResponseMock(resource);

            _container.Setup(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse);

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Single(items);
            _container.Verify(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncWithAllowedTypesSerializationBinder()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All, // CODEQL [cs/unsafe-type-name-handling] we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
                MaxDepth = null,
                SerializationBinder = new AllowedTypesSerializationBinder(
                    new List<Type>
                    {
                        typeof(IStoreItem),
                    }),
            };

            InitStorage(jsonSerializerSettings: jsonSerializerSettings);
            
            var storeItem = new StoreItem
            {
                ETag = "*"
            };
            var document = JObject.FromObject(storeItem, JsonSerializer.Create(jsonSerializerSettings));
            var resource = new CosmosDbPartitionedStorage.DocumentStoreItem
            {
                RealId = "RealId",
                ETag = "ETag1",
                Document = document
            };
            var itemResponse = new DocumentStoreItemResponseMock(resource);

            _container.Setup(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse);

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Single(items);
            _container.Verify(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncWithEmptyAllowedTypesSerializationBinder()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All, // CODEQL [cs/unsafe-type-name-handling] we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
                MaxDepth = null,
                SerializationBinder = new AllowedTypesSerializationBinder(),
            };
            InitStorage(jsonSerializerSettings: jsonSerializerSettings);
            
            var storeItem = new StoreItem
            {
                ETag = "*"
            };
            var document = JObject.FromObject(storeItem, JsonSerializer.Create(jsonSerializerSettings));
            var resource = new CosmosDbPartitionedStorage.DocumentStoreItem
            {
                RealId = "RealId",
                ETag = "ETag1",
                Document = document
            };
            var itemResponse = new DocumentStoreItemResponseMock(resource);

            _container.Setup(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.ReadAsync(new string[] { "key" }));

            _container.Verify(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncPartitionKey()
        {
            InitStorage("/_partitionKey");

            var resource = new CosmosDbPartitionedStorage.DocumentStoreItem
            {
                RealId = "RealId",
                ETag = "ETag1",
                Document = JObject.Parse("{ \"ETag\":\"ETag2\" }")
            };
            var itemResponse = new DocumentStoreItemResponseMock(resource);

            _container.Setup(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(itemResponse);

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Single(items);
            _container.Verify(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncNotFound()
        {
            InitStorage();

            _container.Setup(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("NotFound", HttpStatusCode.NotFound, 0, "0", 0));

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Empty(items);
            _container.Verify(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncFailure()
        {
            InitStorage();

            _container.Setup(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("InternalServerError", HttpStatusCode.InternalServerError, 0, "0", 0));

            await Assert.ThrowsAsync<CosmosException>(() => _storage.ReadAsync(new string[] { "key" }));
            _container.Verify(e => e.ReadItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncCustomPartitionKeyFailure()
        {
            InitStorage("/customKey");

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.ReadAsync(new string[] { "key" }));
        }

        [Fact]
        public async Task WriteAsyncValidation()
        {
            InitStorage();

            // No changes. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.WriteAsync(null));

            // Empty changes. Should return.
            await _storage.WriteAsync(new Dictionary<string, object>());
        }

        [Fact]
        public async Task WriteAsync()
        {
            InitStorage();

            _container.Setup(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()));

            var changes = new Dictionary<string, object>
            {
                { "key1", new CosmosDbPartitionedStorage.DocumentStoreItem() },
                { "key2", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = "*" } },
                { "key3", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = "ETag" } },
            };

            await _storage.WriteAsync(changes);

            _container.Verify(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }
        
        [Fact]
        public async Task WriteAsyncWithAllowedTypesSerializationBinder()
        {            
            var serializationBinder = new AllowedTypesSerializationBinder(
                new List<Type>
                {
                    typeof(IStoreItem),
                });
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All, // CODEQL [cs/unsafe-type-name-handling] we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
                MaxDepth = null,
                SerializationBinder = serializationBinder,
            };

            InitStorage(jsonSerializerSettings: jsonSerializerSettings);

            _container.Setup(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()));
            
            var storeItem = new StoreItem
            {
                ETag = "*"
            };
            var document = JObject.FromObject(storeItem, JsonSerializer.Create(jsonSerializerSettings));
            var changes = new Dictionary<string, object>
            {
                { "key1", new CosmosDbPartitionedStorage.DocumentStoreItem() },
                { "key2", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = "*", Document = document } },
                { "key3", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = "ETag" } },
            };

            await _storage.WriteAsync(changes);

            _container.Verify(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            Assert.Equal(3, serializationBinder.AllowedTypes.Count);
        }
        
        [Fact]
        public async Task WriteAsyncWithEmptyAllowedTypesSerializationBinder()
        {            
            var serializationBinder = new AllowedTypesSerializationBinder();
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All, // CODEQL [cs/unsafe-type-name-handling] we use All so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
                MaxDepth = null,
                SerializationBinder = serializationBinder,
            };
            InitStorage(jsonSerializerSettings: jsonSerializerSettings);

            _container.Setup(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()));
            
            var storeItem = new StoreItem
            {
                ETag = "*"
            };
            var document = JObject.FromObject(storeItem, JsonSerializer.Create(jsonSerializerSettings));
            var changes = new Dictionary<string, object>
            {
                { "key1", new CosmosDbPartitionedStorage.DocumentStoreItem() },
                { "key2", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = "*", Document = document } },
                { "key3", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = "ETag" } },
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.WriteAsync(changes));

            _container.Verify(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
            Assert.Empty(serializationBinder.AllowedTypes);
        }

        [Fact]
        public async Task WriteAsyncEmptyTagFailure()
        {
            InitStorage();

            var changes = new Dictionary<string, object>
            {
                { "key", new CosmosDbPartitionedStorage.DocumentStoreItem { ETag = string.Empty } },
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _storage.WriteAsync(changes));
        }

        [Fact]
        public async Task WriteAsyncFailure()
        {
            InitStorage();

            _container.Setup(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("InternalServerError", HttpStatusCode.InternalServerError, 0, "0", 0));

            var changes = new Dictionary<string, object> { { "key", new CosmosDbPartitionedStorage.DocumentStoreItem() } };

            await Assert.ThrowsAsync<CosmosException>(() => _storage.WriteAsync(changes));
            _container.Verify(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WriteAsyncWithNestedFailure()
        {
            InitStorage();

            _container.Setup(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("InternalServerError", HttpStatusCode.InternalServerError, 0, "0", 0));

            var nestedJson = GenerateNestedDict();

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.WriteAsync(nestedJson));
            _container.Verify(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WriteAsyncWithNestedDialogFailure()
        {
            InitStorage();

            _container.Setup(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("InternalServerError", HttpStatusCode.InternalServerError, 0, "0", 0));

            var nestedJson = GenerateNestedDict();

            var dialogInstance = new DialogInstance { State = nestedJson };
            var dialogState = new DialogState(new List<DialogInstance> { dialogInstance });
            var changes = new Dictionary<string, object> { { "state", dialogState } };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.WriteAsync(changes));
            _container.Verify(e => e.UpsertItemAsync(It.IsAny<CosmosDbPartitionedStorage.DocumentStoreItem>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteAsync(null));
        }

        [Fact]
        public async Task DeleteAsync()
        {
            InitStorage();

            _container.Setup(e => e.DeleteItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()));

            await _storage.DeleteAsync(new string[] { "key" });

            _container.Verify(e => e.DeleteItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsyncNotFound()
        {
            InitStorage();

            _container.Setup(e => e.DeleteItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("NotFound", HttpStatusCode.NotFound, 0, "0", 0));

            await _storage.DeleteAsync(new string[] { "key" });

            _container.Verify(e => e.DeleteItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsyncFailure()
        {
            InitStorage();

            _container.Setup(e => e.DeleteItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("InternalServerError", HttpStatusCode.InternalServerError, 0, "0", 0));

            await Assert.ThrowsAsync<CosmosException>(() => _storage.DeleteAsync(new string[] { "key" }));
            _container.Verify(e => e.DeleteItemAsync<CosmosDbPartitionedStorage.DocumentStoreItem>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private void InitStorage(string partitionKey = "/id", CosmosDbPartitionedStorageOptions storageOptions = default, JsonSerializerSettings jsonSerializerSettings = default)
        {
            var client = new Mock<CosmosClient>();
            var containerProperties = new ContainerProperties("id", partitionKey);
            var containerResponse = new Mock<ContainerResponse>();
            var jsonSerializer = jsonSerializerSettings != null ? JsonSerializer.Create(jsonSerializerSettings) : null;

            containerResponse.SetupGet(e => e.Resource)
                .Returns(containerProperties);
            client.Setup(e => e.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_container.Object);
            _container.Setup(e => e.ReadContainerAsync(It.IsAny<ContainerRequestOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(containerResponse.Object);

            var options = storageOptions ?? new CosmosDbPartitionedStorageOptions
            {
                CosmosDbEndpoint = "CosmosDbEndpoint",
                AuthKey = "AuthKey",
                DatabaseId = "DatabaseId",
                ContainerId = "ContainerId",
            };

            _storage = new CosmosDbPartitionedStorage(client.Object, options, jsonSerializer);
        }

        private Dictionary<string, object> GenerateNestedDict()
        {
            var nested = new Dictionary<string, object>();
            var current = new Dictionary<string, object>();

            nested.Add("0", current);
            for (var i = 1; i <= 127; i++)
            {
                var child = new Dictionary<string, object>();
                current.Add(i.ToString(), child);
                current = child;
            }

            return nested;
        }

        private class DocumentStoreItemResponseMock : ItemResponse<CosmosDbPartitionedStorage.DocumentStoreItem>
        {
            public DocumentStoreItemResponseMock(CosmosDbPartitionedStorage.DocumentStoreItem resource)
            {
                Resource = resource;
            }

            public override CosmosDbPartitionedStorage.DocumentStoreItem Resource { get; }
        }
        
        private class StoreItem : IStoreItem
        {
            public int Id { get; set; } = 0;

            public string Topic { get; set; } = "car";

            public string ETag { get; set; }
        }
    }
}
