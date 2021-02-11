// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class BotFrameworkAdapterOAuthTests
    {
        [Fact]
        public async Task BotFrameworkAdapterFindsOAuthCards()
        {
            var mockConnector = new MemoryConnectorClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var adapter = new MockAdapter(mockCredentialProvider.Object, () =>
            {
                return new TokenResponse() { Token = "12345" };
            });
            var originalActivity = CreateBasicActivity();

            var eventActivity = await ProcessOAuthCardTest(adapter, mockConnector, originalActivity);
            
            // bot received the event activity
            Assert.NotNull(eventActivity);
            Assert.Equal(originalActivity.Conversation.Id, eventActivity.Conversation.Id);
            Assert.Equal(originalActivity.From.Id, eventActivity.From.Id);
            Assert.Equal(originalActivity.From.Name, eventActivity.From.Name);
            Assert.Equal(ActivityTypes.Event, eventActivity.Type);
            Assert.Equal(SignInConstants.TokenResponseEventName, eventActivity.Name);

            var tokenResponse = eventActivity.Value as TokenResponse;
            Assert.Equal("12345", tokenResponse.Token);
        }

        [Fact]
        public async Task SendsTokenOnSecondAttempt()
        {
            var mockConnector = new MemoryConnectorClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            int callCount = 0;
            var adapter = new MockAdapter(mockCredentialProvider.Object, () =>
            {
                callCount++;
                if (callCount > 1)
                {
                    return new TokenResponse() { Token = "1234" };
                }

                return null;
            });
            var originalActivity = CreateBasicActivity();
            var eventActivity = await ProcessOAuthCardTest(adapter, mockConnector, originalActivity);
            
            // bot received the event activity
            Assert.NotNull(eventActivity);
            Assert.Equal(originalActivity.Conversation.Id, eventActivity.Conversation.Id);
            Assert.Equal(originalActivity.From.Id, eventActivity.From.Id);
            Assert.Equal(originalActivity.From.Name, eventActivity.From.Name);
            Assert.Equal(ActivityTypes.Event, eventActivity.Type);
            Assert.Equal(SignInConstants.TokenResponseEventName, eventActivity.Name);

            var tokenResponse = eventActivity.Value as TokenResponse;
            Assert.Equal("1234", tokenResponse.Token);
        }

        [Fact]
        public async Task PollingEnds()
        {
            var mockConnector = new MemoryConnectorClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            int calls = 0;
            var adapter = new MockAdapter(
                mockCredentialProvider.Object,
                () =>
                {
                    calls++;
                    return new TokenResponse() { Token = "12345" };
                });
            var originalActivity = CreateBasicActivity();

            var eventActivity = await ProcessOAuthCardTest(adapter, mockConnector, originalActivity);

            // Only 1 call to GetToken is called
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task TokenResponsePropertiesEndPolling()
        {
            var mockConnector = new MemoryConnectorClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            int callCount = 0;
            var adapter = new MockAdapter(
                mockCredentialProvider.Object,
                () =>
                {
                    callCount++;
                    return new TokenResponse()
                    {
                        Properties = MakeProperties(0, null),
                    };
                },
                new MockLogger());

            var originalActivity = CreateBasicActivity();

            var eventActivity = await ProcessOAuthCardTest(adapter, mockConnector, originalActivity, null, false);

            // Wait a bit to let the polling Task run (enough for 3 potential polls)
            await Task.Delay(3000);

            // Make sure it only polled once and it ended
            Assert.Equal(1, callCount);
            Assert.Contains("PollForTokenAsync completed without receiving a token", adapter.Logger.LogData);
        }

        [Fact]
        public async Task TokenResponsePropertiesCanChange()
        {
            var mockConnector = new MemoryConnectorClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            int callCount = 0;
            var adapter = new MockAdapter(
                mockCredentialProvider.Object,
                () =>
                {
                    callCount++;
                    if (callCount < 2)
                    {
                        return new TokenResponse()
                        {
                            Properties = MakeProperties(50000, 500),
                        };
                    }
                    else
                    {
                        return new TokenResponse()
                        {
                            Token = "123",
                        };
                    }
                },
                new MockLogger());

            var originalActivity = CreateBasicActivity();

            var eventActivity = await ProcessOAuthCardTest(adapter, mockConnector, originalActivity);

            // Wait a bit to let the polling Task run (enough for 3 potential polls)
            await Task.Delay(2000);

            // Make sure it only polled twice and it changed settings
            Assert.Equal(2, callCount);
            Assert.Contains("PollForTokenAsync received new polling settings: timeout=50000, interval=500", adapter.Logger.LogData);
        }

        [Fact]
        public async Task NoConnectionNameThrows()
        {
            var mockConnector = new MemoryConnectorClient();
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var adapter = new MockAdapter(
                mockCredentialProvider.Object,
                () =>
                {
                    return new TokenResponse() { Token = "12345" };
                },
                new MockLogger());
            var originalActivity = CreateBasicActivity();
            var badOauth = CreateOAuthCardActivity();
            ((OAuthCard)badOauth.Attachments.First().Content).ConnectionName = null;

            var mockClaims = new Mock<ClaimsIdentity>();

            bool threw = false;

            await adapter.ProcessActivityAsync(
                mockClaims.Object,
                originalActivity,
                async (context, token) =>
                {
                    switch (context.Activity.Type)
                    {
                        case ActivityTypes.Message:
                            context.TurnState.Remove(typeof(IConnectorClient).FullName);
                            context.TurnState.Add<IConnectorClient>(mockConnector);

                            try
                            {
                                await context.SendActivityAsync(badOauth);
                            }
                            catch (InvalidOperationException)
                            {
                                threw = true;
                            }

                            break;
                    }
                },
                CancellationToken.None);

            Assert.True(threw);
        }

        private async Task<Activity> ProcessOAuthCardTest(MockAdapter adapter, MemoryConnectorClient mockConnector, Activity originalActivity, Activity outhCardActivity = null, bool expectsEvent = true)
        {
            var mockClaims = new Mock<ClaimsIdentity>();
            Activity eventActivity = null;

            outhCardActivity = outhCardActivity ?? CreateOAuthCardActivity();

            TaskCompletionSource<string> receivedEventActivity = new TaskCompletionSource<string>();

            await adapter.ProcessActivityAsync(
                mockClaims.Object,
                originalActivity,
                async (context, token) =>
                {
                    switch (context.Activity.Type)
                    {
                        case ActivityTypes.Message:
                            context.TurnState.Remove(typeof(IConnectorClient).FullName);
                            context.TurnState.Add<IConnectorClient>(mockConnector);
                            await context.SendActivityAsync(outhCardActivity);
                            break;
                        case ActivityTypes.Event:
                            eventActivity = context.Activity;
                            receivedEventActivity.SetResult("done");
                            break;
                    }

                    if (!expectsEvent)
                    {
                        receivedEventActivity.SetResult("done");
                    }
                },
                CancellationToken.None);

            await receivedEventActivity.Task;

            return eventActivity;
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

        private JObject MakeProperties(int? timeout, int? interval)
        {
            var settings = new TokenPollingSettings();
            if (timeout.HasValue)
            {
                settings.Timeout = timeout.Value;
            }

            if (interval.HasValue)
            {
                settings.Interval = interval.Value;
            }

            var properties = new JObject();
            properties[TurnStateConstants.TokenPollingSettingsKey] = JObject.FromObject(settings);

            return properties;
        }

        private class MockAdapter : BotFrameworkHttpAdapterBase
        {
            private Func<TokenResponse> _getUserTokenAction;

            public MockAdapter(ICredentialProvider credentialProvider, Func<TokenResponse> getUserTokenAction, MockLogger logger = null)
                : base(credentialProvider, null, logger)
            {
                _getUserTokenAction = getUserTokenAction;
                Logger = logger;
            }

            public new MockLogger Logger { get; private set; }

            public override Task<TokenResponse> GetUserTokenAsync(ITurnContext turnContext, AppCredentials appCredentials, string connectionName, string magicCode, CancellationToken cancellationToken)
            {
                return Task.FromResult(_getUserTokenAction());
            }
        }

        private class MockLogger : ILogger<BotFrameworkHttpAdapterBase>
        {
            public List<string> LogData { get; set; } = new List<string>();

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LogData.Add(formatter(state, exception));
            }
        }
    }
}
