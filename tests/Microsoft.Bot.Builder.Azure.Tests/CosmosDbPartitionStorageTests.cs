// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Azure.CosmosDbPartitionedStorage;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    /// <summary>
    /// Requests to the Cosmos Container (ReadItemAsync, WriteItemAsync, etc) can be recorded <see cref=CosmosTestRecorder" />.
    /// Be sure to add _testRecorder.RecordingFileName = GetActualAsyncMethodName() <see cref="GetActualAsyncMethodName(string)"/>; to each test that needs recording.
    /// </summary>
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

        // Switch between Record and Playback mode using "COSMOS_RECORDING_MODE" Env Var, 
        // or switching the right-hand side of the null-coalesce to RecordingMode.Record.
        // This should default to Playback, but tests should be re-recorded with any test or CosmosDbPartitionedStorage change.
        private static readonly string _recordingMode = Environment.GetEnvironmentVariable("COSMOS_RECORDING_MODE") ?? RecordingMode.Playback;

        private static readonly string _noConnectionMessage = $"Unable to connect to Cosmos Endpoint {CosmosServiceEndpoint}. Running tests against recordings.";
        private static CosmosTestRecorder _testRecorder;
        private static readonly Lazy<bool> _canConnectToCosmosEndpoint = new Lazy<bool>(() =>
        {
            var connectionMade = false;

            // It's more code to build an authenticated request, so instead, we'll just catch
            // the errors and base the connection status off of that.
            try
            {
                WebRequest.Create(CosmosServiceEndpoint).GetResponse();
                connectionMade = true;
            }
            catch (WebException e)
            {
                // Unauthorized means the endpoint is up.
                connectionMade = e.Status == WebExceptionStatus.ProtocolError && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized;
            }

            if (!connectionMade && Debugger.IsAttached)
            {
                // 3 = Warning
                Debugger.Log(3, "CosmosDbPartitionStorageTests", _noConnectionMessage);
            }

            return connectionMade;
        });

        private IStorage _storage;

        [ClassInitialize]
        public static async Task AllTestsInitialize(TestContext testContext)
        {
            if (testContext is null)
            {
                throw new ArgumentNullException(nameof(testContext));
            }

            if (_canConnectToCosmosEndpoint.Value)
            {
                var client = new CosmosClient(
                    CosmosServiceEndpoint,
                    CosmosAuthKey,
                    new CosmosClientOptions());

                await client.CreateDatabaseIfNotExistsAsync(CosmosDatabaseName).ConfigureAwait(false);
            }
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static async Task AllTestsCleanup()
        {
            if (_canConnectToCosmosEndpoint.Value)
            {
                var client = new CosmosClient(
                CosmosServiceEndpoint,
                CosmosAuthKey,
                new CosmosClientOptions());
                try
                {
                    await client.GetDatabase(CosmosDatabaseName).DeleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error cleaning up resources: {0}", ex.ToString());
                }
            }
        }

        [TestInitialize]
        public async Task TestInit()
        {
            var options = new CosmosDbPartitionedStorageOptions()
            {
                AuthKey = CosmosAuthKey,
                ContainerId = CosmosCollectionName,
                CosmosDbEndpoint = CosmosServiceEndpoint,
                DatabaseId = CosmosDatabaseName,
            };

            // Playback mode by default

            if (!_canConnectToCosmosEndpoint.Value && _recordingMode == RecordingMode.Record)
            {
                throw new InvalidOperationException($"Unable to connect to Cosmos at {CosmosServiceEndpoint}. A connection to Emulator or a Cosmos instance is required for Record mode.");
            }

            _testRecorder = new CosmosTestRecorder(_recordingMode);

            options.CosmosClientOptions = new CosmosClientOptions();
            _storage = await GetStorageWithMockContainer(options).ConfigureAwait(false);
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            _storage = null;

            if (_testRecorder?.Mode == RecordingMode.Record)
            {
                await _testRecorder.WriteRecordingsToFiles().ConfigureAwait(false);
            }
        }

        [TestMethod]
        public void Constructor_Should_Throw_On_InvalidOptions()
        {
            // No Options. Should throw.
            Assert.ThrowsException<ArgumentNullException>(() => new CosmosDbPartitionedStorage(null));

            // No Endpoint. Should throw.
            Assert.ThrowsException<ArgumentException>(() => new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions()
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

        [TestMethod]
        public async Task CreateObjectTest()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await CreateObjectTest(_storage).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ReadUnknownTest()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await ReadUnknownTest(_storage).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task UpdateObjectTest()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await UpdateObjectTest(_storage).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeleteObjectTest()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await DeleteObjectTest(_storage).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DeleteUnknownObjectTest()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await _storage.DeleteAsync(new[] { "unknown_delete" }).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task HandleCrazyKeys()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await HandleCrazyKeys(_storage).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ReadingEmptyKeysReturnsEmptyDictionary()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            var state = await _storage.ReadAsync(new string[] { }).ConfigureAwait(false);
            Assert.IsInstanceOfType(state, typeof(Dictionary<string, object>));
            Assert.AreEqual(state.Count, 0);
        }

        [TestMethod]
        public async Task ReadingNullKeysThrowException()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _storage.ReadAsync(null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task WritingNullStoreItemsThrowException()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _storage.WriteAsync(null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task WritingNoStoreItemsDoesntThrow()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
            var changes = new Dictionary<string, object>();
            await _storage.WriteAsync(changes).ConfigureAwait(false);
        }

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
        public async Task CosmosStorageCanHandleBotStateInWaterfallDialog()
        {
            _testRecorder.RecordingFileName = GetActualAsyncMethodName();
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
                    await promptContext.Context.SendActivityAsync(succeededMessage, cancellationToken).ConfigureAwait(false);
                    return true;
                }

                var reply = MessageFactory.Text($"Please send a name that is longer than 3 characters. {promptContext.AttemptCount}");
                await promptContext.Context.SendActivityAsync(reply, cancellationToken).ConfigureAwait(false);

                return false;
            }));

            var steps = new WaterfallStep[]
                {
                    async (stepContext, ct) =>
                    {
                        Assert.AreEqual(typeof(int), stepContext.ActiveDialog.State["stepIndex"].GetType());
                        await stepContext.Context.SendActivityAsync("step1", cancellationToken: ct).ConfigureAwait(false);
                        return Dialog.EndOfTurn;
                    },
                    async (stepContext, ct) =>
                    {
                        Assert.AreEqual(typeof(int), stepContext.ActiveDialog.State["stepIndex"].GetType());
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please type your name.") }, ct).ConfigureAwait(false);
                    },
                    async (stepContext, ct) =>
                    {
                        Assert.AreEqual(typeof(int), stepContext.ActiveDialog.State["stepIndex"].GetType());
                        await stepContext.Context.SendActivityAsync("step3", cancellationToken: ct).ConfigureAwait(false);
                        return Dialog.EndOfTurn;
                    },
                };

            dialogs.Add(new WaterfallDialog(nameof(WaterfallDialog), steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);

                await dc.ContinueDialogAsync().ConfigureAwait(false);
                if (!turnContext.Responded)
                {
                    _ = await dc.BeginDialogAsync(nameof(WaterfallDialog)).ConfigureAwait(false);
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
                .StartTestAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Used for setting the filename in the TestRecorder.
        /// </summary>
        /// <remarks>
        /// MethodInfo.GetCurrentMethod() doesn't work with async methods; this does.
        /// </remarks>
        /// <param name="name">Not passed in directly and represents the name of the method.</param>
        /// <returns>The name of the original calling method.</returns>
        internal static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        /// <summary>
        /// In Record Mode, we need both a real container to make the actual calls, and a mocked container
        /// so that we can intercept the calls to the container and record them. We use an unmocked CosmosDbPartitionedStorage
        /// and use reflection to set the mocked container on it. The mocked container uses the real container in non-Playback modes.
        /// </summary>
        /// <param name="options">Represents the CosmosDbPartitionedStorageOptions passed to the CosmosDbPartitionedStorage.</param>
        /// <returns>A Task representing an instance of CosmosDbPartitionedStorage.</returns>
        internal async Task<CosmosDbPartitionedStorage> GetStorageWithMockContainer(CosmosDbPartitionedStorageOptions options)
        {
            // Create a normal, un-mocked CosmosDbPartitionedStorage used in all Recording modes
            var storage = new CosmosDbPartitionedStorage(options);

            // Use reflection to access the private _container field.
            var containerField = storage.GetType().GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);

            // Setup a mock container to intercept calls to the container.
            var mockContainer = new Mock<Container>();

            // Record Mode needs to use a real container, generated by the private InitializeAsync() method.
            Container realContainer = null;
            if (_recordingMode != RecordingMode.Playback)
            {
                var method = storage.GetType().GetMethod("InitializeAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                await (Task)method.Invoke(storage, null);

                // Use reflection to get the _container field (which is a real container in non-Playback modes, from above)
                realContainer = (Container)containerField.GetValue(storage);
            }
            else
            {
                realContainer = mockContainer.Object;
            }

            mockContainer.Setup(x => x.UpsertItemAsync(
                It.IsAny<DocumentStoreItem>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>())).Returns(async (
                    DocumentStoreItem item,
                    PartitionKey partitionKey,
                    ItemRequestOptions requestOptions,
                    CancellationToken cancellationToken) =>
                {
                    return await HandleMockedMethods(async () => await realContainer.UpsertItemAsync(
                            item,
                            partitionKey,
                            requestOptions,
                            cancellationToken)
                        .ConfigureAwait(false)).ConfigureAwait(false);
                });

            mockContainer.Setup(x => x.ReadItemAsync<DocumentStoreItem>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>())).Returns(async (
                    string id,
                    PartitionKey partitionKey,
                    ItemRequestOptions requestOptions,
                    CancellationToken cancellationToken) =>
                {
                    return await HandleMockedMethods(async () => await realContainer.ReadItemAsync<DocumentStoreItem>(
                            id,
                            partitionKey,
                            requestOptions,
                            cancellationToken)
                        .ConfigureAwait(false)).ConfigureAwait(false);
                });

            mockContainer.Setup(x => x.DeleteItemAsync<DocumentStoreItem>(
               It.IsAny<string>(),
               It.IsAny<PartitionKey>(),
               It.IsAny<ItemRequestOptions>(),
               It.IsAny<CancellationToken>())).Returns(async (
                   string id,
                   PartitionKey partitionKey,
                   ItemRequestOptions requestOptions,
                   CancellationToken cancellationToken) =>
               {
                   return await HandleMockedMethods(async () => await realContainer.DeleteItemAsync<DocumentStoreItem>(
                            id,
                            partitionKey,
                            requestOptions,
                            cancellationToken)
                        .ConfigureAwait(false)).ConfigureAwait(false);
               });

            // Set the mocked container to _container of the CosmosDbPartitionedStorage instance.
            containerField.SetValue(storage, mockContainer.Object);

            return storage;
        }

        /// <summary>
        /// We do basically the same thing to all mocked methods (read, write, delete):
        ///   1. If in record mode, use the real container to call the method, then record.
        ///   2. If in playback mode, get the saved responses from the recorded files.
        /// </summary>
        /// <param name="method">A method from the real container such as ReadItemAsync, WriteItemAsync, etc.</param>
        /// <returns>Either the actual DocumentStoreItem, or the one from the saved file if in Playback Mode.</returns>
        internal async Task<ItemResponse<DocumentStoreItem>> HandleMockedMethods(Func<Task<ItemResponse<DocumentStoreItem>>> method = null)
        {
            DocumentStoreItem document;
            if (_recordingMode == RecordingMode.Record)
            {
                try
                {
                    var response = await method().ConfigureAwait(false);

                    document = response.Resource;
                }

                // We need to store all CosmosExceptions so that we can re-throw them during playback.
                catch (CosmosException exception)
                {
                    document = ConvertCosmosExceptionToDocumentStoreItem(exception);

                    _testRecorder.AddRecordingToQueue(document);

                    throw;
                }

                _testRecorder.AddRecordingToQueue(document);
            }
            else
            {
                // In Playback mode, get the recorded DocumentStoreItem
                document = await _testRecorder.GetRecording().ConfigureAwait(false);

                // Because we record exceptions above, we need to re-throw them so they're handled by CosmosDbPartitionedStorage
                if (document?.Id == nameof(CosmosException))
                {
                    var exception = document.Document.ToObject<CosmosException>();
                    throw new CosmosException(exception.Message, exception.StatusCode, exception.SubStatusCode, exception.ActivityId, exception.RequestCharge);
                }
            }

            var mockItemResponse = new Mock<ItemResponse<DocumentStoreItem>>();
            mockItemResponse.SetupGet(x => x.Resource).Returns(document);

            return mockItemResponse.Object;
        }

        internal DocumentStoreItem ConvertCosmosExceptionToDocumentStoreItem(CosmosException exception)
        {
            // Some properties of CosmosException are more difficult to deserialize than it's worth,
            // so we'll remove some of them here.
            var exceptionObject = new
            {
                exception.Message,
                exception.StatusCode,
                exception.SubStatusCode,
                exception.ActivityId,
                exception.RequestCharge
            };

            var document = new DocumentStoreItem
            {
                Document = JObject.FromObject(exceptionObject),
                ETag = new Guid().ToString(),
                Id = nameof(CosmosException),
            };

            // RealId is internal and required for Cosmos, so we'll use reflection to set it here.
            document.GetType().GetProperty("RealId").SetValue(document, nameof(CosmosException));

            return document;
        }
    }
}
