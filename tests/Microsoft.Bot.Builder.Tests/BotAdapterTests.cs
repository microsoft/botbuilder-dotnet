// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotAdapterTests
    {
        [Fact]
        public void AdapterSingleUse()
        {
            var a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware());
        }

        [Fact]
        public void AdapterUseChaining()
        {
            var a = new SimpleAdapter();
            a.Use(new CallCountingMiddleware()).Use(new CallCountingMiddleware());
        }

        [Fact]
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
            Assert.True(resourceResponse.Id == activityId, "Incorrect response Id returned");
        }

        [Fact]
        public async Task GetLocaleFromActivity()
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
            activity.Locale = "de-DE";

            Task SimpleCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                Assert.Equal("de-DE", turnContext.Activity.Locale);
                return Task.CompletedTask;
            }

            await a.ProcessRequest(activity, SimpleCallback, default(CancellationToken));
        }

        [Fact]
        public async Task ContinueConversation_DirectMsgAsync()
        {
            bool callbackInvoked = false;
            var adapter = new TestAdapter(TestAdapter.CreateConversation("ContinueConversation_DirectMsgAsync"));
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
            Assert.True(callbackInvoked);
        }
    }
}
