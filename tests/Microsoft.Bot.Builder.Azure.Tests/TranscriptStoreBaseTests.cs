// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Activity = Microsoft.Bot.Schema.Activity;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    /// <summary>
    /// Base tests for <seealso cref="BlobsTranscriptStoreTests"/> and <seealso cref="AzureBlobTranscriptStoreTests"/>.
    /// </summary>
    public abstract class TranscriptStoreBaseTests
    {
        protected const string BlobStorageEmulatorConnectionString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        protected static readonly string[] LongId =
        {
            "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq1234567890Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq098765432112345678900987654321Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq123456789009876543211234567890Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq09876543211234567890098765432112345678900987654321",
        };

        private const string ChannelId = "test";

        private static readonly string[] ConversationIds =
        {
            "qaz", "wsx", "edc", "rfv", "tgb", "yhn", "ujm", "123", "456", "789",
            "ZAQ", "XSW", "CDE", "VFR", "BGT", "NHY", "NHY", "098", "765", "432",
            "zxc", "vbn", "mlk", "jhy", "yui", "kly", "asd", "asw", "aaa", "zzz",
        };

        private static readonly string[] ConversationSpecialIds = { "asd !&/#.'+:?\"", "ASD@123<>|}{][", "$%^;\\*()_" };

        protected abstract ITranscriptStore TranscriptStore { get; }

        protected abstract string ContainerName { get; }

        [TestMethod]
        public async Task TranscriptsEmptyTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var unusedChannelId = Guid.NewGuid().ToString();
                var transcripts = await TranscriptStore.ListTranscriptsAsync(unusedChannelId);
                Assert.AreEqual(transcripts.Items.Length, 0);
            }
        }

        [TestMethod]
        public async Task ActivityEmptyTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                foreach (var convoId in ConversationSpecialIds)
                {
                    var activities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, convoId);
                    Assert.AreEqual(activities.Items.Length, 0);
                }
            }
        }

        [TestMethod]
        public async Task ActivityAddTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var loggedActivities = new IActivity[5];
                var activities = new List<IActivity>();
                for (var i = 0; i < 5; i++)
                {
                    var a = CreateActivity(i, i, ConversationIds);
                    await TranscriptStore.LogActivityAsync(a);
                    activities.Add(a);
                    loggedActivities[i] = TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]).Result.Items[0];
                }

                Assert.AreEqual(5, loggedActivities.Length);
            }
        }

        [TestMethod]
        public async Task TranscriptRemoveTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                for (var i = 0; i < 5; i++)
                {
                    var a = CreateActivity(i, i, ConversationIds);
                    await TranscriptStore.LogActivityAsync(a);
                    await TranscriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);

                    var loggedActivities =
                        await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]);
                    Assert.AreEqual(0, loggedActivities.Items.Length);
                }
            }
        }

        [TestMethod]
        public async Task ActivityAddSpecialCharsTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var loggedActivities = new IActivity[ConversationSpecialIds.Length];
                var activities = new List<IActivity>();
                for (var i = 0; i < ConversationSpecialIds.Length; i++)
                {
                    var a = CreateActivity(i, i, ConversationSpecialIds);
                    await TranscriptStore.LogActivityAsync(a);
                    activities.Add(a);
                    loggedActivities[i] = TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationSpecialIds[i]).Result.Items[0];
                }

                Assert.AreEqual(activities.Count, loggedActivities.Length);
            }
        }

        [TestMethod]
        public async Task TranscriptRemoveSpecialCharsTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                for (var i = 0; i < ConversationSpecialIds.Length; i++)
                {
                    var a = CreateActivity(i, i, ConversationSpecialIds);
                    await TranscriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);

                    var loggedActivities =
                        await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationSpecialIds[i]);
                    Assert.AreEqual(0, loggedActivities.Items.Length);
                }
            }
        }

        [TestMethod]
        public async Task ActivityAddPagedResultTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var cleanChanel = Guid.NewGuid().ToString();

                var loggedPagedResult = new PagedResult<IActivity>();
                var activities = new List<IActivity>();

                for (var i = 0; i < ConversationIds.Length; i++)
                {
                    var a = CreateActivity(0, i, ConversationIds);
                    a.ChannelId = cleanChanel;

                    await TranscriptStore.LogActivityAsync(a);
                    activities.Add(a);
                }

                loggedPagedResult = TranscriptStore.GetTranscriptActivitiesAsync(cleanChanel, ConversationIds[0]).Result;
                var ct = loggedPagedResult.ContinuationToken;
                Assert.AreEqual(20, loggedPagedResult.Items.Length);
                Assert.IsNotNull(ct);
                Assert.IsTrue(loggedPagedResult.ContinuationToken.Length > 0);
                loggedPagedResult = TranscriptStore.GetTranscriptActivitiesAsync(cleanChanel, ConversationIds[0], ct).Result;
                ct = loggedPagedResult.ContinuationToken;
                Assert.AreEqual(10, loggedPagedResult.Items.Length);
                Assert.IsNull(ct);
            }
        }

        [TestMethod]
        public async Task TranscriptRemovePagedTest()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var loggedActivities = new PagedResult<IActivity>();
                int i;
                for (i = 0; i < ConversationSpecialIds.Length; i++)
                {
                    var a = CreateActivity(i, i, ConversationIds);
                    await TranscriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);
                }

                loggedActivities =
                    await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]);
                Assert.AreEqual(0, loggedActivities.Items.Length);
            }
        }

        [TestMethod]
        public async Task NullParameterTests()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var store = TranscriptStore;

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    await store.LogActivityAsync(null));
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    await store.GetTranscriptActivitiesAsync(null, ConversationIds[0]));
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                    await store.GetTranscriptActivitiesAsync(ChannelId, null));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogActivities()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                TestAdapter adapter = new TestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(TranscriptStore));

                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    var typingActivity = new Activity
                    {
                        Type = ActivityTypes.Typing,
                        RelatesTo = context.Activity.RelatesTo
                    };
                    await context.SendActivityAsync(typingActivity);
                    await Task.Delay(500);
                    await context.SendActivityAsync("echo:" + context.Activity.Text);
                })
                    .Send("foo")
                        .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                        .AssertReply("echo:foo")
                    .Send("bar")
                        .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                        .AssertReply("echo:bar")
                    .StartTestAsync();

                await Task.Delay(1000);

                var pagedResult = await TranscriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
                Assert.AreEqual(6, pagedResult.Items.Length);
                Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
                Assert.IsNotNull(pagedResult.Items[1].AsTypingActivity());
                Assert.AreEqual("echo:foo", pagedResult.Items[2].AsMessageActivity().Text);
                Assert.AreEqual("bar", pagedResult.Items[3].AsMessageActivity().Text);
                Assert.IsNotNull(pagedResult.Items[4].AsTypingActivity());
                Assert.AreEqual("echo:bar", pagedResult.Items[5].AsMessageActivity().Text);
                foreach (var activity in pagedResult.Items)
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
                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                TestAdapter adapter = new TestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(TranscriptStore));
                Activity activityToUpdate = null;
                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    if (context.Activity.Text == "update")
                    {
                        activityToUpdate.Text = "new response";
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

                var pagedResult = await TranscriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
                Assert.AreEqual(3, pagedResult.Items.Length);
                Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
                Assert.AreEqual("new response", pagedResult.Items[1].AsMessageActivity().Text);
                Assert.AreEqual("update", pagedResult.Items[2].AsMessageActivity().Text);
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task TestDateLogUpdateActivities()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var dateTimeStartOffset1 = new DateTimeOffset(DateTime.Now);
                var dateTimeStartOffset2 = new DateTimeOffset(DateTime.UtcNow);
                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                TestAdapter adapter = new TestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(TranscriptStore));
                Activity activityToUpdate = null;
                await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    if (context.Activity.Text == "update")
                    {
                        activityToUpdate.Text = "new response";
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

                await Task.Delay(5000);

                // Perform some queries
                var pagedResult = await TranscriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, dateTimeStartOffset1.DateTime);
                Assert.AreEqual(3, pagedResult.Items.Length);
                Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
                Assert.AreEqual("new response", pagedResult.Items[1].AsMessageActivity().Text);
                Assert.AreEqual("update", pagedResult.Items[2].AsMessageActivity().Text);

                // Perform some queries
                pagedResult = await TranscriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, DateTimeOffset.MinValue);
                Assert.AreEqual(3, pagedResult.Items.Length);
                Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
                Assert.AreEqual("new response", pagedResult.Items[1].AsMessageActivity().Text);
                Assert.AreEqual("update", pagedResult.Items[2].AsMessageActivity().Text);

                // Perform some queries
                pagedResult = await TranscriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, DateTimeOffset.MaxValue);
                Assert.AreEqual(0, pagedResult.Items.Length);
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogDeleteActivities()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
                TestAdapter adapter = new TestAdapter(conversation)
                    .Use(new TranscriptLoggerMiddleware(TranscriptStore));
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

                var pagedResult = await TranscriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
                Assert.AreEqual(3, pagedResult.Items.Length);
                Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
                Assert.IsNotNull(pagedResult.Items[1].AsMessageDeleteActivity());
                Assert.AreEqual(ActivityTypes.MessageDelete, pagedResult.Items[1].Type);
                Assert.AreEqual("deleteIt", pagedResult.Items[2].AsMessageActivity().Text);
            }
        }

        protected static Activity CreateActivity(int i, int j, string[] conversationIds)
        {
            return CreateActivity(j, conversationIds[i]);
        }

        private static Activity CreateActivity(int j, string conversationId)
        {
            return new Activity
            {
                Id = (j + 1).ToString().PadLeft(2, '0'),
                ChannelId = "test",
                Text = "test",
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount(id: conversationId),
                Timestamp = DateTime.Now,
                From = new ChannelAccount("testUser"),
                Recipient = new ChannelAccount("testBot"),
            };
        }
    }
}
