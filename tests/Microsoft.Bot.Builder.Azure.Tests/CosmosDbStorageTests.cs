// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

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
        private const string CosmosDatabaseName = "test-CosmosDbStorageTests";
        private const string CosmosCollectionName = "bot-storage";
        private const string DocumentId = "UtteranceLog-001";

        private const string NoEmulatorMessage = "This test requires CosmosDB Emulator! go to https://aka.ms/documentdb-emulator-docs to download and install.";
        private static readonly string _emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");
        private static readonly Lazy<bool> _hasEmulator = new Lazy<bool>(() =>
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME")))
            {
                return false;
            }

            if (File.Exists(_emulatorPath))
            {
                var tries = 5;

                do
                {
                    var p = new Process();
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = _emulatorPath;
                    p.StartInfo.Arguments = "/GetStatus";
                    p.Start();
                    p.WaitForExit();

                    switch (p.ExitCode)
                    {
                        case 1: // starting
                            Task.Delay(1000).Wait();
                            tries--;
                            break;

                        case 2: // started
                            return true;

                        case 3: // stopped
                            return false;

                        default:
                            return false; // unknown status code
                    }
                }
                while (tries > 0);
            }

            return false;
        });

        // Item used to test delete cases
        private readonly StoreItem itemToTest = new StoreItem { MessageList = new string[] { "hi", "how are u" }, City = "Contoso" };

        private IStorage _storage;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            if (CheckEmulator())
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
        public async Task HandleCrazyKeys()
        {
            if (CheckEmulator())
            {
                await HandleCrazyKeys(_storage);
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
                    ConnectionPolicyConfigurator = (ConnectionPolicy policy) => policyRef = policy,
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

                var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
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

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteAsyncFromSingleCollection()
        {
            if (CheckEmulator())
            {
                var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions());
                var changes = new Dictionary<string, object>
                {
                    { DocumentId, itemToTest }
                };

                await storage.WriteAsync(changes, CancellationToken.None);

                var result = await Task.WhenAny(storage.DeleteAsync(new string[] { DocumentId }, CancellationToken.None)).ConfigureAwait(false);
                Assert.IsTrue(result.IsCompletedSuccessfully);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteAsyncFromPartitionedCollection()
        {
            if (CheckEmulator())
            {
                /// The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
                /// The partitionKeyPath must have the "document" value to properly route the values as partitionKey
                /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
                var partitionKeyPath = "document/city";

                await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

                // Connect to the comosDb created before with "Contoso" as partitionKey
                var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions("Contoso"));
                var changes = new Dictionary<string, object>
                {
                    { DocumentId, itemToTest }
                };

                await storage.WriteAsync(changes, CancellationToken.None);

                var result = await Task.WhenAny(storage.DeleteAsync(new string[] { DocumentId }, CancellationToken.None)).ConfigureAwait(false);
                Assert.IsTrue(result.IsCompletedSuccessfully);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task DeleteAsyncFromPartitionedCollectionWithoutPartitionKey()
        {
            if (CheckEmulator())
            {
                /// The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
                /// The partitionKeyPath must have the "document" value to properly route the values as partitionKey
                /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
                var partitionKeyPath = "document/city";

                await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

                // Connect to the comosDb created before
                var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions());
                var changes = new Dictionary<string, object>
                {
                    { DocumentId, itemToTest }
                };

                await storage.WriteAsync(changes, CancellationToken.None);

                // Should throw InvalidOperationException: PartitionKey value must be supplied for this operation.
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await storage.DeleteAsync(new string[] { DocumentId }, CancellationToken.None));
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadAsyncWithPartitionKey()
        {
            if (CheckEmulator())
            {
                /// The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
                /// The partitionKeyPath must have the "document" value to properly route the values as partitionKey
                /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
                var partitionKeyPath = "document/city";

                await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

                // Connect to the comosDb created before with "Contoso" as partitionKey
                var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions("Contoso"));
                var changes = new Dictionary<string, object>
                {
                    { DocumentId, itemToTest }
                };

                await storage.WriteAsync(changes, CancellationToken.None);

                var result = await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None);
                Assert.AreEqual(itemToTest.City, result[DocumentId].City);
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task ReadAsyncWithoutPartitionKey()
        {
            if (CheckEmulator())
            {
                /// The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
                /// The partitionKeyPath must have the "document" value to properly route the values as partitionKey
                /// <seealso cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
                var partitionKeyPath = "document/city";

                await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

                // Connect to the comosDb created before without partitionKey
                var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions());
                var changes = new Dictionary<string, object>
                {
                    { DocumentId, itemToTest }
                };

                await storage.WriteAsync(changes, CancellationToken.None);

#if NETCOREAPP2_1
                // Should throw DocumentClientException: Cross partition query is required but disabled
                await Assert.ThrowsExceptionAsync<DocumentClientException>(async () => await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None));
#else // required by NETCOREAPP3_0 (have only tested NETCOREAPP2_1 and NETCOREAPP3_0)
                var badRequestExceptionThrown = false;
                try
                {
                    await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None);
                }
                catch (DocumentClientException ex)
                {
                    badRequestExceptionThrown = ex.StatusCode == HttpStatusCode.BadRequest; 
                }

                Assert.IsTrue(badRequestExceptionThrown, "Expected: DocumentClientException with HttpStatusCode.BadRequest");

                // TODO: netcoreapp3.0 throws Microsoft.Azure.Documents.BadRequestException which derives from DocumentClientException, but it is internal
                //await Assert.ThrowsExceptionAsync<DocumentClientException>(async () => await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None));
#endif
            }
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task StatePersistsThroughMultiTurn_TypeNameHandlingNone()
        {
            if (CheckEmulator())
            {
                var storage = new CosmosDbStorage(
                                   CreateCosmosDbStorageOptions(),
                                   new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
                await StatePersistsThroughMultiTurn(storage);
            }
        }

        public bool CheckEmulator()
        {
            if (!_hasEmulator.Value)
            {
                Assert.Inconclusive(NoEmulatorMessage);
            }

            if (Debugger.IsAttached)
            {
                Assert.IsTrue(_hasEmulator.Value, NoEmulatorMessage);
            }

            return _hasEmulator.Value;
        }

        private static async Task CreateCosmosDbWithPartitionedCollection(string partitionKey)
        {
            using var client = new DocumentClient(new Uri(CosmosServiceEndpoint), CosmosAuthKey);
            Database database = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = CosmosDatabaseName });
            var partitionKeyDefinition = new PartitionKeyDefinition { Paths = new Collection<string> { $"/{partitionKey}" } };
            var collectionDefinition = new DocumentCollection { Id = CosmosCollectionName, PartitionKey = partitionKeyDefinition };

            await client.CreateDocumentCollectionIfNotExistsAsync(database.SelfLink, collectionDefinition);
        }

        private static CosmosDbStorageOptions CreateCosmosDbStorageOptions(string partitionKey = "")
        {
            return new CosmosDbStorageOptions()
            {
                PartitionKey = partitionKey,
                AuthKey = CosmosAuthKey,
                CollectionId = CosmosCollectionName,
                CosmosDBEndpoint = new Uri(CosmosServiceEndpoint),
                DatabaseId = CosmosDatabaseName,
            };
        }

        private Mock<IDocumentClient> GetDocumentClient()
        {
            var mock = new Mock<IDocumentClient>();

            mock.Setup(client => client.CreateDatabaseIfNotExistsAsync(It.IsAny<Database>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(() =>
                {
                    var database = new Database();
                    database.SetPropertyValue("SelfLink", "dummyDB_SelfLink");
                    return new ResourceResponse<Database>(database);
                });

            mock.Setup(client => client.CreateDocumentCollectionIfNotExistsAsync(It.IsAny<Uri>(), It.IsAny<DocumentCollection>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(() =>
                {
                    var documentCollection = new DocumentCollection();
                    documentCollection.SetPropertyValue("SelfLink", "dummyDC_SelfLink");
                    return new ResourceResponse<DocumentCollection>(documentCollection);
                });

            mock.Setup(client => client.ConnectionPolicy).Returns(new ConnectionPolicy());

            return mock;
        }

        internal class StoreItem : IStoreItem
        {
            [JsonProperty(PropertyName = "messageList")]
            public string[] MessageList { get; set; }

            [JsonProperty(PropertyName = "city")]
            public string City { get; set; }

            public string ETag { get; set; }
        }
    }
}
