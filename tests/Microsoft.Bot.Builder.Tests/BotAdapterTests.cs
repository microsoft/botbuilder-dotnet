// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector.Authentication;
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

        [TestMethod]
        public async Task ContinueConversation_AppIdNotInformedAsync()
        {
            bool callbackInvoked = false;
            var adapter = new BotFrameworkAdapter(new SimpleCredentialProvider("AppId", "AppSecret"));
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
                ServiceUrl = "https://test.com",
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

            await adapter.ContinueConversationAsync(null, cr, ContinueCallback, default(CancellationToken));
            Assert.IsTrue(callbackInvoked);
        }

        [TestMethod]
        public async Task ContinueConversation_AppIdInformedAsync()
        {
            bool callbackInvoked = false;
            var adapter = new BotFrameworkAdapter(new SimpleCredentialProvider("AppId", "AppSecret"));
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
                ServiceUrl = "https://test.com",
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

            await adapter.ContinueConversationAsync("AppId", cr, ContinueCallback, default(CancellationToken));
            Assert.IsTrue(callbackInvoked);
        }

        [TestMethod]
        [ExpectedException(
            typeof(ArgumentNullException),
            "An AppId of null was inappropriately allowed.")]
        public async Task ContinueConversation_AppIdNullAsync()
        {
            bool callbackInvoked = false;
            var adapter = new BotFrameworkAdapter(new SimpleCredentialProvider());
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
                ServiceUrl = "https://test.com",
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

            await adapter.ContinueConversationAsync(null, cr, ContinueCallback, default(CancellationToken));
            Assert.IsTrue(callbackInvoked);
        }
    }
}
