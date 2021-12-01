// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests.Common.Storage;
using Microsoft.Bot.Schema;
using Xunit;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Blobs.Tests
{
    /// <summary>
    /// Base tests for <seealso cref="BlobsStorageTests"/> and <seealso cref="AzureBlobStorageTests"/>.
    /// </summary>
    public abstract class BlobStorageBaseTests : StorageTestsBase
    {
        protected const string ConnectionString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        protected abstract string ContainerName { get; }

        [Fact]
        public async Task TestBlobStorageWriteRead()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                var changes = new Dictionary<string, object>
                {
                    { "x", "hello" },
                    { "y", "world" },
                };

                // Act
                await storage.WriteAsync(changes);
                var result = await storage.ReadAsync(new[] { "x", "y" });

                // Assert
                Assert.Equal(2, result.Count);
                Assert.Equal("hello", result["x"]);
                Assert.Equal("world", result["y"]);
            }
        }

        [Fact]
        public async Task TestBlobStorageWriteWithNullChangesShouldFail()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => storage.WriteAsync(null));
            }
        }

        [Fact]
        public async Task TestBlobStorageWriteWithEmptyKeyChangesShouldFail()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                var changes = new Dictionary<string, object>
                {
                    { string.Empty, "hello" },
                };

                // Act
                await Assert.ThrowsAsync<ArgumentNullException>(() => storage.WriteAsync(changes));
            }
        }

        [Fact]
        public async Task TestBlobStorageWriteReadWithNullKeysShouldFail()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                var changes = new Dictionary<string, object>
                {
                    { "x", "hello" },
                    { "y", "world" },
                };

                // Act
                await storage.WriteAsync(changes);

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => storage.ReadAsync(null));
            }
        }

        [Fact]
        public async Task TestBlobStorageWriteDeleteRead()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                var changes = new Dictionary<string, object>
                {
                    { "x", "hello" },
                    { "y", "world" },
                };

                // Act
                await storage.WriteAsync(changes);
                await storage.DeleteAsync(new[] { "x" });
                var result = await storage.ReadAsync(new[] { "x", "y" });

                // Assert
                Assert.Equal(1, result.Count);
                Assert.Equal("world", result["y"]);
            }
        }

        [Fact]
        public async Task TestBlobStorageWriteDeleteWithNullKeysShouldFail()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                var changes = new Dictionary<string, object>
                {
                    { "x", "hello" },
                    { "y", "world" },
                };

                // Act
                await storage.WriteAsync(changes);

                // Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => storage.DeleteAsync(null));
            }
        }

        [Fact]
        public async Task TestBlobStorageChanges()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();

                // Act
                await storage.WriteAsync(new Dictionary<string, object> { { "a", "1.0" }, { "b", "2.0" } });
                await storage.WriteAsync(new Dictionary<string, object> { { "c", "3.0" } });
                await storage.DeleteAsync(new[] { "b" });
                await storage.WriteAsync(new Dictionary<string, object> { { "a", "1.1" } });
                var result = await storage.ReadAsync(new[] { "a", "b", "c", "d", "e" });

                // Assert
                Assert.Equal(2, result.Count);
                Assert.Equal("1.1", result["a"]);
                Assert.Equal("3.0", result["c"]);
            }
        }

        [Fact]
        public async Task TestConversationStateBlobStorage()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var storage = GetStorage();
                var conversationState = new ConversationState(storage);
                var propAccessor = conversationState.CreateProperty<Prop>("prop");

                var adapter = new TestStorageAdapter();
                var activity = new Activity
                {
                    ChannelId = "123",
                    Conversation = new ConversationAccount { Id = "abc" },
                };

                // Act
                var turnContext1 = new TurnContext(adapter, activity);
                var propValue1 = await propAccessor.GetAsync(turnContext1, () => new Prop());
                propValue1.X = "hello";
                propValue1.Y = "world";
                await conversationState.SaveChangesAsync(turnContext1, force: true);

                var turnContext2 = new TurnContext(adapter, activity);
                var propValue2 = await propAccessor.GetAsync(turnContext2);

                // Assert
                Assert.Equal("hello", propValue2.X);
                Assert.Equal("world", propValue2.Y);

                await propAccessor.DeleteAsync(turnContext1);
                await conversationState.SaveChangesAsync(turnContext1);
            }
        }

        [Fact]
        public async Task TestConversationStateBlobStorage_TypeNameHandlingDefault()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await TestConversationStateBlobStorage_Method(GetStorage());
            }
        }

        [Fact]
        public async Task TestConversationStateBlobStorage_TypeNameHandlingNone()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await TestConversationStateBlobStorage_Method(GetStorage(true));
            }
        }

        [Fact]
        public async Task StatePersistsThroughMultiTurn_TypeNameHandlingNone()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                await StatePersistsThroughMultiTurn(GetStorage(true));
            }
        }

        protected abstract IStorage GetStorage(bool typeNameHandlingNone = false);

        private async Task TestConversationStateBlobStorage_Method(IStorage blobs)
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                // Arrange
                var conversationState = new ConversationState(blobs);
                var propAccessor = conversationState.CreateProperty<Prop>("prop");
                var adapter = new TestStorageAdapter();
                var activity = new Activity
                {
                    ChannelId = "123",
                    Conversation = new ConversationAccount { Id = "abc" },
                };

                // Act
                var turnContext1 = new TurnContext(adapter, activity);
                var propValue1 = await propAccessor.GetAsync(turnContext1, () => new Prop());
                propValue1.X = "hello";
                propValue1.Y = "world";
                await conversationState.SaveChangesAsync(turnContext1, force: true);

                var turnContext2 = new TurnContext(adapter, activity);
                var propValue2 = await propAccessor.GetAsync(turnContext2);

                // Assert
                Assert.Equal("hello", propValue2.X);
                Assert.Equal("world", propValue2.Y);
            }
        }

        private class TestStorageAdapter : BotAdapter
        {
            public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class Prop : IStoreItem
        {
            public string X { get; set; }

            public string Y { get; set; }

            string IStoreItem.ETag { get; set; }
        }
    }
}
