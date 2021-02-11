// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - CosmosDB Partitioned")]
    public class CosmosDbPartitionStorageTests : StorageBaseTests, IAsyncLifetime, IClassFixture<CosmosDbPartitionStorageFixture>
    {
        // Endpoint and Authkey for the CosmosDB Emulator running locally
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "test-CosmosDbPartitionStorageTests";
        private const string CosmosCollectionName = "bot-storage";
        private IStorage _storage;

        public CosmosDbPartitionStorageTests()
        {
            _storage = new CosmosDbPartitionedStorage(
                new CosmosDbPartitionedStorageOptions
                {
                    AuthKey = CosmosAuthKey,
                    ContainerId = CosmosCollectionName,
                    CosmosDbEndpoint = CosmosServiceEndpoint,
                    DatabaseId = CosmosDatabaseName,
                });
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _storage = null;
        }

        [Fact]
        public void Constructor_Should_Throw_On_InvalidOptions()
        {
            // No Options. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbPartitionedStorage(null));

            // No Endpoint. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "test",
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = null,
            }));

            // No Auth Key. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = null,
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
            }));

            // No Database Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = "testId",
                DatabaseId = null,
                CosmosDbEndpoint = "testEndpoint",
            }));

            // No Container Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = null,
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
            }));

            // Invalid Row Key characters in KeySuffix
            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
                KeySuffix = "?#*test",
                CompatibilityMode = false
            }));

            Assert.Throws<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
                KeySuffix = "thisisatest",
                CompatibilityMode = true
            }));
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task CreateObjectCosmosDBPartitionTest()
        {
            await CreateObjectTest(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task ReadUnknownCosmosDBPartitionTest()
        {
            await ReadUnknownTest(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task UpdateObjectCosmosDBPartitionTest()
        {
            await UpdateObjectTest<CosmosException>(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task DeleteObjectCosmosDBPartitionTest()
        {
            await DeleteObjectTest(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task DeleteUnknownObjectCosmosDBPartitionTest()
        {
            await _storage.DeleteAsync(new[] { "unknown_delete" });
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task HandleCrazyKeysCosmosDBPartition()
        {
            await HandleCrazyKeys(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task ReadingEmptyKeysReturnsEmptyDictionary()
        {
            var state = await _storage.ReadAsync(new string[] { });
            Assert.IsType<Dictionary<string, object>>(state);
            Assert.Equal(0, state.Count);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task ReadingNullKeysThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _storage.ReadAsync(null));
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task WritingNullStoreItemsThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _storage.WriteAsync(null));
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task WritingNoStoreItemsDoesntThrow()
        {
            var changes = new Dictionary<string, object>();
            await _storage.WriteAsync(changes);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // For issue https://github.com/Microsoft/botbuilder-dotnet/issues/871
        // See the linked issue for details. This issue was happening when using the CosmosDB
        // data store for state. The stepIndex, which was an object being cast to an Int64
        // after deserialization, was throwing an exception for not being Int32 datatype.
        // This test checks to make sure that this error is no longer thrown.
        //
        // The problem was reintroduced when the prompt retry count feature was implemented:
        // https://github.com/microsoft/botbuilder-dotnet/issues/1859
        // The waterfall in this test has been modified to include a prompt.
        [IgnoreOnNoEmulatorFact]
        public async Task WaterfallCosmos()
        {
            var convoState = new ConversationState(_storage);

            var adapter = new TestAdapter(TestAdapter.CreateConversation("waterfallTest"))
                .Use(new AutoSaveStateMiddleware(convoState));

            var dialogState = convoState.CreateProperty<DialogState>("dialogStateForWaterfallTest");
            var dialogs = new DialogSet(dialogState);

            dialogs.Add(new TextPrompt(nameof(TextPrompt), async (promptContext, cancellationToken) =>
            {
                var result = promptContext.Recognized.Value;
                if (result.Length > 3)
                {
                    var succeededMessage = MessageFactory.Text($"You got it at the {promptContext.AttemptCount}th try!");
                    await promptContext.Context.SendActivityAsync(succeededMessage, cancellationToken);
                    return true;
                }

                var reply = MessageFactory.Text($"Please send a name that is longer than 3 characters. {promptContext.AttemptCount}");
                await promptContext.Context.SendActivityAsync(reply, cancellationToken);

                return false;
            }));

            var steps = new WaterfallStep[]
                {
                    async (stepContext, ct) =>
                    {
                        Assert.Equal(typeof(int), stepContext.ActiveDialog.State["stepIndex"].GetType());
                        await stepContext.Context.SendActivityAsync("step1");
                        return Dialog.EndOfTurn;
                    },
                    async (stepContext, ct) =>
                    {
                        Assert.Equal(typeof(int), stepContext.ActiveDialog.State["stepIndex"].GetType());
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please type your name.") }, ct);
                    },
                    async (stepContext, ct) =>
                    {
                        Assert.Equal(typeof(int), stepContext.ActiveDialog.State["stepIndex"].GetType());
                        await stepContext.Context.SendActivityAsync("step3");
                        return Dialog.EndOfTurn;
                    },
                };

            dialogs.Add(new WaterfallDialog(nameof(WaterfallDialog), steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (turnContext.Activity.Text == "reset")
                {
                    await dialogState.DeleteAsync(turnContext);
                }
                else
                {
                    var dc = await dialogs.CreateContextAsync(turnContext);

                    await dc.ContinueDialogAsync();

                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync(nameof(WaterfallDialog));
                    }
                }
            })
                .Send("reset")
                .Send("hello")
                .AssertReply("step1")
                .Send("hello")
                .AssertReply("Please type your name.")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters. 1")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters. 2")
                .Send("hi")
                .AssertReply("Please send a name that is longer than 3 characters. 3")
                .Send("Kyle")
                .AssertReply("You got it at the 4th try!")
                .AssertReply("step3")
                .StartTestAsync();
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task Should_Be_Aware_Of_Nesting_Limit()
        {
            async Task TestNestAsync(int depth)
            {
                // This creates nested data with both objects and arrays
                static JToken CreateNestedData(int count, bool isArray = false)
                    => count > 0
                        ? (isArray
                            ? new JArray { CreateNestedData(count - 1, false) } as JToken
                            : new JObject { new JProperty("data", CreateNestedData(count - 1, true)) })
                        : null;

                var changes = new Dictionary<string, object>
                {
                    { "CONTEXTKEY", CreateNestedData(depth) },
                };

                await _storage.WriteAsync(changes);
            }

            // Should not throw
            await TestNestAsync(127);

            try
            {
                // Should either not throw or throw a special exception
                await TestNestAsync(128);
            }
            catch (InvalidOperationException ex)
            {
                // If the nesting limit is changed on the Cosmos side
                // then this assertion won't be reached, which is okay
                Assert.Contains("recursion", ex.Message);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task Should_Be_Aware_Of_Nesting_Limit_With_Dialogs()
        {
            async Task TestDialogNestAsync(int dialogDepth)
            {
                Dialog CreateNestedDialog(int depth) => new ComponentDialog(nameof(ComponentDialog))
                    .AddDialog(depth > 0
                        ? CreateNestedDialog(depth - 1)
                        : new WaterfallDialog(
                            nameof(WaterfallDialog),
                            new List<WaterfallStep>
                            {
                                async (stepContext, ct) => Dialog.EndOfTurn
                            }));

                var dialog = CreateNestedDialog(dialogDepth);

                var convoState = new ConversationState(_storage);

                var adapter = new TestAdapter(TestAdapter.CreateConversation("nestingTest"))
                    .Use(new AutoSaveStateMiddleware(convoState));

                var dialogState = convoState.CreateProperty<DialogState>("dialogStateForNestingTest");

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    if (turnContext.Activity.Text == "reset")
                    {
                        await dialogState.DeleteAsync(turnContext);
                    }
                    else
                    {
                        await dialog.RunAsync(turnContext, dialogState, cancellationToken);
                    }
                })
                    .Send("reset")
                    .Send("hello")
                    .StartTestAsync();
            }

            // Should not throw
            await TestDialogNestAsync(23);

            try
            {
                // Should either not throw or throw a special exception
                await TestDialogNestAsync(24);
            }
            catch (InvalidOperationException ex)
            {
                // If the nesting limit is changed on the Cosmos side
                // then this assertion won't be reached, which is okay
                Assert.Contains("dialogs", ex.Message);
            }
        }
    }
}
