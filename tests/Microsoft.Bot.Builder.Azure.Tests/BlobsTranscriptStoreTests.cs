// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - BlobsTranscriptStore")]
    public class BlobsTranscriptStoreTests : TranscriptStoreBaseTests, IAsyncLifetime
    {
        private readonly string _testName;

        public BlobsTranscriptStoreTests(ITestOutputHelper testOutputHelper)
        {
            var helper = (TestOutputHelper)testOutputHelper;

            var test = (ITest)helper.GetType().GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(helper);

            _testName = test.TestCase.TestMethod.Method.Name;

            if (StorageEmulatorHelper.CheckEmulator())
            {
                new BlobContainerClient(BlobStorageEmulatorConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        protected override string ContainerName => $"blobstranscript{_testName.ToLower()}";

        protected override ITranscriptStore TranscriptStore => new BlobsTranscriptStore(BlobStorageEmulatorConnectionString, ContainerName);

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await new BlobContainerClient(BlobStorageEmulatorConnectionString, ContainerName)
                    .DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [Fact]
        public async Task LongIdAddTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                try
                {
                    var a = CreateActivity(0, 0, LongId);

                    await TranscriptStore.LogActivityAsync(a);
                    throw new XunitException("Should have thrown an error");
                }
                catch (System.Xml.XmlException xmlEx)
                {
                    // Unfortunately, Azure.Storage.Blobs v12.4.4 currently throws this XmlException for long keys :(
                    if (xmlEx.Message == "'\"' is an unexpected token. Expecting whitespace. Line 1, position 50.")
                    {
                        return;
                    }
                }

                throw new XunitException("Should have thrown an error");
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [Fact]
        public void BlobTranscriptParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsTranscriptStore(null, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsTranscriptStore(BlobStorageEmulatorConnectionString, null));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsTranscriptStore(string.Empty, ContainerName));

                Assert.Throws<ArgumentNullException>(() =>
                    new BlobsTranscriptStore(BlobStorageEmulatorConnectionString, string.Empty));
            }
        }

        [Fact]
        public async Task GenericActivityOverwriteThrowsTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var conversationId = "GenericActivityOverwriteThrowsTest";

                var activity = CreateActivity(99, conversationId);

                await TranscriptStore.LogActivityAsync(activity);
                var loggedActivities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, conversationId);

                Assert.Equal("100", loggedActivities.Items[0].Id);

                await Assert.ThrowsAsync<RequestFailedException>(async () => await TranscriptStore.LogActivityAsync(activity));
            }
        }

        [Fact]
        public async Task UpdateActivityOverwriteDoesNotThrowTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var conversationId = "UpdateActivityOverwriteDoesNotThrowTest";

                var activity = CreateActivity(99, conversationId);
                activity.ChannelData = new
                {
                    value = "original"
                };

                await TranscriptStore.LogActivityAsync(activity);
                var loggedActivities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, conversationId);

                Assert.Equal("100", loggedActivities.Items[0].Id);
                Assert.Equal("original", (loggedActivities.Items[0].ChannelData as JToken)["value"]);

                activity.Type = ActivityTypes.MessageUpdate;
                activity.ChannelData = new
                {
                    value = "overwritten"
                };

                await TranscriptStore.LogActivityAsync(activity);
                loggedActivities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, conversationId);

                Assert.Single(loggedActivities.Items);
                Assert.Equal("overwritten", (loggedActivities.Items[0].ChannelData as JToken)["value"]);
            }
        }

        [Fact]
        public async Task TombstonedActivityOverwriteDoesNotThrowTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var conversationId = "TombstonedActivityOverwriteDoesNotThrowTest";

                var activity = CreateActivity(99, conversationId);

                await TranscriptStore.LogActivityAsync(activity);
                var loggedActivities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, conversationId);

                Assert.Equal("100", loggedActivities.Items[0].Id);

                activity.Type = ActivityTypes.MessageDelete;

                await TranscriptStore.LogActivityAsync(activity);
                loggedActivities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, conversationId);

                Assert.Single(loggedActivities.Items);
                Assert.Equal("deleted", loggedActivities.Items[0].From.Id);
            }
        }
    }
}
