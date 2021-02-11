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
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [Trait("TestCategory", "Storage")]
    [Trait("TestCategory", "Storage - CosmosDB")]
    public class CosmosDbStorageTests : StorageBaseTests, IAsyncLifetime
    {
        // Endpoint and Authkey for the CosmosDB Emulator running locally
        private const string CosmosServiceEndpoint = "https://localhost:8081";
        private const string CosmosAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string CosmosDatabaseName = "test-CosmosDbStorageTests";
        private const string CosmosCollectionName = "bot-storage";
        private const string DocumentId = "UtteranceLog-001";

        private static readonly string EmulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Azure Cosmos DB Emulator\CosmosDB.Emulator.exe");
        private static readonly Lazy<bool> HasEmulator = new Lazy<bool>(() =>
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME")))
            {
                return false;
            }

            if (File.Exists(EmulatorPath))
            {
                var tries = 5;

                do
                {
                    var p = new Process();
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = EmulatorPath;
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
        private readonly StoreItem _itemToTest = new StoreItem { MessageList = new string[] { "hi", "how are u" }, City = "Contoso" };

        private IStorage _storage;

        public CosmosDbStorageTests()
        {
            if (HasEmulator.Value)
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

        public Task InitializeAsync() 
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
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

        [IgnoreOnNoEmulatorFact]
        public void Sanitize_Key_Should_Work()
        {
            // Note: The SanitizeKey method delegates to the CosmosDBKeyEscape class. The method is
            // marked as obsolete, and should no longer be used. This test is here to make sure
            // the method does actually delegate, as we can't remove it due to back-compat reasons.
#pragma warning disable 0618
            // Ascii code of "?" is "3f".
            var sanitizedKey = CosmosDbStorage.SanitizeKey("?test?");
            Assert.Equal("*3ftest*3f", sanitizedKey);
#pragma warning restore 0618
        }

        [IgnoreOnNoEmulatorFact]
        public void Constructor_Should_Throw_On_InvalidOptions()
        {
            // No Options. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(null));

            // No Endpoint. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = "testId",
                DatabaseId = "testDb",
                CosmosDBEndpoint = null,
            }));

            // No Auth Key. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = null,
                CollectionId = "testId",
                DatabaseId = "testDb",
                CosmosDBEndpoint = new Uri("https://test.com"),
            }));

            // No Database Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = "testId",
                DatabaseId = null,
                CosmosDBEndpoint = new Uri("https://test.com"),
            }));

            // No Collection Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = "test",
                CollectionId = null,
                DatabaseId = "testDb",
                CosmosDBEndpoint = new Uri("https://test.com"),
            }));
        }

        [IgnoreOnNoEmulatorFact]
        public void CustomConstructor_Should_Throw_On_InvalidOptions()
        {
            var customClient = GetDocumentClient().Object;

            // No client. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(null, new CosmosDbCustomClientOptions
            {
                CollectionId = "testId",
                DatabaseId = "testDb",
            }));

            // No Options. Should throw.
            Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(customClient, null));

            // No Database Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(customClient, new CosmosDbCustomClientOptions
            {
                CollectionId = "testId",
                DatabaseId = null,
            }));

            // No Collection Id. Should throw.
            Assert.Throws<ArgumentException>(() => new CosmosDbStorage(customClient, new CosmosDbCustomClientOptions
            {
                CollectionId = null,
                DatabaseId = "testDb",
            }));
        }

        [IgnoreOnNoEmulatorFact]
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
            Assert.True(wasCalled, "The Connection Policy Configurator was not called.");
        }

        [IgnoreOnNoEmulatorFact]
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
        [IgnoreOnNoEmulatorFact]
        public async Task CreateObjectCosmosDBTest()
        {
            await CreateObjectTest(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task ReadUnknownCosmosDBTest()
        {
            await ReadUnknownTest(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task UpdateObjectCosmosDBTest()
        {
            await UpdateObjectTest<DocumentClientException>(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task DeleteObjectCosmosDBTest()
        {
            await DeleteObjectTest(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task HandleCrazyKeysCosmosDB()
        {
            await HandleCrazyKeys(_storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public void ConnectionPolicyConfiguratorShouldBeCalled()
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

            Assert.NotNull(policyRef);
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

            var adapter = new TestAdapter(TestAdapter.CreateConversation(nameof(WaterfallCosmos)))
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

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task DeleteAsyncFromSingleCollection()
        {
            var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions());
            var changes = new Dictionary<string, object>
            {
                { DocumentId, _itemToTest }
            };

            await storage.WriteAsync(changes, CancellationToken.None);

            var result = await Task.WhenAny(storage.DeleteAsync(new string[] { DocumentId }, CancellationToken.None)).ConfigureAwait(false);
            Assert.True(result.IsCompletedSuccessfully);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task DeleteAsyncFromPartitionedCollection()
        {
            // The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
            // The partitionKeyPath must have the "document" value to properly route the values as partitionKey
            // <see also cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
            const string partitionKeyPath = "document/city";

            await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

            // Connect to the cosmosDb created before with "Contoso" as partitionKey
            var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions("Contoso"));
            var changes = new Dictionary<string, object>
            {
                { DocumentId, _itemToTest }
            };

            await storage.WriteAsync(changes, CancellationToken.None);

            var result = await Task.WhenAny(storage.DeleteAsync(new string[] { DocumentId }, CancellationToken.None)).ConfigureAwait(false);
            Assert.True(result.IsCompletedSuccessfully);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task DeleteAsyncFromPartitionedCollectionWithoutPartitionKey()
        {
            // The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
            // The partitionKeyPath must have the "document" value to properly route the values as partitionKey
            // <see also cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
            const string partitionKeyPath = "document/city";

            await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

            // Connect to the cosmosDb created before
            var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions());
            var changes = new Dictionary<string, object>
            {
                { DocumentId, _itemToTest }
            };

            await storage.WriteAsync(changes, CancellationToken.None);

            // Should throw InvalidOperationException: PartitionKey value must be supplied for this operation.
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await storage.DeleteAsync(new string[] { DocumentId }, CancellationToken.None));
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task ReadAsyncWithPartitionKey()
        {
            // The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
            // The partitionKeyPath must have the "document" value to properly route the values as partitionKey
            // <see also cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
            const string partitionKeyPath = "document/city";

            await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

            // Connect to the cosmosDb created before with "Contoso" as partitionKey
            var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions("Contoso"));
            var changes = new Dictionary<string, object>
            {
                { DocumentId, _itemToTest }
            };

            await storage.WriteAsync(changes, CancellationToken.None);

            var result = await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None);
            Assert.Equal(_itemToTest.City, result[DocumentId].City);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task ReadAsyncWithoutPartitionKey()
        {
            // The WriteAsync method receive a object as a parameter then encapsulate it in a object named "document"
            // The partitionKeyPath must have the "document" value to properly route the values as partitionKey
            // <see also cref="WriteAsync(IDictionary{string, object}, CancellationToken)"/>
            const string partitionKeyPath = "document/city";

            await CreateCosmosDbWithPartitionedCollection(partitionKeyPath);

            // Connect to the cosmosDb created before without partitionKey
            var storage = new CosmosDbStorage(CreateCosmosDbStorageOptions());
            var changes = new Dictionary<string, object>
            {
                { DocumentId, _itemToTest }
            };

            await storage.WriteAsync(changes, CancellationToken.None);

#if NETCOREAPP2_1
            // Should throw DocumentClientException: Cross partition query is required but disabled
            await Assert.ThrowsAsync<DocumentClientException>(async () => await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None));
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

            Assert.True(badRequestExceptionThrown, "Expected: DocumentClientException with HttpStatusCode.BadRequest");

            // TODO: netcoreapp3.0 throws Microsoft.Azure.Documents.BadRequestException which derives from DocumentClientException, but it is internal
            //await Assert.ThrowsAsync<DocumentClientException>(async () => await storage.ReadAsync<StoreItem>(new string[] { DocumentId }, CancellationToken.None));
#endif
        }

        // NOTE: THESE TESTS REQUIRE THAT THE COSMOS DB EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [IgnoreOnNoEmulatorFact]
        public async Task StatePersistsThroughMultiTurn_TypeNameHandlingNone()
        {
            var storage = new CosmosDbStorage(
                               CreateCosmosDbStorageOptions(),
                               new JsonSerializer() { TypeNameHandling = TypeNameHandling.None });
            await StatePersistsThroughMultiTurn(storage);
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
