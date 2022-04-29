// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - BlobTranscripts")]
    public class AzureBlobTranscriptStoreTests
    {
        protected const string ConnectionString = @"UseDevelopmentStorage=true";
        protected const string ContainerName = "containername";

        private Mock<CloudBlobStream> _stream;
        private Mock<CloudBlockBlob> _mockBlockBlob;
        private Mock<CloudBlobContainer> _mockContainer;
        private Mock<CloudBlobClient> _mockBlobClient;
        private Mock<CloudStorageAccount> _mockAccount;
        private AzureBlobTranscriptStore _blobTranscript;
        private BlobResultSegment _segment;

        private readonly Activity _activity = new Activity
        {
            Type = ActivityTypes.Message,
            Text = "Hello",
            Id = "test-id",
            ChannelId = "channel-id",
            Conversation = new ConversationAccount() { Id = "conversation-id" },
            Timestamp = new DateTimeOffset(),
            From = new ChannelAccount() { Id = "account-1" },
            Recipient = new ChannelAccount() { Id = "account-2" }
        };

        [Fact]
        public void ConstructorValidation()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);

            // Should work.
            Assert.NotNull(new AzureBlobTranscriptStore(ConnectionString, ContainerName));
            Assert.NotNull(new AzureBlobTranscriptStore(storageAccount, ContainerName));

            // No storageAccount. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureBlobTranscriptStore(storageAccount: null, ContainerName));

            // No containerName. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureBlobTranscriptStore(storageAccount, null));

            // Wrong format. Should throw.
            Assert.Throws<FormatException>(() => new AzureBlobTranscriptStore("123", ContainerName));
        }

        [Fact]
        public async Task LogActivityAsyncNullActivityFailure()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var blobTranscript = new AzureBlobTranscriptStore(storageAccount, ContainerName);

            await Assert.ThrowsAsync<ArgumentNullException>(() => blobTranscript.LogActivityAsync(null));
        }

        [Fact]
        public async Task LogActivityAsyncDefault()
        {
            InitStorage();

            await _blobTranscript.LogActivityAsync(_activity);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlockBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsyncMessageUpdate()
        {
            _activity.Type = ActivityTypes.MessageUpdate;

            InitStorage();

            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(JsonConvert.SerializeObject(_activity));

            await _blobTranscript.LogActivityAsync(_activity);

            BaseVerification();

            _mockBlockBlob.Verify(x => x.DownloadTextAsync(), Times.Exactly(2));
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task LogActivityAsyncMessageUpdateNullBlob()
        {
            _activity.Type = ActivityTypes.MessageUpdate;

            InitStorage();

            var segment = new BlobResultSegment(new List<CloudBlockBlob>(), null);

            _mockContainer.Setup(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int?>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(segment);

            await _blobTranscript.LogActivityAsync(_activity);

            BaseVerification();

            _mockContainer.Verify(x => x.GetBlockBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsyncMessageDelete()
        {
            _activity.Type = ActivityTypes.MessageDelete;

            InitStorage();

            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(JsonConvert.SerializeObject(_activity));

            await _blobTranscript.LogActivityAsync(_activity);

            BaseVerification();

            _mockBlockBlob.Verify(x => x.DownloadTextAsync(), Times.Exactly(2));
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task LogActivityAsyncInternalFindActivityBlobAsync()
        {
            _activity.Type = ActivityTypes.MessageUpdate;

            InitStorage();

            _mockBlockBlob.Object.Metadata["Id"] = "test-id";
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(JsonConvert.SerializeObject(_activity));

            await _blobTranscript.LogActivityAsync(_activity);

            BaseVerification();

            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncValidations()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var blobTranscript = new AzureBlobTranscriptStore(storageAccount, ContainerName);

            // No channel id. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => blobTranscript.GetTranscriptActivitiesAsync(null, "convo-id"));

            // No conversation id. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => blobTranscript.GetTranscriptActivitiesAsync("channel-id", null));
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsync()
        {
            InitStorage();

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId");

            BaseVerification();
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncWithMetadata()
        {
            InitStorage();

            _mockBlockBlob.Object.Metadata["Timestamp"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId");

            BaseVerification();
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncContinuationToken()
        {
            InitStorage();

            _mockBlockBlob.Object.Metadata["Timestamp"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _mockBlockBlob.SetupGet(x => x.Name).Returns("token-name");

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId", "token-name");

            BaseVerification();
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncMultipleBlobs()
        {
            InitStorage();

            var jsonString = JsonConvert.SerializeObject(new Activity());
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(jsonString);
            _mockBlockBlob.Object.Metadata["Timestamp"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _mockBlockBlob.SetupGet(x => x.Name).Returns("token-name");

            var segment = new BlobResultSegment(CreateSegment(21, _mockBlockBlob.Object).ToList(), null);

            _mockContainer.Setup(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int?>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(segment);

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId", "token-name");

            BaseVerification();
        }

        [Fact]
        public async Task ListTranscriptAsyncValidations()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var blobTranscript = new AzureBlobTranscriptStore(storageAccount, ContainerName);

            // No channel id. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => blobTranscript.ListTranscriptsAsync(null));
        }

        [Fact]
        public async Task ListTranscriptAsync()
        {
            InitStorage();

            await _blobTranscript.ListTranscriptsAsync("channelId", null);

            BaseVerification();
        }

        [Fact]
        public async Task DeleteTranscriptAsyncValidations()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var blobTranscript = new AzureBlobTranscriptStore(storageAccount, ContainerName);

            // No channel id. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => blobTranscript.DeleteTranscriptAsync(null, "convo-id"));

            // No conversation id. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => blobTranscript.DeleteTranscriptAsync("channel-id", null));
        }

        [Fact]
        public async Task DeleteTranscriptAsync()
        {
            InitStorage();

            await _blobTranscript.DeleteTranscriptAsync("channelId", "convo-id");

            BaseVerification();

            _mockBlockBlob.Verify(x => x.DeleteIfExistsAsync(), Times.Once);
        }

        private static IEnumerable<CloudBlockBlob> CreateSegment(int count, CloudBlockBlob blob)
        {
            return Enumerable.Range(0, count).Select(x => blob);
        }

        private void BaseVerification()
        {
            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int?>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private void InitStorage()
        {
            var jsonString = JsonConvert.SerializeObject(new Activity());

            _stream = new Mock<CloudBlobStream>();
            _stream.SetupGet(x => x.CanWrite).Returns(true);

            _mockBlockBlob = new Mock<CloudBlockBlob>(new Uri("http://test/myaccount/blob"));
            _mockBlockBlob.Setup(x => x.OpenWriteAsync()).ReturnsAsync(_stream.Object);
            _mockBlockBlob.Setup(x => x.SetMetadataAsync());
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(JsonConvert.SerializeObject(_activity));
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).ReturnsAsync(jsonString);

            _segment = new BlobResultSegment(new List<CloudBlockBlob> { _mockBlockBlob.Object }, null);

            _mockContainer = new Mock<CloudBlobContainer>(new Uri("https://testuri.com"));
            _mockContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>())).Returns(_mockBlockBlob.Object);
            _mockContainer.Setup(x => x.CreateIfNotExistsAsync());
            _mockContainer.Setup(x => x.GetDirectoryReference(It.IsAny<string>())).CallBase();
            _mockContainer.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                It.IsAny<int?>(),
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(_segment);

            _mockBlobClient = new Mock<CloudBlobClient>(new Uri("https://testuri.com"), null);
            _mockBlobClient.Setup(x => x.GetContainerReference(It.IsAny<string>())).Returns(_mockContainer.Object);

            _mockAccount = new Mock<CloudStorageAccount>(new StorageCredentials("accountName", "S2V5VmFsdWU=", "key"), false);

            _blobTranscript = new AzureBlobTranscriptStore(_mockAccount.Object, ContainerName, _mockBlobClient.Object);
        }
    }
}
