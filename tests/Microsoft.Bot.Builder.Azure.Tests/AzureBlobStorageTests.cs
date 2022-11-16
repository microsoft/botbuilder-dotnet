// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Core;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class AzureBlobStorageTests
    {
        protected const string ConnectionString = @"UseDevelopmentStorage=true";
        protected const string ContainerName = "containername";

        private AzureBlobStorage _blobStorage;
        private Mock<CloudBlob> _mockBlob;
        private Mock<CloudBlobContainer> _mockContainer;
        private Mock<CloudBlobClient> _mockBlobClient;
        private Mock<CloudStorageAccount> _mockAccount;
        private Mock<CloudBlockBlob> _mockBlockBlob;

        [Fact]
        public void ConstructorValidation()
        {
            // Should work.
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);

            Assert.NotNull(new AzureBlobStorage(ConnectionString, ContainerName));
            Assert.NotNull(new AzureBlobStorage(storageAccount, ContainerName));
            Assert.NotNull(new AzureBlobStorage(storageAccount, ContainerName, new JsonSerializer()));
            Assert.NotNull(new AzureBlobStorage(storageAccount, ContainerName, new CloudBlobClient(new Uri("http://mytest"))));

            // No JsonSerializer. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureBlobStorage(storageAccount, ContainerName, jsonSerializer: null));

            // No storageAccount. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureBlobStorage(null, ContainerName, new JsonSerializer()));

            // No containerName. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureBlobStorage(storageAccount, null, new JsonSerializer()));
        }

        [Fact]
        public async Task DeleteAsyncNullKeysFailure()
        {
            _blobStorage = new AzureBlobStorage(ConnectionString, ContainerName);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _blobStorage.DeleteAsync(null));
        }

        [Fact]
        public async Task DeleteAsyncEmptyKeysFailure()
        {
            var keys = new[] { string.Empty };

            InitStorage();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _blobStorage.DeleteAsync(keys, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAsyncTwoKeys()
        {
            var keys = new[] { "key1", "key2" };

            InitStorage();

            await _blobStorage.DeleteAsync(keys, CancellationToken.None);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlobReference(It.IsAny<string>()), Times.Exactly(2));
            _mockBlob.Verify(x => x.DeleteIfExistsAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ReadAsyncNullKeysFailure()
        {
            _blobStorage = new AzureBlobStorage(ConnectionString, ContainerName);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _blobStorage.ReadAsync(null));
        }

        [Fact]
        public async Task ReadAsyncOneKey()
        {
            var keys = new[] { "key1" };
            
            InitStorage();

            var items = await _blobStorage.ReadAsync(keys, CancellationToken.None);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlob.Verify(
                x => x.OpenReadAsync(
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(1, items.Count);            
            Assert.True(items.ContainsKey(keys[0]));
        }

        [Fact]
        public async Task ReadAsyncContinueOnNotFoundException()
        {
            var keys = new[] { "key1" };
            var result = new RequestResult { HttpStatusCode = 404 };

            InitStorage();

            _mockBlob.Setup(x => x.OpenReadAsync(
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>())).Throws(new StorageException(result, "exception thrown", new Exception()));

            var items = await _blobStorage.ReadAsync(keys, CancellationToken.None);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlob.Verify(
                x => x.OpenReadAsync(
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(0, items.Count);
        }

        [Fact]
        public async Task ReadAsyncContinueOnAggregateException()
        {
            var keys = new[] { "key1" };
            var result = new RequestResult { HttpStatusCode = 404 };

            InitStorage();
            
            _mockBlob.Setup(x => x.OpenReadAsync(
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>())).Throws(new AggregateException(new StorageException(result, "exception thrown", new Exception())));

            var items = await _blobStorage.ReadAsync(keys, CancellationToken.None);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlob.Verify(
                x => x.OpenReadAsync(
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(0, items.Count);
        }

        [Fact]
        public async Task WriteAsyncNullChangesFailure()
        {
            _blobStorage = new AzureBlobStorage(ConnectionString, ContainerName);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _blobStorage.WriteAsync(null));
        }

        [Fact]
        public async Task WriteAsyncMultipleChanges()
        {
            var changes = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 0 },
                { "key3", true },
                { "key4", new StoreItem() },
                { "key5", new List<StoreItem>() { new StoreItem() } },
                { "key6", new Activity() }
            };

            InitStorage();

            await _blobStorage.WriteAsync(changes, CancellationToken.None);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlockBlobReference(It.IsAny<string>()), Times.Exactly(6));
            _mockBlockBlob.Verify(
                x => x.UploadFromStreamAsync(
                    It.IsAny<MultiBufferMemoryStream>(),
                    It.IsAny<AccessCondition>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(6));
        }

        [Fact]
        public async Task WriteAsyncFailure()
        {
            var changes = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            var result = new RequestResult { HttpStatusCode = 400 };

            InitStorage();
            
            _mockBlockBlob.Setup(x => x.UploadFromStreamAsync(
                It.IsAny<MultiBufferMemoryStream>(),
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>())).Throws(new StorageException(result, "exception thrown", new Exception()));

            await Assert.ThrowsAsync<StorageException>(() => _blobStorage.WriteAsync(changes, CancellationToken.None));
        }

        private void InitStorage()
        {
            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes("{\"Id\":0,\"Topic\":\"car\"}"));

            _mockBlob = new Mock<CloudBlob>(new Uri("http://test/myaccount/blob"));
            _mockBlob.Setup(x => x.DeleteIfExistsAsync(It.IsAny<CancellationToken>()));
            _mockBlob.Setup(x => x.OpenReadAsync(
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>())).Returns(Task.FromResult(stream));

            _mockBlockBlob = new Mock<CloudBlockBlob>(new Uri("http://test/myaccount/blob"));
            _mockBlockBlob.Setup(x => x.UploadFromStreamAsync(
                It.IsAny<MultiBufferMemoryStream>(),
                It.IsAny<AccessCondition>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>()));

            _mockContainer = new Mock<CloudBlobContainer>(new Uri("https://testuri.com"));
            _mockContainer.Setup(x => x.GetBlobReference(It.IsAny<string>())).Returns(_mockBlob.Object);
            _mockContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>())).Returns(_mockBlockBlob.Object);

            _mockBlobClient = new Mock<CloudBlobClient>(new Uri("https://testuri.com"), null);
            _mockBlobClient.Setup(x => x.GetContainerReference(It.IsAny<string>())).Returns(_mockContainer.Object);

            _mockAccount = new Mock<CloudStorageAccount>(new StorageCredentials("accountName", "S2V5VmFsdWU=", "key"), false);

            var jsonSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Objects, // lgtm [cs/unsafe-type-name-handling]
                MaxDepth = null,
                SerializationBinder = new AllowedTypesSerializationBinder(
                    new List<Type>
                    {
                        typeof(IStoreItem),
                        typeof(Dictionary<string, object>),
                        typeof(Activity)
                    }),
            };

            _blobStorage = new AzureBlobStorage(_mockAccount.Object, ContainerName, _mockBlobClient.Object, jsonSerializer);
        }
        
        private class StoreItem : IStoreItem
        {
            public string ETag { get; set; }
        }
    }
}
