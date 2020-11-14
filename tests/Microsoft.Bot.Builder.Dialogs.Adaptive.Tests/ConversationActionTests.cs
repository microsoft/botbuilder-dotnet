// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class ConversationActionTests
    {
        [Fact]
        public async Task ContinueConversationLaterTests()
        {
            var queueName = nameof(ContinueConversationLaterTests).ToLower();

            var cr = TestAdapter.CreateConversation(nameof(ContinueConversationLaterTests));
            var adapter = new TestAdapter(cr)
                   .UseStorage(new MemoryStorage())
                   .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            var queueStorage = new MockQueue();
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
            var activity = await queueStorage.ReceiveActivity();
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(ActivityEventNames.ContinueConversation, activity.Name);
            Assert.Equal("foo", activity.Value);
            Assert.NotNull(activity.RelatesTo);
            var cr2 = activity.GetConversationReference();
            cr.ActivityId = null;
            cr2.ActivityId = null;
            Assert.Equal(JsonConvert.SerializeObject(cr), JsonConvert.SerializeObject(cr2));
        }

        [Fact]
        public async Task ContinueConversationTests()
        {
            var conv1 = $"{nameof(ContinueConversationTests)}1";
            var conv2 = $"{nameof(ContinueConversationTests)}2";
            var cr1 = TestAdapter.CreateConversation(conv1);
            var cr2 = TestAdapter.CreateConversation(conv2);
            var adapter = new TestAdapter(cr1)
                   .UseStorage(new MemoryStorage())
                   .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            var queueStorage = new MockQueue();
            var dm = new DialogManager(new AdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                    {
                        new OnMessageActivity()
                        {
                            Actions = new List<Dialog>()
                            {
                                new ContinueConversation()
                                {
                                    ConversationReference = cr2,
                                    Value = "foo"
                                },
                                new SendActivity() { Activity = new ActivityTemplate("ContinueConversation Sent") }
                            }
                        }
                    }
            });

            dm.InitialTurnState.Set<QueueStorage>(queueStorage);

            await new TestFlow((TestAdapter)adapter, dm.OnTurnAsync)
                .Send("hi")
                    .AssertReply("ContinueConversation Sent")
                .StartTestAsync();
            var activity = await queueStorage.ReceiveActivity();
            Assert.Equal(ActivityTypes.Event, activity.Type);
            Assert.Equal(ActivityEventNames.ContinueConversation, activity.Name);
            Assert.Equal("foo", activity.Value);
            Assert.NotNull(activity.RelatesTo);
            var crReceived = activity.GetConversationReference();
            cr2.ActivityId = null;
            crReceived.ActivityId = null;
            Assert.Equal(JsonConvert.SerializeObject(cr2), JsonConvert.SerializeObject(crReceived));
        }

        [Fact]
        public async Task GetConversationReferenceTest()
        {
            var conv1 = $"{nameof(ContinueConversationTests)}1";
            var cr1 = TestAdapter.CreateConversation(conv1);
            var adapter = new TestAdapter(cr1)
                   .UseStorage(new MemoryStorage())
                   .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new MemoryStorage()));

            var queueStorage = new MockQueue();
            var dm = new DialogManager(new AdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                    {
                        new OnMessageActivity()
                        {
                            Actions = new List<Dialog>()
                            {
                                new GetConversationReference()
                                {
                                    Property = "$cr"
                                },
                                new AssertCondition() { Condition = "$cr.channelId == 'test' " },
                                new AssertCondition() { Condition = $"$cr.conversation.id == '{cr1.Conversation.Id}' " },
                                new AssertCondition() { Condition = $"$cr.conversation.id == turn.activity.conversation.id" },
                                new AssertCondition() { Condition = $"$cr.bot.id == turn.activity.recipient.id" },
                                new AssertCondition() { Condition = $"$cr.user.id == turn.activity.from.id" },
                                new SendActivity("ok")
                            }
                        }
                    }
            });

            dm.InitialTurnState.Set<QueueStorage>(queueStorage);

            await new TestFlow((TestAdapter)adapter, dm.OnTurnAsync)
                .Send("hi")
                    .AssertReply($"ok")
                .StartTestAsync();
        }
    }
}
