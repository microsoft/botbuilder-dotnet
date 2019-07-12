// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
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
        private const string ConnectionString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        private const string ChannelId = "test";

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

        public TestContext TestContext { get; set; }

        public string ContainerName { get { return TestContext.TestName.ToLower(); } }

        public AzureBlobTranscriptStore TranscriptStore { get { return new AzureBlobTranscriptStore(ConnectionString, ContainerName); } }

        // These tests require Azure Storage Emulator v5.7
        [ClassInitialize]
        public static void ClassInitialize(TestContext a)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                StorageEmulatorHelper.StartStorageEmulator();
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestInitialize]
        public void TestInit()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var container = CloudStorageAccount.Parse(ConnectionString)
                .CreateCloudBlobClient()
                .GetContainerReference(ContainerName);
                container.DeleteIfExists();
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void StorageNullTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            Assert.IsNotNull(TranscriptStore);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptsEmptyTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var unusedChannelId = Guid.NewGuid().ToString();
            var transcripts = await TranscriptStore.ListTranscriptsAsync(unusedChannelId);
            Assert.AreEqual(transcripts.Items.Length, 0);
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityEmptyTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            foreach (var convoId in ConversationSpecialIds)
            {
                var activities = await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, convoId);
                Assert.AreEqual(activities.Items.Length, 0);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptRemoveTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            for (var i = 0; i < 5; i++)
            {
                var a = CreateActivity(i, i, ConversationIds);
                await TranscriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);

                var loggedActivities =
                    await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[i]);
                Assert.AreEqual(0, loggedActivities.Items.Length);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddSpecialCharsTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptRemoveSpecialCharsTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            for (var i = 0; i < ConversationSpecialIds.Length; i++)
            {
                var a = CreateActivity(i, i, ConversationSpecialIds);
                await TranscriptStore.DeleteTranscriptAsync(a.ChannelId, a.Conversation.Id);

                var loggedActivities =
                    await TranscriptStore.GetTranscriptActivitiesAsync(ChannelId, ConversationSpecialIds[i]);
                Assert.AreEqual(0, loggedActivities.Items.Length);
            }
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task ActivityAddPagedResultTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task TranscriptRemovePagedTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public async Task LongIdAddTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            try
            {
                var a = CreateActivity(0, 0, LongId);
    
                await TranscriptStore.LogActivityAsync(a);
                Assert.Fail("Should have thrown ");
            }
            catch(StorageException err)
            {}
            Assert.Fail("Should have thrown ");
        }

        // These tests require Azure Storage Emulator v5.7
        [TestMethod]
        public void BlobParamTest()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            AzureBlobTranscriptStore store = null;

            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await store.LogActivityAsync(CreateActivity(0, 0, ConversationIds)));
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await store.GetTranscriptActivitiesAsync(ChannelId, ConversationIds[0]));
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
    }
}
