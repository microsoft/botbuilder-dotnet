// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
{
    public class BotFrameworkHttpAdapterBaseTests
    {
        [Fact]
        public async Task NoCallerIdShouldSetNullOAuthScope()
        {
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockSocket = new Mock<WebSocket>();
            var bot = new TestBot(null);
            var adapter = new MockAdapter(mockCredentialProvider.Object, bot);

            var originalActivity = CreateBasicActivity(); // Has no callerId, therefore OAuthScope in TurnState should be null.
            adapter.CreateStreamingRequestHandler(mockSocket.Object, originalActivity);

            await adapter.ProcessStreamingActivityAsync(originalActivity, bot.OnTurnAsync);
        }

        [Fact]
        public async Task PublicCloudCallerIdShouldSetCorrectOAuthScope()
        {
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var mockSocket = new Mock<WebSocket>();
            var oAuthScope = AuthenticationConstants.ToBotFromChannelTokenIssuer;
            var bot = new TestBot(oAuthScope);
            var adapter = new MockAdapter(mockCredentialProvider.Object, bot);

            var originalActivity = CreateBasicActivity();
            originalActivity.CallerId = CallerIdConstants.PublicAzureChannel;
            adapter.CreateStreamingRequestHandler(mockSocket.Object, originalActivity, oAuthScope);

            await adapter.ProcessStreamingActivityAsync(originalActivity, bot.OnTurnAsync);
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

        private class MockAdapter : BotFrameworkHttpAdapterBase
        {
            public MockAdapter(ICredentialProvider credentialProvider, IBot bot, MockLogger logger = null)
                : base(credentialProvider, null, logger)
            {
                Logger = logger;
                ConnectedBot = bot;
            }

            public new MockLogger Logger { get; private set; }

            public void CreateStreamingRequestHandler(WebSocket socket, Activity activity, string audience = null)
            {
                var srh = new StreamingRequestHandler(ConnectedBot, this, socket, audience, Logger);

                // Prepare StreamingRequestHandler for BotFrameworkHttpAdapterBase.ProcessStreamingActivityAsync()
                // Add ConversationId to StreamingRequestHandler's conversations cache
                var cacheField = typeof(StreamingRequestHandler).GetField("_conversations", BindingFlags.NonPublic | BindingFlags.Instance);
                var cache = (ConcurrentDictionary<string, DateTime>)cacheField.GetValue(srh);
                cache.TryAdd(activity.Conversation.Id, DateTime.Now);

                // Add ServiceUrl to StreamingRequestHandler
                var serviceUrlProp = typeof(StreamingRequestHandler).GetProperty("ServiceUrl");
                serviceUrlProp.DeclaringType.GetProperty("ServiceUrl");
                serviceUrlProp.GetSetMethod(true).Invoke(srh, new object[] { activity.ServiceUrl });

                if (RequestHandlers != null)
                {
                    RequestHandlers.Add(srh);
                    return;
                }
                
                RequestHandlers = new List<StreamingRequestHandler> { srh };
            }
        }

        private class TestBot : IBot
        {
            private string _expectedOAuthScope;
            
            public TestBot(string expectedOAuthScope)
            {
                _expectedOAuthScope = expectedOAuthScope;
            }

            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                Assert.Equal(turnContext.TurnState.Get<string>(BotAdapter.OAuthScopeKey), _expectedOAuthScope);
                return Task.CompletedTask;
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
