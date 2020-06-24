// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Azure.Queues;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class AzureQueueTests : StorageBaseTests
    {
        private const string ConnectionString = @"UseDevelopmentStorage=true";

        public TestContext TestContext { get; set; }

        // These tests require Azure Storage Emulator v5.7
        public async Task<QueueClient> ContainerInit(string name)
        {
            var queue = new QueueClient(ConnectionString, name);
            await queue.CreateIfNotExistsAsync();
            await queue.ClearMessagesAsync();
            return queue;
        }

        [TestMethod]
        public async Task ContinueConversationLaterTests()
        {
            if (StorageEmulatorHelper.CheckEmulator())
            {
                var queueName = nameof(ContinueConversationLaterTests).ToLower();
                var queue = await ContainerInit(queueName);
                var cr = TestAdapter.CreateConversation(nameof(ContinueConversationLaterTests));
                var adapter = new TestAdapter(cr)
                       .UseStorage(new MemoryStorage())
                       .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

                var dm = new DialogManager(new ContinueConversationLater()
                {
                    ConnectionString = ConnectionString,
                    QueueName = queueName,
                    Date = "=addSeconds(utcNow(), 2)",
                    Value = "foo"
                });
                await new TestFlow((TestAdapter)adapter, dm.OnTurnAsync)
                    .Send("hi")
                    .StartTestAsync();
                await Task.Delay(2000);
                var messages = await queue.ReceiveMessagesAsync();
                var message = messages.Value[0];
                var messageJson = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                var activity = JsonConvert.DeserializeObject<Activity>(messageJson);
                Assert.AreEqual(ActivityTypes.Event, activity.Type);
                Assert.AreEqual("ContinueConversation", activity.Name);
                Assert.AreEqual("foo", activity.Value);
                Assert.IsNotNull(activity.RelatesTo);
                var cr2 = activity.GetConversationReference();
                cr.ActivityId = null;
                cr2.ActivityId = null;
                Assert.AreEqual(JsonConvert.SerializeObject(cr), JsonConvert.SerializeObject(cr2));
            }
        }
    }
}
