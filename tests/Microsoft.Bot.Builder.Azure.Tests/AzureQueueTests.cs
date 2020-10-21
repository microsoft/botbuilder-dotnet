// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Azure.Queues;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class AzureQueueTests : StorageBaseTests
    {
        private const string ConnectionString = @"UseDevelopmentStorage=true";

        // These tests require Azure Storage Emulator v5.7
        public async Task<QueueClient> ContainerInit(string name)
        {
            var queue = new QueueClient(ConnectionString, name);
            await queue.CreateIfNotExistsAsync();
            await queue.ClearMessagesAsync();
            return queue;
        }

        [Fact]
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

                var queueStorage = new AzureQueueStorage(ConnectionString, queueName);
                var dm = new DialogManager(new ContinueConversationLater()
                {
                    Date = "=addSeconds(utcNow(), 2)",
                    Value = "foo"
                });

                dm.InitialTurnState.Set<QueueStorage>(queueStorage);

                await new TestFlow((TestAdapter)adapter, dm.OnTurnAsync)
                    .Send("hi")
                    .StartTestAsync();
                await Task.Delay(2000);
                var messages = await queue.ReceiveMessagesAsync();
                var message = messages.Value[0];
                var messageJson = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                var activity = JsonConvert.DeserializeObject<Activity>(messageJson);
                Assert.Equal(ActivityTypes.Event, activity.Type);
                Assert.Equal(ActivityEventNames.ContinueConversation, activity.Name);
                Assert.Equal("foo", activity.Value);
                Assert.NotNull(activity.RelatesTo);
                var cr2 = activity.GetConversationReference();
                cr.ActivityId = null;
                cr2.ActivityId = null;
                Assert.Equal(JsonConvert.SerializeObject(cr), JsonConvert.SerializeObject(cr2));
            }
        }
    }
}
