// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - CosmosDB Partitioned")]
    public class CosmosDbPartitionStorageTests : StorageBaseTests
    {
        // Endpoint and Authkey for the CosmosDB Emulator running locally
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "test-CosmosDbPartitionStorageTests";
        private const string CosmosCollectionName = "bot-storage";

        private const string _noEmulatorMessage = "This test requires CosmosDB Emulator! go to https://aka.ms/documentdb-emulator-docs to download and install.";
        private static readonly string _emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");
        private static readonly Lazy<bool> _hasEmulator = new Lazy<bool>(() =>
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME")))
            {
                return false;
            }

            if (File.Exists(_emulatorPath))
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName = _emulatorPath,
                        Arguments = "/GetStatus",
                    },
                };
                p.Start();
                p.WaitForExit();

                return p.ExitCode == 2;
            }

            return false;
        });

        private IStorage _storage;

        [ClassInitialize]
        public static async Task AllTestsInitialize(TestContext testContext)
        {
            if (_hasEmulator.Value)
            {
                var client = new CosmosClient(
                    CosmosServiceEndpoint,
                    CosmosAuthKey,
                    new CosmosClientOptions());

                await client.CreateDatabaseIfNotExistsAsync(CosmosDatabaseName);
            }
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static async Task AllTestsCleanup()
        {
            var client = new CosmosClient(
                CosmosServiceEndpoint,
                CosmosAuthKey,
                new CosmosClientOptions());
            try
            {
                await client.GetDatabase(CosmosDatabaseName).DeleteAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
            }
        }

        [TestInitialize]
        public void TestInit()
        {
            if (_hasEmulator.Value)
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
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            _storage = null;
        }

        [TestMethod]
        public void Constructor_Should_Throw_On_InvalidOptions()
        {
            // No Options. Should throw.
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbPartitionedStorage(null));

            // No Endpoint. Should throw.
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "test",
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = null,
            }));

            // No Auth Key. Should throw.
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = null,
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
            }));

            // No Database Id. Should throw.
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = "testId",
                DatabaseId = null,
                CosmosDbEndpoint = "testEndpoint",
            }));

            // No Container Id. Should throw.
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = null,
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
            }));

            // Invalid Row Key characters in KeySuffix
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = "testAuthKey",
                ContainerId = "testId",
                DatabaseId = "testDb",
                CosmosDbEndpoint = "testEndpoint",
                KeySuffix = "?#*test",
                CompatibilityMode = false
            }));

            Assert.ThrowsException<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
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
        [TestMethod]
        public async Task CreateObjectTest()
        {
            if (CheckEmulator())
            {
                await CreateObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadUnknownTest()
        {
            if (CheckEmulator())
            {
                await ReadUnknownTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task UpdateObjectTest()
        {
            if (CheckEmulator())
            {
                await UpdateObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteObjectTest()
        {
            if (CheckEmulator())
            {
                await DeleteObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteUnknownObjectTest()
        {
            if (CheckEmulator())
            {
                await _storage.DeleteAsync(new[] { "unknown_delete" });
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            if (CheckEmulator())
            {
                await HandleCrazyKeys(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadingEmptyKeysReturnsEmptyDictionary()
        {
            if (CheckEmulator())
            {
                var state = await _storage.ReadAsync(new string[] { });
                Assert.IsInstanceOfType(state, typeof(Dictionary<string, object>));
                Assert.AreEqual(state.Count, 0);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadingNullKeysThrowException()
        {
            if (CheckEmulator())
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _storage.ReadAsync(null));
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task WritingNullStoreItemsThrowException()
        {
            if (CheckEmulator())
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _storage.WriteAsync(null));
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task WritingNoStoreItemsDoesntThrow()
        {
            if (CheckEmulator())
            {
                var changes = new Dictionary<string, object>();
                await _storage.WriteAsync(changes);
            }
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
        [TestMethod]
        public async Task WaterfallCosmos()
        {
            if (CheckEmulator())
            {
                var convoState = new ConversationState(_storage);

                var adapter = new TestAdapter()
                    .Use(new AutoSaveStateMiddleware(convoState));

                var dialogState = convoState.CreateProperty<DialogState>("dialogState");
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
                            Assert.AreEqual(stepContext.ActiveDialog.State["stepIndex"].GetType(), typeof(int));
                            await stepContext.Context.SendActivityAsync("step1");
                            return Dialog.EndOfTurn;
                        },
                        async (stepContext, ct) =>
                        {
                            Assert.AreEqual(stepContext.ActiveDialog.State["stepIndex"].GetType(), typeof(int));
                            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please type your name.") }, ct);
                        },
                        async (stepContext, ct) =>
                        {
                            Assert.AreEqual(stepContext.ActiveDialog.State["stepIndex"].GetType(), typeof(int));
                            await stepContext.Context.SendActivityAsync("step3");
                            return Dialog.EndOfTurn;
                        },
                    };

                dialogs.Add(new WaterfallDialog(nameof(WaterfallDialog), steps));

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext);

                    await dc.ContinueDialogAsync();
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync(nameof(WaterfallDialog));
                    }
                })
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
        }

        public bool CheckEmulator()
        {
            if (!_hasEmulator.Value)
            {
                Assert.Inconclusive(_noEmulatorMessage);
            }

            if (Debugger.IsAttached)
            {
                Assert.IsTrue(_hasEmulator.Value, _noEmulatorMessage);
            }

            return _hasEmulator.Value;
        }
    }
}
