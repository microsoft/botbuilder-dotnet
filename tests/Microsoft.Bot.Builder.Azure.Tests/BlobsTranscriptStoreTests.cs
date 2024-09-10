// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - BlobsTranscriptStore")]
    public class BlobsTranscriptStoreTests
    {
        private const string ConnectionString = @"UseDevelopmentStorage=true";

        private BlobsTranscriptStore _storage;
        private readonly Mock<BlobClient> _client = new Mock<BlobClient>();
        private readonly Mock<BlobContainerClient> _container = new Mock<BlobContainerClient>();
        private readonly Activity _activity = new Activity
        {
            Type = ActivityTypes.Message,
            Text = "Text",
            Id = "Id",
            ChannelId = "ChannelId",
            Conversation = new ConversationAccount { Id = "Conversation-Id" },
            Timestamp = new DateTimeOffset(),
            From = new ChannelAccount { Id = "From-Id" },
            Recipient = new ChannelAccount { Id = "Recipient-Id" }
        };

        [Fact]
        public void ConstructorValidation()
        {
            // Should work.
            _ = new BlobsTranscriptStore(
                ConnectionString,
                "containerName",
                JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

            var blobServiceUri = new Uri("https://storage.net/");

            var mockCredential = new Mock<TokenCredential>();
            mockCredential
                .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));

            // No dataConnectionString. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(null, "containerName"));
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(string.Empty, "containerName"));

            // No containerName. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(ConnectionString, null));
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(ConnectionString, string.Empty));

            // No URI. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(blobServiceUri: null, mockCredential.Object, "containerName"));

            // No tokenCredential. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(blobServiceUri, null, "containerName"));
            
            // No containerName. Should throw.
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(blobServiceUri, mockCredential.Object, null));
            Assert.Throws<ArgumentNullException>(() => new BlobsTranscriptStore(blobServiceUri, mockCredential.Object, string.Empty));
        }

        [Fact]
        public async void LogActivityAsync()
        {
            InitStorage();

            await _storage.LogActivityAsync(_activity);

            _client.Verify(e => e.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void LogActivityAsyncMessageUpdate()
        {
            InitStorage();

            _activity.Type = ActivityTypes.MessageUpdate;

            await _storage.LogActivityAsync(_activity);

            _client.Verify(e => e.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void LogActivityAsyncMessageUpdateNotFound()
        {
            InitStorage();

            _activity.Type = ActivityTypes.MessageUpdate;

            Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(string.Empty));
            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: stream);
            var response = new Mock<Response<BlobDownloadInfo>>();

            response.SetupGet(e => e.Value).Returns(blobDownloadInfo);
            _client.Setup(e => e.DownloadAsync()).ReturnsAsync(response.Object);

            await _storage.LogActivityAsync(_activity);

            _client.Verify(e => e.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _client.Verify(e => e.DownloadAsync(), Times.Once);
        }

        [Fact]
        public async void LogActivityAsyncMessageDelete()
        {
            InitStorage();

            _activity.Type = ActivityTypes.MessageDelete;

            await _storage.LogActivityAsync(_activity);

            _client.Verify(e => e.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void LogActivityAsyncContinuationToken()
        {
            InitStorage();

            var continuationPage = GeneratePage(0, "0");
            var finalPage = GeneratePage(0);
            var processed = false;

            _container.Setup(e => e.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    if (processed)
                    {
                        return finalPage.Object;
                    }

                    processed = true;
                    return continuationPage.Object;
                });

            _activity.Type = ActivityTypes.MessageUpdate;

            await _storage.LogActivityAsync(_activity);

            _client.Verify(e => e.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void LogActivityAsyncHttpPreconditionFailure()
        {
            InitStorage();
            var precondition = true;

            _container.Setup(e => e.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    if (!precondition)
                    {
                        throw new Exception("error");
                    }

                    precondition = false;
                    throw new RequestFailedException((int)HttpStatusCode.PreconditionFailed, "error");
                });

            _activity.Type = ActivityTypes.MessageUpdate;

            await Assert.ThrowsAsync<Exception>(() => _storage.LogActivityAsync(_activity));

            _container.Verify(e => e.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async void GetTranscriptActivitiesAsyncValidation()
        {
            InitStorage();

            // No channelId. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.GetTranscriptActivitiesAsync(null, "conversationId"));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.GetTranscriptActivitiesAsync(string.Empty, "conversationId"));

            // No conversationId. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.GetTranscriptActivitiesAsync("channelId", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.GetTranscriptActivitiesAsync("channelId", string.Empty));
        }

        [Fact]
        public async void GetTranscriptActivitiesAsync()
        {
            InitStorage();

            var pageResult = await _storage.GetTranscriptActivitiesAsync("channelId", "conversationId");

            Assert.NotNull(pageResult);
            Assert.Single(pageResult.Items);
            Assert.Equal(_activity.Id, pageResult.Items[0].Id);
            _client.Verify(e => e.DownloadAsync(), Times.Once);
        }

        [Fact]
        public async void GetTranscriptActivitiesAsyncContinuationToken()
        {
            InitStorage();

            var pageResult = await _storage.GetTranscriptActivitiesAsync("channelId", "conversationId", "1");

            Assert.NotNull(pageResult);
            Assert.Empty(pageResult.Items);
            _client.Verify(e => e.DownloadAsync(), Times.Never);
        }

        [Fact]
        public async void GetTranscriptActivitiesAsyncFullPageSize()
        {
            const int pageSize = 20;
            InitStorage(20);

            var pageResult = await _storage.GetTranscriptActivitiesAsync("channelId", "conversationId");

            Assert.NotNull(pageResult);
            Assert.Equal(pageSize, pageResult.Items.Length);
            Assert.Equal(pageSize.ToString(), pageResult.ContinuationToken);
            _client.Verify(e => e.DownloadAsync(), Times.Exactly(20));
        }

        [Fact]
        public async void ListTranscriptsAsyncValidation()
        {
            InitStorage();

            // No channelId. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.ListTranscriptsAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.ListTranscriptsAsync(string.Empty));
        }

        [Fact]
        public async void ListTranscriptsAsync()
        {
            InitStorage();

            var pageResult = await _storage.ListTranscriptsAsync("channelId");

            Assert.NotNull(pageResult);
            Assert.Single(pageResult.Items);
            Assert.Equal("1", pageResult.Items[0].Id);
        }

        [Fact]
        public async void ListTranscriptsAsyncContinuationToken()
        {
            InitStorage();

            var pageResult = await _storage.ListTranscriptsAsync("channelId", "1");

            Assert.NotNull(pageResult);
            Assert.Empty(pageResult.Items);
            _client.Verify(e => e.DownloadAsync(), Times.Never);
        }

        [Fact]
        public async void ListTranscriptsAsyncFullPageSize()
        {
            const int pageSize = 20;
            InitStorage(pageSize);

            var pageResult = await _storage.ListTranscriptsAsync("channelId");

            Assert.NotNull(pageResult);
            Assert.Equal(pageSize, pageResult.Items.Length);
            Assert.Equal(pageSize.ToString(), pageResult.ContinuationToken);
        }

        [Fact]
        public async void DeleteTranscriptAsyncValidation()
        {
            InitStorage();

            // No channelId. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteTranscriptAsync(null, "conversationId"));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteTranscriptAsync(string.Empty, "conversationId"));

            // No conversationId. Should throw.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteTranscriptAsync("channelId", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _storage.DeleteTranscriptAsync("channelId", string.Empty));
        }

        [Fact]
        public async void DeleteTranscriptAsync()
        {
            InitStorage();

            _client.Setup(e => e.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()));

            await _storage.DeleteTranscriptAsync("channelId", "conversationId");

            _client.Verify(e => e.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private void InitStorage(int pageSize = 1)
        {
            var page = GeneratePage(pageSize);

            _container.Setup(e => e.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(page.Object);
            _container.Setup(e => e.GetBlobClient(It.IsAny<string>()))
                .Returns(_client.Object);
            _client.Setup(e => e.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()));
            _client.Setup(e => e.DownloadAsync())
                .ReturnsAsync(() =>
                {
                    Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(_activity)));
                    var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: stream);
                    var response = new Mock<Response<BlobDownloadInfo>>();

                    response.SetupGet(e => e.Value).Returns(blobDownloadInfo);
                    return response.Object;
                });

            _storage = new BlobsTranscriptStore(_container.Object);
        }

        private Mock<AsyncPageable<BlobItem>> GeneratePage(int pageSize = 1, string continuationToken = default)
        {
            var page = new Mock<Page<BlobItem>>();
            var asyncPageable = new Mock<AsyncPageable<BlobItem>>();

            async IAsyncEnumerable<Page<BlobItem>> GenerateItems()
            {
                yield return await Task.FromResult(page.Object);
            }

            var items = GenerateItems();
            var blobItems = GenerateBlobItems(pageSize);

            page.SetupGet(e => e.ContinuationToken).Returns(continuationToken);
            page.SetupGet(e => e.Values).Returns(blobItems);

            asyncPageable.Setup(e => e.AsPages(It.IsAny<string>(), It.IsAny<int?>()))
                .Returns(items);

            return asyncPageable;
        }

        private IReadOnlyList<BlobItem> GenerateBlobItems(int pageSize = 1)
        {
            var blobItems = new List<BlobItem>();
            for (var i = 1; i <= pageSize; i++)
            {
                var blobItem = BlobsModelFactory.BlobItem(
                    name: i.ToString(),
                    metadata: new Dictionary<string, string>
                    {
                        { "Id", _activity.Id },
                        { "Timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture) }
                    });
                blobItems.Add(blobItem);
            }

            return blobItems;
        }
    }
}
