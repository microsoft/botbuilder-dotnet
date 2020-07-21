// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Azure.Storage;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Activity = Microsoft.Bot.Schema.Activity;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - BlobTranscripts")]
    public class BlobsTranscriptLoggerTests : StorageBaseTests
    {
        private const string ConnectionString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        
        private static readonly string[] LongId =
        {
            "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq1234567890Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq098765432112345678900987654321Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq123456789009876543211234567890Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq09876543211234567890098765432112345678900987654321",
        };

        private static readonly string[] ConversationIds =
        {
            "qaz", "wsx", "edc", "rfv", "tgb", "yhn", "ujm", "123", "456", "789",
            "ZAQ", "XSW", "CDE", "VFR", "BGT", "NHY", "NHY", "098", "765", "432",
            "zxc", "vbn", "mlk", "jhy", "yui", "kly", "asd", "asw", "aaa", "zzz",
        };

        private static readonly string[] ConversationSpecialIds = { "asd !&/#.'+:?\"", "ASD@123<>|}{][", "$%^;\\*()_" };
        private static readonly string[] ActivityIds =
        {
            "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
            "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
            "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
        };

        public string ContainerName
        {
            get { return $"blobs{TestContext.TestName.ToLower()}"; }
        }

        public TestContext TestContext { get; set; }

        public BlobsTranscriptLoggerTest TranscriptLogger
        {
            get { return new BlobsTranscriptLoggerTest(ConnectionString, ContainerName); }
        }

        [TestInitialize]
        public async Task BeforeTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var client = new BlobContainerClient(ConnectionString, ContainerName);
                await client.DeleteIfExistsAsync();
                await client.CreateIfNotExistsAsync();
            }
        }

        [TestCleanup]
        public async Task AfterTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var client = new BlobContainerClient(ConnectionString, ContainerName);
                await client.DeleteIfExistsAsync();
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var logger = TranscriptLogger;

                var loggedActivities = new IActivity[5];
                var activities = new List<IActivity>();
                for (var i = 0; i < 5; i++)
                {
                    var a = CreateActivity(i, i, ConversationIds);
                    await logger.LogActivityAsync(a);
                    activities.Add(a);
                    loggedActivities[i] = await logger.GetStoredActivityAsync(a);
                }

                Assert.AreEqual(5, loggedActivities.Length);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddSpecialCharsTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var logger = TranscriptLogger;

                var loggedActivities = new IActivity[ConversationSpecialIds.Length];
                var activities = new List<IActivity>();
                for (var i = 0; i < ConversationSpecialIds.Length; i++)
                {
                    var a = CreateActivity(i, i, ConversationSpecialIds);
                    await logger.LogActivityAsync(a);
                    activities.Add(a);
                    loggedActivities[i] = await logger.GetStoredActivityAsync(a);
                }

                Assert.AreEqual(activities.Count, loggedActivities.Length);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task LongIdAddTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                try
                {
                    var a = CreateActivity(0, 0, LongId);

                    await TranscriptLogger.LogActivityAsync(a);
                    Assert.Fail("Should have thrown ");
                }
                catch (System.Xml.XmlException xmlEx)
                {
                    // Unfortunately, Azure.Storage.Blobs v12.4.4 currently throws this XmlException for long keys :(
                    if (xmlEx.Message == "'\"' is an unexpected token. Expecting whitespace. Line 1, position 50.")
                    {
                        return;
                    }
                }

                //catch (RequestFailedException ex)
                //when ((HttpStatusCode)ex.Status == HttpStatusCode.PreconditionFailed)
                //{
                //    return;
                //}

                Assert.Fail("Should have thrown ");
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void BlobTranscriptParamTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(null, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(ConnectionString, null));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(string.Empty, ContainerName));

                Assert.ThrowsException<ArgumentNullException>(() =>
                    new BlobsTranscriptLogger(ConnectionString, string.Empty));
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task NullParameterTests()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var logger = TranscriptLogger;
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    await logger.LogActivityAsync(null));
                await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                    await logger.GetStoredActivityAsync(null));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogActivities()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var logger = TranscriptLogger;
                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                var adapter = new LoggingTestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(logger)) as LoggingTestAdapter;

                await new TestFlow(adapter, async (context, cancellationToken) =>
                    {
                        var typingActivity = new Activity
                        {
                            Id = $"typing_{Guid.NewGuid()}",
                            Type = ActivityTypes.Typing,
                            RelatesTo = context.Activity.RelatesTo
                        };
                        await context.SendActivityAsync(typingActivity);
                        await Task.Delay(500);
                        var echoActivity = MessageFactory.Text("echo:" + context.Activity.Text);
                        await context.SendActivityAsync(echoActivity);
                        await Task.Delay(500);
                    })
                    .Send("foo")
                        .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                        .AssertReply("echo:foo")
                    .Send("bar")
                        .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                        .AssertReply("echo:bar")
                    .StartTestAsync();

                await Task.Delay(1000);

                var foundActivities = adapter.ProcessedActivities.Union(adapter.SentActivities)
                                            .Select(a => logger.GetStoredActivityAsync(a).Result)
                                            .OrderBy(a => a.Timestamp)
                                            .ToList();

                Assert.AreEqual(6, foundActivities.Count);
                Assert.AreEqual("foo", foundActivities[0].AsMessageActivity().Text);
                Assert.IsNotNull(foundActivities[1].AsTypingActivity());
                Assert.AreEqual("echo:foo", foundActivities[2].AsMessageActivity().Text);
                Assert.AreEqual("bar", foundActivities[3].AsMessageActivity().Text);
                Assert.IsNotNull(foundActivities[4].AsTypingActivity());
                Assert.AreEqual("echo:bar", foundActivities[5].AsMessageActivity().Text);
                foreach (var activity in foundActivities)
                {
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(activity.Id));
                    Assert.IsTrue(activity.Timestamp > default(DateTimeOffset));
                }
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogUpdateActivities()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var logger = TranscriptLogger;

                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                var adapter = new LoggingTestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(logger)) as LoggingTestAdapter;
                Activity activityToUpdate = null;
                var activities = new List<Activity>();
                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    if (context.Activity.Text == "update")
                    {
                        activityToUpdate.Text = "new response";
                        
                        // Set the Timestamp before calling update, to ensure the test
                        // finds the activity based on channel/conversation.id/activity.id
                        activityToUpdate.Timestamp = DateTimeOffset.UtcNow;
                        await context.UpdateActivityAsync(activityToUpdate);
                    }
                    else
                    {
                        var activity = context.Activity.CreateReply("response");
                        var response = await context.SendActivityAsync(activity);
                        activity.Id = response.Id;

                        // clone the activity, so we can use it to do an update
                        activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                    }
                })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();

                await Task.Delay(1000);

                var foundActivities = adapter.ProcessedActivities.Union(adapter.SentActivities)
                                    .Select(a => logger.GetStoredActivityAsync(a).Result)
                                    .OrderBy(a => a.Timestamp)
                                    .ToList();

                Assert.AreEqual(3, foundActivities.Count);
                Assert.AreEqual("foo", foundActivities[0].AsMessageActivity().Text);
                Assert.AreEqual("new response", foundActivities[1].AsMessageActivity().Text);
                Assert.AreEqual("update", foundActivities[2].AsMessageActivity().Text);
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task BlobsLogDeleteActivities()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var logger = TranscriptLogger;

                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                var adapter = new LoggingTestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(logger)) as LoggingTestAdapter;
                string activityId = null;
                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    if (context.Activity.Text == "deleteIt")
                    {
                        await context.DeleteActivityAsync(activityId);
                    }
                    else
                    {
                        var activity = context.Activity.CreateReply("response");
                        var response = await context.SendActivityAsync(activity);
                        activityId = response.Id;
                    }
                })
                    .Send("foo")
                        .AssertReply("response")
                    .Send("deleteIt")
                    .StartTestAsync();

                await Task.Delay(1000);

                var foundActivities = adapter.ProcessedActivities.Union(adapter.SentActivities)
                                    .Select(a => logger.GetStoredActivityAsync(a).Result)
                                    .OrderBy(a => a.Timestamp)
                                    .ToList();

                Assert.AreEqual(3, foundActivities.Count);
                Assert.AreEqual("foo", foundActivities[0].AsMessageActivity().Text);
                Assert.IsNotNull(foundActivities[1].AsMessageDeleteActivity());
                Assert.AreEqual(ActivityTypes.MessageDelete, foundActivities[1].Type);
                Assert.AreEqual("deleteIt", foundActivities[2].AsMessageActivity().Text);
            }
        }

        private static Activity CreateActivity(int i, int j, string[] conversationIds)
        {
            return new Activity
            {
                Id = ActivityIds[j],
                ChannelId = "test",
                Text = "test",
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount(id: conversationIds[i]),
                Timestamp = DateTime.Now,
                From = new ChannelAccount("testUser"),
                Recipient = new ChannelAccount("testBot"),
            };
        }

        public class BlobsTranscriptLoggerTest : BlobsTranscriptLogger
        {
            public BlobsTranscriptLoggerTest(string connectionString, string containerName)
                : base(connectionString, containerName)
            {
            }

            public async Task<Activity> GetStoredActivityAsync(IActivity fromActivity)
            {
                return await GetActivityAsync(fromActivity);
            }
        }

        public class LoggingTestAdapter : TestAdapter
        {
            public LoggingTestAdapter(ConversationReference conversation = null, bool sendTraceActivity = false)
                : base(conversation, sendTraceActivity)
            {
            }

            public List<Activity> ProcessedActivities { get; set; } = new List<Activity>();

            public List<Activity> SentActivities { get; set; } = new List<Activity>();

            public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
            {
                SentActivities.AddRange(activities);
                return base.SendActivitiesAsync(turnContext, activities, cancellationToken);
            }

            public override Task ProcessActivityAsync(Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken = default)
            {
                ProcessedActivities.Add(activity);
                return base.ProcessActivityAsync(activity, callback, cancellationToken);
            }
        }
    }
}
