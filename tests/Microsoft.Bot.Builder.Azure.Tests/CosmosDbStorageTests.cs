// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestMethod]
        public void Sanatize_Key_Should_Work()
        {
            // Note: The SanatizeKey method delegates to the CosmosDBKeyEscape class. The method is 
            // marked as obsolete, and should no longer be used. This test is here to make sure
            // the method does actually delegate, as we can't remove it due to back-compat reasons.

#pragma warning disable 0618
            // Ascii code of "?" is "3f".
            var sanitizedKey = CosmosDbStorage.SanitizeKey("?test?");
            Assert.AreEqual(sanitizedKey, "*3ftest*3f");
#pragma warning restore 0618
        }

        [TestMethod]
        public void Constructor_Should_Throw_On_InvalidOptions()
        {
            // No Options. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbStorage(null));

            // No Endpoint. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = "testId",
                DatabaseId = "testDb",
                CosmosDBEndpoint = null,
            }));

            // No Auth Key. Should throw. 
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = null,
                CollectionId = "testId",
                DatabaseId = "testDb",
                CosmosDBEndpoint = new Uri("https://test.com"),
            }));

            // No Database Id. Should throw. 
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = "testId",
                DatabaseId = null,
                CosmosDBEndpoint = new Uri("https://test.com"),
            }));

            // No Collection Id. Should throw. 
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = null,
                DatabaseId = "testDb",
                CosmosDBEndpoint = new Uri("https://test.com"),
            }));
        }

        [TestMethod]
        public void CustomConstructor_Should_Throw_On_InvalidOptions()
        {
            var customClient = GetDocumentClient().Object;

            // No client. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbStorage(null, new CosmosDbCustomClientOptions
            {
                CollectionId = "testId",
                DatabaseId = "testDb",
            }));

            // No Options. Should throw. 
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbStorage(customClient, null));

            // No Database Id. Should throw. 
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbStorage(customClient, new CosmosDbCustomClientOptions
            {
                CollectionId = "testId",
                DatabaseId = null,
            }));

            // No Collection Id. Should throw. 
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbStorage(customClient, new CosmosDbCustomClientOptions
            {
                CollectionId = null,
                DatabaseId = "testDb",
            }));
        }

        [TestMethod]
        public void Connection_Policy_Configurator_Should_Be_Called_If_Present()
        {
            var wasCalled = false;

            var optionsWithConfigurator = new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = "testId",
                DatabaseId = "testDb",
                CosmosDBEndpoint = new Uri("https://test.com"),

                // Make sure the Callback is called.
                ConnectionPolicyConfigurator = (ConnectionPolicy p) => wasCalled = true,
            };

            var storage = new CosmosDbStorage(optionsWithConfigurator);
            Assert.IsTrue(wasCalled, "The Connection Policy Configurator was not called.");
        }

        private Mock<IDocumentClient> GetDocumentClient()
        {
            var mock = new Mock<IDocumentClient>();

            mock.Setup(client => client.CreateDatabaseIfNotExistsAsync(It.IsAny<Database>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(() => {
                    var database = new Database();
                    database.SetPropertyValue("SelfLink", "dummyDB_SelfLink");
                    return new ResourceResponse<Database>(database);
                });

            mock.Setup(client => client.CreateDocumentCollectionIfNotExistsAsync(It.IsAny<Uri>(), It.IsAny<DocumentCollection>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(() => {
                    var documentCollection = new DocumentCollection();
                    documentCollection.SetPropertyValue("SelfLink", "dummyDC_SelfLink");
                    return new ResourceResponse<DocumentCollection>(documentCollection);
                });

            mock.Setup(client => client.ConnectionPolicy).Returns(new ConnectionPolicy());

            return mock;
        }

        [TestMethod]
        public async Task Database_Creation_Request_Options_Should_Be_Used()
        {
            var documentClientMock = GetDocumentClient();

            var databaseCreationRequestOptions = new RequestOptions { OfferThroughput = 1000 };
            var documentCollectionRequestOptions = new RequestOptions { OfferThroughput = 500 };

            var optionsWithConfigurator = new CosmosDbCustomClientOptions
            {
                CollectionId = "testId",
                DatabaseId = "testDb",
                DatabaseCreationRequestOptions = databaseCreationRequestOptions,
                DocumentCollectionRequestOptions = documentCollectionRequestOptions,
            };

            var storage = new CosmosDbStorage(documentClientMock.Object, optionsWithConfigurator);
            await storage.DeleteAsync(new string[] { "foo" }, CancellationToken.None);

            documentClientMock.Verify(client => client.CreateDatabaseIfNotExistsAsync(It.IsAny<Database>(), databaseCreationRequestOptions), Times.Once());
            documentClientMock.Verify(client => client.CreateDocumentCollectionIfNotExistsAsync(It.IsAny<Uri>(), It.IsAny<DocumentCollection>(), documentCollectionRequestOptions), Times.Once());
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
        public async Task ReadingNullKeysReturnsEmptyDictionary()
        {
            if (CheckEmulator())
            {
                string[] nullKeys = null;
                var state = await _storage.ReadAsync(nullKeys);
                Assert.IsInstanceOfType(state, typeof(Dictionary<string, object>));
                Assert.AreEqual(state.Count, 0);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task WritingNullStoreItemsDoesntThrow()
        {
            if (CheckEmulator())
            {
                await _storage.WriteAsync(null);
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
                dialogs.Add(new WaterfallDialog("test", new WaterfallStep[]
                {
                    async (stepContext, ct) =>
                    {
                        Assert.AreEqual(stepContext.ActiveDialog.State["stepIndex"].GetType(), typeof(Int32));
                        await stepContext.Context.SendActivityAsync("step1"); return Dialog.EndOfTurn;
                    },
                    async (stepContext, ct) =>
                    {
                        Assert.AreEqual(stepContext.ActiveDialog.State["stepIndex"].GetType(), typeof(Int32));
                        await stepContext.Context.SendActivityAsync("step2"); return Dialog.EndOfTurn;
                    },
                    async (stepContext, ct) =>
                    {
                        Assert.AreEqual(stepContext.ActiveDialog.State["stepIndex"].GetType(), typeof(Int32));
                        await stepContext.Context.SendActivityAsync("step3"); return Dialog.EndOfTurn;
                    },
                }));

                await new TestFlow(adapter, async (turnContext, cancellationToken) =>
                {
                    var dc = await dialogs.CreateContextAsync(turnContext);
                    await dc.ContinueDialogAsync();
                    if (!turnContext.Responded)
                    {
                        await dc.BeginDialogAsync("test");
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
