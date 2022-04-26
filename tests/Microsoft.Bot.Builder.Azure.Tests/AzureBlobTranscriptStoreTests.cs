// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
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
        private Mock<CloudBlobDirectory> _mockDirectory;
        private Activity _activity;

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

            _activity = new Activity
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

            await _blobTranscript.LogActivityAsync(_activity);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlockBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsyncMessageUpdate()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.MessageUpdate,
                Text = "Hello",
                Id = "test-id",
                ChannelId = "channel-id",
                Conversation = new ConversationAccount { Id = "conversation-id" },
                Timestamp = new DateTimeOffset(),
                From = new ChannelAccount { Id = "account-1" },
                Recipient = new ChannelAccount { Id = "account-2" }
            };

            InitStorage();

            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).Returns(Task.FromResult(JsonConvert.SerializeObject(_activity)));

            await _blobTranscript.LogActivityAsync(_activity);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockBlockBlob.Verify(x => x.DownloadTextAsync(), Times.Exactly(2));
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Exactly(2));
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsyncMessageUpdateNullBlob()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.MessageUpdate,
                Text = "Hello",
                Id = "test-id",
                ChannelId = "channel-id",
                Conversation = new ConversationAccount() { Id = "conversation-id" },
                Timestamp = new DateTimeOffset(),
                From = new ChannelAccount() { Id = "account-1" },
                Recipient = new ChannelAccount() { Id = "account-2" }
            };

            InitStorage();

            var segment = new BlobResultSegment(new List<CloudBlockBlob>(), null);

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                It.IsAny<int>(),
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(segment));

            _mockContainer.Setup(x => x.GetDirectoryReference(It.IsAny<string>())).Returns(_mockDirectory.Object);

            await _blobTranscript.LogActivityAsync(_activity);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetBlockBlobReference(It.IsAny<string>()), Times.Once);
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsyncMessageDelete()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.MessageDelete,
                Text = "Hello",
                Id = "test-id",
                ChannelId = "channel-id",
                Conversation = new ConversationAccount { Id = "conversation-id" },
                Timestamp = new DateTimeOffset(),
                From = new ChannelAccount { Id = "account-1" },
                Recipient = new ChannelAccount { Id = "account-2" }
            };

            InitStorage();

            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).Returns(Task.FromResult(JsonConvert.SerializeObject(_activity)));

            await _blobTranscript.LogActivityAsync(_activity);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockBlockBlob.Verify(x => x.DownloadTextAsync(), Times.Exactly(2));
            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);
            _mockBlockBlob.Verify(x => x.SetMetadataAsync(), Times.Exactly(2));
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
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

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                null,
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(_segment));

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId");

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    null,
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncWithMetadata()
        {
            InitStorage();

            _mockBlockBlob.Object.Metadata["Timestamp"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                null,
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(_segment));

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId");

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    null,
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncContinuationToken()
        {
            InitStorage();

            _mockBlockBlob.Object.Metadata["Timestamp"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _mockBlockBlob.SetupGet(x => x.Name).Returns("token-name");

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                null,
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(_segment));

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId", "token-name");

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    null,
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetTranscriptActivitiesAsyncMultipleBlobs()
        {
            InitStorage();

            var jsonString = JsonConvert.SerializeObject(new Activity());
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).Returns(Task.FromResult(jsonString));
            _mockBlockBlob.Object.Metadata["Timestamp"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _mockBlockBlob.SetupGet(x => x.Name).Returns("token-name");

            var segment = new BlobResultSegment(CreateSegment(21, _mockBlockBlob.Object).ToList(), null);

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                null,
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(segment));

            await _blobTranscript.GetTranscriptActivitiesAsync("channelId", "conversationId", "token-name");

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    null,
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
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

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                null,
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(_segment));

            _mockContainer.Setup(x => x.GetDirectoryReference(It.IsAny<string>())).Returns(_mockDirectory.Object);

            await _blobTranscript.ListTranscriptsAsync("channelId", null);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    null,
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
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

            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                null,
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(_segment));

            await _blobTranscript.DeleteTranscriptAsync("channelId", "convo-id");

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);
            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    null,
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
            _mockBlockBlob.Verify(x => x.DeleteIfExistsAsync(), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsyncInternalFindActivityBlobAsync()
        {
            _activity = new Activity
            {
                Type = ActivityTypes.MessageUpdate,
                Text = "Hello",
                Id = "test-id",
                ChannelId = "channel-id",
                Conversation = new ConversationAccount { Id = "conversation-id" },
                Timestamp = new DateTimeOffset(),
                From = new ChannelAccount { Id = "account-1" },
                Recipient = new ChannelAccount { Id = "account-2" }
            };

            InitStorage();

            _mockBlockBlob.Object.Metadata["Id"] = "test-id";
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).Returns(Task.FromResult(JsonConvert.SerializeObject(_activity)));

            await _blobTranscript.LogActivityAsync(_activity);

            _mockBlobClient.Verify(x => x.GetContainerReference(It.IsAny<string>()), Times.Once);
            _mockContainer.Verify(x => x.GetDirectoryReference(It.IsAny<string>()), Times.Once);

            _mockBlockBlob.Verify(x => x.OpenWriteAsync(), Times.Once);

            _mockDirectory.Verify(
                x => x.ListBlobsSegmentedAsync(
                    It.IsAny<bool>(),
                    It.IsAny<BlobListingDetails>(),
                    It.IsAny<int>(),
                    It.IsAny<BlobContinuationToken>(),
                    It.IsAny<BlobRequestOptions>(),
                    It.IsAny<OperationContext>()), Times.Once);
        }

        private static IEnumerable<CloudBlockBlob> CreateSegment(int count, CloudBlockBlob blob)
        {
            return Enumerable.Range(0, count).Select(x => blob);
        }

        private void InitStorage()
        {
            var jsonString = JsonConvert.SerializeObject(new Activity());

            _stream = new Mock<CloudBlobStream>();
            _stream.SetupGet(x => x.CanWrite).Returns(true);

            _mockBlockBlob = new Mock<CloudBlockBlob>(new Uri("http://test/myaccount/blob"));
            _mockBlockBlob.Setup(x => x.OpenWriteAsync()).Returns(Task.FromResult(_stream.Object));
            _mockBlockBlob.Setup(x => x.SetMetadataAsync());
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).Returns(Task.FromResult(JsonConvert.SerializeObject(_activity)));
            _mockBlockBlob.Setup(x => x.DownloadTextAsync()).Returns(Task.FromResult(jsonString));

            _segment = new BlobResultSegment(new List<CloudBlockBlob> { _mockBlockBlob.Object }, null);

            _mockDirectory = new Mock<CloudBlobDirectory>();
            _mockDirectory.Setup(x => x.ListBlobsSegmentedAsync(
                It.IsAny<bool>(),
                It.IsAny<BlobListingDetails>(),
                It.IsAny<int>(),
                It.IsAny<BlobContinuationToken>(),
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>())).Returns(Task.FromResult(_segment));

            _mockContainer = new Mock<CloudBlobContainer>(new Uri("https://testuri.com"));
            _mockContainer.Setup(x => x.GetBlockBlobReference(It.IsAny<string>())).Returns(_mockBlockBlob.Object);
            _mockContainer.Setup(x => x.CreateIfNotExistsAsync());
            _mockContainer.Setup(x => x.GetDirectoryReference(It.IsAny<string>())).Returns(_mockDirectory.Object);

            _mockBlobClient = new Mock<CloudBlobClient>(new Uri("https://testuri.com"), null);
            _mockBlobClient.Setup(x => x.GetContainerReference(It.IsAny<string>())).Returns(_mockContainer.Object);

            _mockAccount = new Mock<CloudStorageAccount>(new StorageCredentials("accountName", "S2V5VmFsdWU=", "key"), false);

            _blobTranscript = new AzureBlobTranscriptStore(_mockAccount.Object, ContainerName, _mockBlobClient.Object);
        }
    }
}
