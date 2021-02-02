// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
{
    public class BotFrameworkHttpAdapterBaseTests
    {
        [Fact]
        public async Task NoCallerIdShouldSetNullOAuthScope()
        {
            var mockCredentialProvider = new Mock<ICredentialProvider>();
            var bot = new TestBot();
            var adapter = new MockAdapter(mockCredentialProvider.Object);
            var originalActivity = CreateBasicActivity(); // Has no callerId, therefore OAuthScope in TurnState should be null.

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
            public MockAdapter(ICredentialProvider credentialProvider, MockLogger logger = null)
                : base(credentialProvider, null, logger)
            {
                Logger = logger;
            }

            public new MockLogger Logger { get; private set; }
        }

        private class TestBot : IBot
        {
            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                Assert.Null(turnContext.TurnState.Get<string>(BotAdapter.OAuthScopeKey));
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
