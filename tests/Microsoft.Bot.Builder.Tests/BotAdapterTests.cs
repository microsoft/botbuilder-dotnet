// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("BotAdapter")]
    public class BotAdapterTests
    {
        [TestMethod]
        public void AdapterSingleUse()
        {
            var a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware());
        }

        [TestMethod]
        public void AdapterUseChaining()
        {
            var a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()).Use(new CallCountingMiddleware());
        }

        [TestMethod]
        public async Task PassResourceResponsesThrough()
        {
            void ValidateResponses(Activity[] activities)
            {
                // no need to do anything.
            }

            var a = new SimpleAdapter(ValidateResponses);
            var c = new TurnContext(a, new Activity());

            var activityId = Guid.NewGuid().ToString();
            var activity = TestMessage.Message();
            activity.Id = activityId;

            var resourceResponse = await c.SendActivityAsync(activity);
            Assert.IsTrue(resourceResponse.Id == activityId, "Incorrect response Id returned");
        }

        [TestMethod]
        public async Task ContinueConversation_DirectMsgAsync()
        {
            bool callbackInvoked = false;
            var adapter = new TestAdapter();
            ConversationReference cr = new ConversationReference
            {
                ActivityId = "activityId",
                Bot = new ChannelAccount
                {
                    Id = "channelId",
                    Name = "testChannelAccount",
                    Role = "bot",
                },
                ChannelId = "testChannel",
                ServiceUrl = "testUrl",
                Conversation = new ConversationAccount
                {
                    ConversationType = string.Empty,
                    Id = "testConversationId",
                    IsGroup = false,
                    Name = "testConversationName",
                    Role = "user",
                },
                User = new ChannelAccount
                {
                    Id = "channelId",
                    Name = "testChannelAccount",
                    Role = "bot",
                },
            };
            Task ContinueCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await adapter.ContinueConversationAsync("MyBot", cr, ContinueCallback, default(CancellationToken));
            Assert.IsTrue(callbackInvoked);
        }
    }
}
