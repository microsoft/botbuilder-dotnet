// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
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
    public class AzureBlobTranscriptStoreTests : StorageBaseTests
    {
        public TestContext TestContext { get; set; }

        private AzureBlobTranscriptStore _transcriptStore;

        private const string ConnectionString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        private const string ContainerName = "transcripttestblob";
        private const string ChannelId = "test";

        private static readonly string[] LongId = {
        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq1234567890Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq098765432112345678900987654321Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq123456789009876543211234567890Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq09876543211234567890098765432112345678900987654321"};
        private static readonly string[] ConversationIds =
        {
            "qaz", "wsx", "edc", "rfv", "tgb", "yhn", "ujm", "123", "456", "789",
            "ZAQ", "XSW", "CDE", "VFR", "BGT", "NHY", "NHY", "098", "765", "432",
            "zxc", "vbn", "mlk", "jhy", "yui", "kly", "asd", "asw", "aaa", "zzz"
        };
        private static readonly string[] ConversationSpecialIds = { "asd !&/#.'+:?\"", "ASD@123<>|}{][", "$%^;\\*()_" };
        private static readonly string[] ActivityIds =
        {
            "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
            "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
            "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
        };

        // These tests require Azure Storage Emulator v5.7
        [TestInitialize]
        public void TestInit()
        {
            StorageEmulatorHelper.StartStorageEmulator();
            _transcriptStore = new AzureBlobTranscriptStore(ConnectionString, ContainerName);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestCleanup]
        public void TestCleanUp()
        {
            StorageEmulatorHelper.StopStorageEmulator();
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void StorageNullTest()
        {
            Assert.IsNotNull(_transcriptStore);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptsEmptyTest()
        {
            var unusedChannelId = Guid.NewGuid().ToString();
            var transcripts = await _transcriptStore.ListTranscriptsAsync(unusedChannelId);
            Assert.AreEqual(transcripts.Items.Length, 0);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityEmptyTest()
        {
            foreach (var convoId in ConversationSpecialIds)
            {
                var activities = await _transcriptStore.GetTranscriptActivitiesAsync(ChannelId, convoId);
                Assert.AreEqual(activities.Items.Length, 0);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddTest()
        {
            var loggedActivities = new IActivity[5];
            var activities = new List<IActivity>();
            for (var i = 0; i < 5; i++)
            {
                var a = CreateActivity(i, i, ConversationIds);
                await _transcriptStore.LogActivityAsync(a);
                activities.Add(a);
                loggedActivities[i] = _transcriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]).Result.Items[0];
            }
            Assert.AreEqual(5, loggedActivities.Length);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptRemoveTest()
        {
            for (var i = 0; i < 5; i++)
            {
                var a = CreateActivity(i, i, ConversationIds);
                await _transcriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);

                var loggedActivities =
                    await _transcriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]);
                Assert.AreEqual(0, loggedActivities.Items.Length);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddSpecialCharsTest()
        {
            var loggedActivities = new IActivity[ConversationSpecialIds.Length];
            var activities = new List<IActivity>();
            for (var i = 0; i < ConversationSpecialIds.Length; i++)
            {
                var a = CreateActivity(i, i, ConversationSpecialIds);
                await _transcriptStore.LogActivityAsync(a);
                activities.Add(a);
                loggedActivities[i] = _transcriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationSpecialIds[i]).Result.Items[0];
            }
            Assert.AreEqual(activities.Count, loggedActivities.Length);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptRemoveSpecialCharsTest()
        {
            for (var i = 0; i < ConversationSpecialIds.Length; i++)
            {
                var a = CreateActivity(i, i, ConversationSpecialIds);
                await _transcriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);

                var loggedActivities =
                    await _transcriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationSpecialIds[i]);
                Assert.AreEqual(0, loggedActivities.Items.Length);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddPagedResultTest()
        {
            var cleanChanel = Guid.NewGuid().ToString();

            var loggedPagedResult = new PagedResult<IActivity>();
            var activities = new List<IActivity>();

            for (var i = 0; i < ConversationIds.Length; i++)
            {
                var a = CreateActivity(0, i, ConversationIds);
                a.ChannelId = cleanChanel;

                await _transcriptStore.LogActivityAsync(a);
                activities.Add(a);
            }
            loggedPagedResult = _transcriptStore.GetTranscriptActivitiesAsync(cleanChanel, ConversationIds[0]).Result;
            var ct = loggedPagedResult.ContinuationToken;
            Assert.AreEqual(20, loggedPagedResult.Items.Length);
            Assert.IsNotNull(ct);
            Assert.IsTrue(loggedPagedResult.ContinuationToken.Length > 0);
            loggedPagedResult = _transcriptStore.GetTranscriptActivitiesAsync(cleanChanel, ConversationIds[0], ct).Result;
            ct = loggedPagedResult.ContinuationToken;
            Assert.AreEqual(10, loggedPagedResult.Items.Length);
            Assert.IsNull(ct);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptRemovePagedTest()
        {
            var loggedActivities = new PagedResult<IActivity>();
            int i;
            for (i = 0; i < ConversationSpecialIds.Length; i++)
            {
                var a = CreateActivity(i, i, ConversationIds);
                await _transcriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);
            }
            loggedActivities =
                await _transcriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]);
            Assert.AreEqual(0, loggedActivities.Items.Length);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        [ExpectedException(typeof(StorageException))]
        public async Task LongIdAddTest()
        {
            var a = CreateActivity(0, 0, LongId);
            await _transcriptStore.LogActivityAsync(a);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void BlobParamTest()
        {
            Assert.ThrowsException<FormatException>(() => new AzureBlobTranscriptStore("123", ContainerName));

            Assert.ThrowsException<ArgumentNullException>(() =>
                new AzureBlobTranscriptStore((CloudStorageAccount)null, ContainerName));

            Assert.ThrowsException<ArgumentNullException>(() =>
                new AzureBlobTranscriptStore((string)null, ContainerName));

            Assert.ThrowsException<ArgumentNullException>(() =>
                new AzureBlobTranscriptStore((CloudStorageAccount)null, null));

            Assert.ThrowsException<ArgumentNullException>(() => new AzureBlobTranscriptStore((string)null, null));
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task NullBlobTest()
        {
            AzureBlobTranscriptStore store = null;

            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await store.LogActivityAsync(CreateActivity(0, 0, ConversationIds)));
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await store.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[0]));
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogActivities()
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(_transcriptStore));

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

            var pagedResult = await _transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
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

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogUpdateActivities()
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(_transcriptStore));
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

            var pagedResult = await _transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
            Assert.AreEqual(3, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[2].AsMessageActivity().Text);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task TestDateLogUpdateActivities()
        {
            var dateTimeStartOffset1 = new DateTimeOffset(DateTime.Now);
            var dateTimeStartOffset2 = new DateTimeOffset(DateTime.UtcNow);
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(_transcriptStore));
            string conversationId = null;
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
            var pagedResult = await _transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, dateTimeStartOffset1.DateTime);
            Assert.AreEqual(3, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[2].AsMessageActivity().Text);
            // Perform some queries
            pagedResult = await _transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, DateTimeOffset.MinValue);
            Assert.AreEqual(3, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[2].AsMessageActivity().Text);
            // Perform some queries
            pagedResult = await _transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, DateTimeOffset.MaxValue);
            Assert.AreEqual(0, pagedResult.Items.Length);

        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task LogDeleteActivities()
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(_transcriptStore));
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

            var pagedResult = await _transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
            Assert.AreEqual(3, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[1].AsMessageDeleteActivity());
            Assert.AreEqual(ActivityTypes.MessageDelete, pagedResult.Items[1].Type);
            Assert.AreEqual("deleteIt", pagedResult.Items[2].AsMessageActivity().Text);
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
                Recipient = new ChannelAccount("testBot")
            };
        }
    }
    public static class StorageEmulatorHelper
    {
        /* Usage:
         * ======
           AzureStorageEmulator.exe init            : Initialize the emulator database and configuration.
           AzureStorageEmulator.exe start           : Start the emulator.
           AzureStorageEmulator.exe stop            : Stop the emulator.
           AzureStorageEmulator.exe status          : Get current emulator status.
           AzureStorageEmulator.exe clear           : Delete all data in the emulator.
           AzureStorageEmulator.exe help [command]  : Show general or command-specific help.
         */
        public enum StorageEmulatorCommand
        {
            Init,
            Start,
            Stop,
            Status,
            Clear
        }

        public static int StartStorageEmulator()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Start);
        }

        public static int StopStorageEmulator()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Stop);
        }

        public static int ExecuteStorageEmulatorCommand(StorageEmulatorCommand command)
        {
            var emulatorPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft SDKs",
                "Azure",
                "Storage Emulator",
                "AzureStorageEmulator.exe");

            var start = new ProcessStartInfo
            {
                Arguments = command.ToString(),
                FileName = emulatorPath
            };
            var exitCode = ExecuteProcess(start);
            return exitCode;
        }

        private static int ExecuteProcess(ProcessStartInfo startInfo)
        {
            int exitCode = -1;
            using (var proc = new Process { StartInfo = startInfo })
            {
                proc.Start();
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            return exitCode;
        }
    }
}
