// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class TeamsSSOTokenExchangeMiddlewareTests
    {
        private const string ConnectionName = "ConnectionName";
        private const string FakeExchangeableItem = "Fake token";
        private const string ExchangeId = "exchange id";
        private const string TeamsUserId = "teams.user.id";
        private const string Token = "token";

        [Fact]
        public void ConstructorValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new TeamsSSOTokenExchangeMiddleware(null, ConnectionName));
            Assert.Throws<ArgumentNullException>(() => new TeamsSSOTokenExchangeMiddleware(new MemoryStorage(), null));
            Assert.Throws<ArgumentNullException>(() => new TeamsSSOTokenExchangeMiddleware(new MemoryStorage(), string.Empty));
        }

        [Fact]
        public async Task TokenExchanged_OnTurnFires()
        {
            // Arrange
            bool wasCalled = false;
            var adapter = new TeamsSSOAdapter(CreateConversationReference())
               .Use(new TeamsSSOTokenExchangeMiddleware(new MemoryStorage(), ConnectionName));
            
            adapter.AddExchangeableToken(ConnectionName, Channels.Msteams, TeamsUserId, FakeExchangeableItem, Token);

            // Act
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                // note the Middleware should not cause the Responded flag to be set
                Assert.False(context.Responded);
                wasCalled = true;
                await context.SendActivityAsync("processed", cancellationToken: cancellationToken);
                await Task.CompletedTask;
            })
                .Send("test")
                .AssertReply("processed")
                .StartTestAsync();

            // Assert
            Assert.True(wasCalled, "Delegate was not called");
        }

        [Fact]
        public async Task TokenExchanged_SecondSendsInvokeResponse()
        {
            // Arrange
            int calledCount = 0;
            var adapter = new TeamsSSOAdapter(CreateConversationReference())
               .Use(new TeamsSSOTokenExchangeMiddleware(new MemoryStorage(), ConnectionName));

            adapter.AddExchangeableToken(ConnectionName, Channels.Msteams, TeamsUserId, FakeExchangeableItem, Token);

            // Act
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                // note the Middleware should not cause the Responded flag to be set
                Assert.False(context.Responded);
                calledCount++;
                await context.SendActivityAsync("processed", cancellationToken: cancellationToken);
                await Task.CompletedTask;
            })
                .Send("test")
                .AssertReply("processed")
                .Send("test")
                .AssertReply((activity) =>
                {
                    // When the 2nd message goes through, it is not processed due to deduplication
                    // but an invokeResponse of 200 status with empty body is sent back
                    Assert.Equal(ActivityTypesEx.InvokeResponse, activity.Type);
                    var invokeResponse = (activity as Activity).Value as InvokeResponse;
                    Assert.Null(invokeResponse.Body);
                    Assert.Equal(200, invokeResponse.Status);
                })
                .StartTestAsync();

            // Assert
            Assert.False(calledCount == 0, "Delegate was not called");
            Assert.True(calledCount == 1, "OnTurn delegate called more than once");
        }

        [Fact]
        public async Task TokenNotExchanged_PreconditionFailed()
        {
            // Arrange
            bool wasCalled = false;
            var adapter = new TeamsSSOAdapter(CreateConversationReference())
               .Use(new TeamsSSOTokenExchangeMiddleware(new MemoryStorage(), ConnectionName));

            // since this test does not setup adapter.AddExchangeableToken, the exchange will not happen

            // Act
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                wasCalled = true;
                await Task.CompletedTask;
            })
                .Send("test")
                .AssertReply((activity) =>
                {
                    Assert.Equal(ActivityTypesEx.InvokeResponse, activity.Type);
                    var invokeResponse = (activity as Activity).Value as InvokeResponse;
                    var tokenExchangeRequest = invokeResponse.Body as TokenExchangeInvokeResponse;
                    Assert.Equal(ConnectionName, tokenExchangeRequest.ConnectionName);
                    Assert.Equal(ExchangeId, tokenExchangeRequest.Id);
                    Assert.Equal(412, invokeResponse.Status); //412:PreconditionFailed
                })
                .StartTestAsync();

            // Assert
            Assert.False(wasCalled, "Delegate was called");
        }

        [Fact]
        public async Task TokenNotExchanged_DirectLineChannel()
        {
            // Arrange
            bool wasCalled = false;
            var adapter = new TeamsSSOAdapter(CreateConversationReference(Channels.Directline))
               .Use(new TeamsSSOTokenExchangeMiddleware(new MemoryStorage(), ConnectionName));

            // Act
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                wasCalled = true;
                await context.SendActivityAsync("processed", cancellationToken: cancellationToken);
                await Task.CompletedTask;
            })
                .Send("test")
                .AssertReply("processed")
                .StartTestAsync();

            // Assert
            Assert.True(wasCalled, "Delegate was not called");
        }

        private ConversationReference CreateConversationReference(string channelId = Channels.Msteams)
        {
            return new ConversationReference
            {
                ChannelId = channelId,
                ServiceUrl = "https://test.com",
                User = new ChannelAccount(TeamsUserId, TeamsUserId),
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount(false, "convo1", "Conversation1"),
                Locale = "en-us",
            };
        }

        private class TeamsSSOAdapter : TestAdapter
        {
            public TeamsSSOAdapter(ConversationReference conversationReference)
                : base(conversationReference)
            {
            }

            public override Task SendTextToBotAsync(string userSays, BotCallbackHandler callback, CancellationToken cancellationToken)
            {
                return ProcessActivityAsync(MakeTokenExchangeActivity(), callback, cancellationToken);
            }

            public Activity MakeTokenExchangeActivity()
            {
                return new Activity
                {
                    Type = ActivityTypes.Invoke,
                    Locale = this.Locale ?? "en-us",
                    From = Conversation.User,
                    Recipient = Conversation.Bot,
                    Conversation = Conversation.Conversation,
                    ServiceUrl = Conversation.ServiceUrl,
                    Id = Guid.NewGuid().ToString(),
                    Name = SignInConstants.TokenExchangeOperationName,
                    Value = JObject.FromObject(new TokenExchangeInvokeRequest() 
                    { 
                        Token = FakeExchangeableItem, 
                        Id = ExchangeId, 
                        ConnectionName = ConnectionName
                    })
                };
            }
        }
    }
}
