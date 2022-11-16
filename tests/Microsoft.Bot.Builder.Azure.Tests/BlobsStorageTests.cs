// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Azure;
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
        public async void WriteAsyncValidation()
        {
            InitStorage();

            // No changes. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.WriteAsync(null));
        }

        [Fact]
        public async void WriteAsync()
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
                { "key4", "value1" },
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
                Times.Exactly(4));
        }

        [Fact]
        public async void WriteAsyncHttpBadRequestFailure()
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
        public async void DeleteAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteAsync(null));
        }

        [Fact]
        public async void DeleteAsync()
        {
            InitStorage();

            _client.Setup(e => e.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()));

            await _storage.DeleteAsync(new string[] { "key" });

            _client.Verify(e => e.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ReadAsyncValidation()
        {
            InitStorage();

            // No keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.ReadAsync(null));
        }

        [Fact]
        public async void ReadAsync()
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
        public async void ReadAsyncHttpPreconditionFailure()
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
        public async void ReadAsyncHttpNotFoundFailure()
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
        public async void GetBlobNameValidation()
        {
            InitStorage();

            // Empty keys. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteAsync(new string[] { string.Empty }));
        }

        private void InitStorage()
        {
            var container = new Mock<BlobContainerClient>();

            container.Setup(e => e.GetBlobClient(It.IsAny<string>()))
                .Returns(_client.Object);
            container.Setup(e => e.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));

            _storage = new BlobsStorage(container.Object);
        }

        private class StoreItem : IStoreItem
        {
            public string ETag { get; set; }
        }
    }
}
