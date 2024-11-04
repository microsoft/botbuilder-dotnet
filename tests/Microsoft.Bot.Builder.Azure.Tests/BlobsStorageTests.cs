// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Bot.Builder.Azure.Blobs;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class BlobsStorageTests
    {
        private const string ConnectionString = @"UseDevelopmentStorage=true";

        private BlobsStorage _storage;
        private readonly Mock<BlobClient> _client = new Mock<BlobClient>();
        
        [Fact]
        public void ConstructorValidation()
        {
            // Should work.
            _ = new BlobsStorage(
                ConnectionString,
                "containerName",
                JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

            // No dataConnectionString. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsStorage(null, "containerName"));
            Assert.Throws<ArgumentNullException>(() => new BlobsStorage(string.Empty, "containerName"));

            // No containerName. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsStorage(ConnectionString, null));
            Assert.Throws<ArgumentNullException>(() => new BlobsStorage(ConnectionString, string.Empty));
        }

        [Fact]
        public void ConstructorWithTokenCredentialValidation()
        {
            var mockTokenCredential = new Moq.Mock<TokenCredential>();
            var storageTransferOptions = new StorageTransferOptions();
            var uri = new Uri("https://uritest.com");

            // Should work.
            _ = new BlobsStorage(
                uri,
                mockTokenCredential.Object,
                storageTransferOptions,
                new BlobClientOptions(),
                JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

            // No blobContainerUri. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsStorage(null, mockTokenCredential.Object, storageTransferOptions));

            // No tokenCredential. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsStorage(uri, null, storageTransferOptions));
        }

        [Fact]
        public async Task WriteAsyncValidation()
        {
            InitStorage();

            // No changes. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.WriteAsync(null));
        }

        [Fact]
        public async Task WriteAsync()
        {
            InitStorage();

            _client.Setup(e => e.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<BlobHttpHeaders>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()));

            var changes = new Dictionary<string, object>
            {
                { "key1", new StoreItem() },
                { "key2", new StoreItem { ETag = "*" } },
                { "key3", new StoreItem { ETag = "ETag" } },
                { "key4", new List<StoreItem>() { new StoreItem() } },
                { "key5", new Dictionary<string, StoreItem>() { { "key1", new StoreItem() } } },
                { "key6", "value1" },
            };

            await _storage.WriteAsync(changes);

            _client.Verify(
                e => e.UploadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<BlobHttpHeaders>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<IProgress<long>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<StorageTransferOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(6));
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

            InitStorage(jsonSerializerSettings);

            _client.Setup(e => e.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<BlobHttpHeaders>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()));

            var changes = new Dictionary<string, object>
            {
                { "key1", new StoreItem() },
                { "key2", new StoreItem { ETag = "*" } },
                { "key3", new StoreItem { ETag = "ETag" } },
                { "key4", new List<StoreItem>() { new StoreItem() } },
                { "key5", new Dictionary<string, StoreItem>() { { "key1", new StoreItem() } } },
                { "key6", "value1" },
            };

            await _storage.WriteAsync(changes);

            _client.Verify(
                e => e.UploadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<BlobHttpHeaders>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<IProgress<long>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<StorageTransferOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(6));
            Assert.Equal(4, serializationBinder.AllowedTypes.Count);
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

            InitStorage(jsonSerializerSettings);

            _client.Setup(e => e.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<BlobHttpHeaders>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<IProgress<long>>(),
                It.IsAny<AccessTier?>(),
                It.IsAny<StorageTransferOptions>(),
                It.IsAny<CancellationToken>()));

            var changes = new Dictionary<string, object>
            {
                { "key1", new StoreItem() },
                { "key2", new StoreItem { ETag = "*" } },
                { "key3", new StoreItem { ETag = "ETag" } },
                { "key4", new List<StoreItem>() { new StoreItem() } },
                { "key5", new Dictionary<string, StoreItem>() { { "key1", new StoreItem() } } },
                { "key6", "value1" },
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.WriteAsync(changes));

            _client.Verify(
                e => e.UploadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<BlobHttpHeaders>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<IProgress<long>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<StorageTransferOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(0));
            Assert.Empty(serializationBinder.AllowedTypes);
        }

        [Fact]
        public async Task WriteAsyncHttpBadRequestFailure()
        {
            InitStorage();

            _client.Setup(e => e.UploadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<BlobHttpHeaders>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<IProgress<long>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<StorageTransferOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.BadRequest, "error", BlobErrorCode.InvalidBlockList.ToString(), null));

            var changes = new Dictionary<string, object> { { "key", new StoreItem() } };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.WriteAsync(changes));
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

            _client.Setup(e => e.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()));

            await _storage.DeleteAsync(new string[] { "key" });

            _client.Verify(e => e.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.ReadAsync(null));
        }

        [Fact]
        public async Task ReadAsync()
        {
            InitStorage();

            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes("{\"ETag\":\"*\"}"));
            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: stream);
            var response = new Mock<Response<BlobDownloadInfo>>();

            response.SetupGet(e => e.Value).Returns(blobDownloadInfo);

            _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response.Object);

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Single(items);
            Assert.Equal("*", JObject.FromObject(items).GetValue("key")?.Value<string>("ETag"));
            _client.Verify(e => e.DownloadAsync(It.IsAny<CancellationToken>()), Times.Once);
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

            InitStorage(jsonSerializerSettings);

            var storeItem = new StoreItem
            {
                ETag = "*"
            };
            var data = JsonConvert.SerializeObject(storeItem, jsonSerializerSettings);
            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: stream);
            var response = new Mock<Response<BlobDownloadInfo>>();

            response.SetupGet(e => e.Value).Returns(blobDownloadInfo);
            
            var blobProperties = BlobsModelFactory.BlobProperties(eTag: new ETag("ETag updated"));
            var properties = new Mock<Response<BlobProperties>>();
            properties.SetupGet(e => e.Value).Returns(blobProperties);
            _client.Setup(e => e.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(properties.Object);
            _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response.Object);

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Single(items);
            Assert.Equal("ETag updated", JObject.FromObject(items).GetValue("key")?.Value<string>("ETag"));
            _client.Verify(e => e.DownloadAsync(It.IsAny<CancellationToken>()), Times.Once);
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

            InitStorage(jsonSerializerSettings);

            var storeItem = new StoreItem
            {
                ETag = "*"
            };
            var data = JsonConvert.SerializeObject(storeItem, jsonSerializerSettings);
            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: stream);
            var response = new Mock<Response<BlobDownloadInfo>>();

            response.SetupGet(e => e.Value).Returns(blobDownloadInfo);
            
            _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.ReadAsync(new string[] { "key" }));

            _client.Verify(e => e.DownloadAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReadAsyncHttpPreconditionFailure()
        {
            InitStorage();

            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes("{\"ETag\":\"*\"}"));
            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: stream);
            var response = new Mock<Response<BlobDownloadInfo>>();

            response.SetupGet(e => e.Value).Returns(blobDownloadInfo);

            _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.PreconditionFailed, "error"))
                .Callback(() =>
                {
                    // Break the retry process.
                    _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(response.Object);
                });

            var items = await _storage.ReadAsync(new string[] { "key" });

            Assert.Single(items);
            Assert.Equal("*", JObject.FromObject(items).GetValue("key")?.Value<string>("ETag"));
            _client.Verify(e => e.DownloadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ReadAsyncHttpNotFoundFailure()
        {
            InitStorage();

            // RequestFailedException => NotFound
            var requestFailedException = new RequestFailedException((int)HttpStatusCode.NotFound, "error");
            _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(requestFailedException)
                .Callback(() =>
                {
                    // AggregateException => RequestFailedException => NotFound
                    _client.Setup(e => e.DownloadAsync(It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new AggregateException(requestFailedException));
                });

            var items = await _storage.ReadAsync(new string[] { "key1", "key2" });

            Assert.Empty(items);
            _client.Verify(e => e.DownloadAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetBlobNameValidation()
        {
            InitStorage();

            // Empty keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteAsync(new string[] { string.Empty }));
        }

        private void InitStorage(JsonSerializerSettings jsonSerializerSettings = default)
        {
            var container = new Mock<BlobContainerClient>();
            var jsonSerializer = jsonSerializerSettings != null ? JsonSerializer.Create(jsonSerializerSettings) : null;

            container.Setup(e => e.GetBlobClient(It.IsAny<string>()))
                .Returns(_client.Object);
            container.Setup(e => e.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));

            _storage = new BlobsStorage(container.Object, jsonSerializer);
        }

        private class StoreItem : IStoreItem
        {
            public int Id { get; set; } = 0;

            public string Topic { get; set; } = "car";

            public string ETag { get; set; }
        }
    }
}
