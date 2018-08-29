// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - CosmosDB")]
    public class CosmosDbStorageTests : StorageBaseTests
    {
        // Endpoint and Authkey for the CosmosDB Emulator running locally
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "test-db";
        private const string CosmosCollectionName = "bot-storage";

        private static string _emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");
        private const string _noEmulatorMessage = "This test requires CosmosDB Emulator! go to https://aka.ms/documentdb-emulator-docs to download and install.";
        private static Lazy<bool> _hasEmulator = new Lazy<bool>(() =>
        {
            if (File.Exists(_emulatorPath))
            { 
                Process p = new Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = _emulatorPath;
                p.StartInfo.Arguments = "/GetStatus";
                p.Start();
                p.WaitForExit();

                return p.ExitCode == 2;
            }

            return false;
        });

        private IStorage _storage;

        [TestInitialize]
        public void TestInit()
        {
            if (_hasEmulator.Value)
            {
                _storage = new CosmosDbStorage(new CosmosDbStorageOptions
                {
                    AuthKey = CosmosAuthKey,
                    CollectionId = CosmosCollectionName,
                    CosmosDBEndpoint = new Uri(CosmosServiceEndpoint),
                    DatabaseId = CosmosDatabaseName,
                });
            }
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            if (_storage != null)
            {
                var client = new DocumentClient(new Uri(CosmosServiceEndpoint), CosmosAuthKey);
                try
                {
                    await client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(CosmosDatabaseName)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
                }
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task CreateObjectTest()
        {
            if (CheckEmulator())
            {
                await base._createObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadUnknownTest()
        {
            if (CheckEmulator())
            {
                await base._readUnknownTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task UpdateObjectTest()
        {
            if (CheckEmulator())
            {
                await base._updateObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteObjectTest()
        {
            if (CheckEmulator())
            {
                await base._deleteObjectTest(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            if (CheckEmulator())
            {
                await base._handleCrazyKeys(_storage);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public void ConnectionPolicyConfiguratorShouldBeCalled()
        {
            if (CheckEmulator())
            {
                ConnectionPolicy policyRef = null;

                _storage = new CosmosDbStorage(new CosmosDbStorageOptions
                {
                    AuthKey = CosmosAuthKey,
                    CollectionId = CosmosCollectionName,
                    CosmosDBEndpoint = new Uri(CosmosServiceEndpoint),
                    DatabaseId = CosmosDatabaseName,
                    ConnectionPolicyConfigurator = (ConnectionPolicy policy) => policyRef = policy
                });

                Assert.IsNotNull(policyRef, "ConnectionPolicy configurator was not called.");
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadingEmptyKeys_Throws()
        {
            if (CheckEmulator())
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(() => _storage.ReadAsync(new string[] { }));
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task WrittingNullStoreItems_Throws()
        {
            if (CheckEmulator())
            {
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _storage.WriteAsync(null));
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // For issue https://github.com/Microsoft/botbuilder-dotnet/issues/871
        // See the linked issue for details. This issue was happening when using the CosmosDB
        // data store for state. The stepIndex, which was an object being cast to an Int64
        // after deserialization, was throwing an exception for not being Int32 datatype.
        // This test checks to make sure that this error is no longer thrown.
        [TestMethod]
        public async Task WaterfallCosmos()
        {
            if (CheckEmulator())
            {
                var convoState = new ConversationState(_storage);

                var adapter = new TestAdapter()
                    .Use(convoState);

                var dialogState = convoState.CreateProperty<DialogState>("dialogState");
                var dialogs = new DialogSet(dialogState);
                dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
                {
                    async (dc, step, ct) =>
                    {
                        Assert.AreEqual(dc.ActiveDialog.State["stepIndex"].GetType(), typeof(Int32));
                        await dc.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn;
                    },
                    async (dc, step, ct) =>
                    {
                        Assert.AreEqual(dc.ActiveDialog.State["stepIndex"].GetType(), typeof(Int32));
                        await dc.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn;
                    },
                    async (dc, step, ct) =>
                    {
                        Assert.AreEqual(dc.ActiveDialog.State["stepIndex"].GetType(), typeof(Int32));
                        await dc.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn;
                    },
                }));

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext);
                    await dc.ContinueAsync();
                    if (!turnContext.Responded)
                    {
                        await dc.BeginAsync("test");
                    }
                })
                    .Send("hello")
                    .AssertReply("step1")
                    .Send("hello")
                    .AssertReply("step2")
                    .Send("hello")
                    .AssertReply("step3")
                    .StartTestAsync();
            }
        }

        public bool CheckEmulator()
        {
            if (!_hasEmulator.Value)
                Assert.Inconclusive(_noEmulatorMessage);
            if (Debugger.IsAttached)
                Assert.IsTrue(_hasEmulator.Value, _noEmulatorMessage);

            return _hasEmulator.Value;
        }
    }
}
