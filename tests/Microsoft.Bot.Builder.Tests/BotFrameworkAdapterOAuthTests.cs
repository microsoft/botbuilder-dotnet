// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class BotFrameworkAdapterOAuthTests
    {
        [TestMethod]
        public async Task BotFrameworkAdapterFindsOAuthCards()
        {
            var mockClaims = new Mock<ClaimsIdentity>();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockConnector = new MemoryConnectorClient();
            Activity eventActivity = null;
            var originalActivity = CreateBasicActivity();

            TaskCompletionSource<string> receivedEventActivity = new TaskCompletionSource<string>();

            var sut = new MockAdapter(mockCredentialProvider.Object);
            await sut.ProcessActivityAsync(
                mockClaims.Object,
                originalActivity,
                (context, token) =>
                {
                    switch (context.Activity.Type)
                    {
                        case ActivityTypes.Message:
                            context.TurnState.Remove(typeof(IConnectorClient).FullName);
                            context.TurnState.Add<IConnectorClient>(mockConnector);
                            context.SendActivityAsync(CreateOAuthCardActivity());
                            break;
                        case ActivityTypes.Event:
                            eventActivity = context.Activity;
                            receivedEventActivity.SetResult("done");
                            break;
                    }

                    return Task.CompletedTask;
                },
                CancellationToken.None);

            await receivedEventActivity.Task;

            // 1 activity sent from bot to user
            Assert.AreEqual(1, ((MemoryConversations)mockConnector.Conversations).SentActivities.Count);

            // bot received the event activity
            Assert.IsNotNull(eventActivity);
            Assert.AreEqual(originalActivity.Conversation.Id, eventActivity.Conversation.Id);
            Assert.AreEqual(originalActivity.From.Id, eventActivity.From.Id);
            Assert.AreEqual(originalActivity.From.Name, eventActivity.From.Name);
            Assert.AreEqual(ActivityTypes.Event, eventActivity.Type);
            Assert.AreEqual("tokens/response", eventActivity.Name);

            var tokenResponse = eventActivity.Value as TokenResponse;
            Assert.IsNotNull(tokenResponse);
            Assert.IsTrue(tokenResponse.Token.Length > 0);
            Assert.IsNotNull(tokenResponse.ConnectionName.Length > 0);
        }

        private Activity CreateBasicActivity()
        {
            return new Activity("test")
            {
                Id = "abc123",
                ChannelId = "directlinespeech",
                ServiceUrl = "urn:botframework:WebSockets",
                Conversation = new ConversationAccount { Id = "123" },
                From = new ChannelAccount()
                {
                    Id = "user",
                    Name = "user",
                },
                Recipient = new ChannelAccount()
                {
                    Id = "bot",
                    Name = "bot",
                },
                Type = ActivityTypes.Message,
                Text = "test",
            };
        }

        private Activity CreateOAuthCardActivity()
        {
            return new Activity("test")
            {
                ChannelId = "directlinespeech",
                ServiceUrl = "urn:botframework:WebSockets",
                Conversation = new ConversationAccount { Id = "123" },
                Attachments = new List<Attachment>()
                    {
                        new Attachment
                        {
                            ContentType = OAuthCard.ContentType,
                            Content = new OAuthCard
                            {
                                Text = "Sign In",
                                ConnectionName = "MyConnection",
                                Buttons = new[]
                                {
                                    new CardAction
                                    {
                                        Title = "Sign In",
                                        Text = "Sign In",
                                        Type = ActionTypes.Signin,
                                        Value = "http://noop.com",
                                    },
                                },
                            },
                        },
                    },
            };
        }

        private class MockAdapter : BotFrameworkAdapter
        {
            public MockAdapter(ICredentialProvider credentialProvider)
                : base(credentialProvider)
            {
            }

            public override Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, string connectionName, string magicCode, CancellationToken cancellationToken)
            {
                return Task.FromResult(new TokenResponse()
                {
                    Token = "12345",
                });
            }
        }
    }
}
