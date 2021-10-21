// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Tests.Tools;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Tests.Integration
{
    public class EndToEndMiniLoadTests
    {
        private readonly ITestOutputHelper _testOutput;

        public EndToEndMiniLoadTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Theory]
        [InlineData(false, false)] // new client, new server
        [InlineData(true, true)] // legacy client, legacy server
        [InlineData(true, false)] // legacy client, new server
        [InlineData(false, true)] // new client, legacy server
        public void SimpleActivityTest(bool useLegacyClient, bool useLegacyServer)
        {
            // Arrange
            var verifiedResponse = false;

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = "test",
                Text = "hi"
            };

            var clientRequestHandler = new Mock<RequestHandler>();
            clientRequestHandler
                .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns<ReceiveRequest, ILogger<RequestHandler>, object, CancellationToken>((request, logger, context, cancellationToken) =>
                {
                    var body = request.ReadBodyAsString();
                    var response = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                    Assert.NotNull(response);
                    Assert.Equal("Echo: hi", response.Text);

                    verifiedResponse = true;

                    return Task.FromResult(StreamingResponse.OK());
                });

            // Act
            RunActivityStreamingTest(activity, new StreamingTestBot(), clientRequestHandler.Object, useLegacyClient, useLegacyServer);

            // Assert
            Assert.True(verifiedResponse);
        }

        private static HttpRequest CreateWebSocketUpgradeRequest(TestWebSocketConnectionFeature connection)
        {
            var webSocketManager = new Mock<WebSocketManager>();
            webSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
            webSocketManager.Setup(m => m.AcceptWebSocketAsync()).Returns(connection.AcceptAsync());

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.WebSockets).Returns(webSocketManager.Object);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(r => r.Method).Returns("GET");
            httpRequest.Setup(r => r.HttpContext).Returns(httpContext.Object);
            httpRequest.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                { "Authorization", new StringValues("Bearer token") },
                { "channelid", new StringValues("test") }
            });

            return httpRequest.Object;
        }

        private void RunActivityStreamingTest(IActivity activity, IBot bot, RequestHandler requestHandler, bool useLegacyClient, bool useLegacyServer)
        {
            var logger = XUnitLogger.CreateLogger(_testOutput);

            using (var connection = new TestWebSocketConnectionFeature())
            {
                IBotFrameworkHttpAdapter server = useLegacyServer
                    ? new BotFrameworkHttpAdapter()
                    : new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);
                var serverRunning = server.ProcessAsync(CreateWebSocketUpgradeRequest(connection), new Mock<HttpResponse>().Object, bot, CancellationToken.None);

                using (var client = new TestStreamingTransportClient("wss://test", requestHandler, connection.Client, logger, useLegacyClient))
                {
                    var clientRunning = client.ConnectAsync();

                    var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");
                    var response = client.SendAsync(StreamingRequest.CreatePost("/api/messages", content)).Result;
                    Assert.Equal(200, response.StatusCode);

                    connection.Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of test", CancellationToken.None).Wait();

                    clientRunning.Wait();
                    Assert.Equal(TaskStatus.RanToCompletion, clientRunning.Status);
                }

                serverRunning.Wait();
                Assert.Equal(TaskStatus.RanToCompletion, serverRunning.Status);
            }
        }

        private class StreamingTestBot : ActivityHandler
        {
            protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
            {
                var activityClone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(turnContext.Activity));
                activityClone.Text = $"Echo: {turnContext.Activity.Text}";

                return turnContext.SendActivityAsync(activityClone, cancellationToken);
            }
        }

        private class StreamingTestBotFrameworkAuthentication : BotFrameworkAuthentication
        {
            public override Task<AuthenticateRequestResult> AuthenticateRequestAsync(Activity activity, string authHeader, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task<AuthenticateRequestResult> AuthenticateStreamingRequestAsync(string authHeader, string channelIdHeader, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AuthenticateRequestResult
                {
                    Audience = "aud",
                    CallerId = "caller",
                    ClaimsIdentity = new ClaimsIdentity(new List<Claim>
                    {
                        new Claim("aud", "aud"),
                        new Claim("iss", "https://login.microsoftonline.com/tid"),
                        new Claim("azp", "appId"),
                        new Claim("tid", "tid"),
                        new Claim("ver", "2.0")
                    })
                });
            }

            public override ConnectorFactory CreateConnectorFactory(ClaimsIdentity claimsIdentity)
            {
                throw new NotImplementedException();
            }

            public override Task<UserTokenClient> CreateUserTokenClientAsync(ClaimsIdentity claimsIdentity, CancellationToken cancellationToken)
            {
                return Task.FromResult<UserTokenClient>(new TestUserTokenClient());
            }
        }

        private class TestStreamingTransportClient : IStreamingTransportClient
        {
            private readonly WebSocket _client;
            private readonly bool _useLegacyClient;

            private readonly WebSocketClient _inner;
            private readonly Bot.Streaming.Transport.WebSockets.WebSocketClient _innerLegacy;

            public TestStreamingTransportClient(string url, RequestHandler requestHandler, WebSocket client, ILogger logger, bool useLegacyClient)
            {
                _client = client ?? throw new ArgumentNullException(nameof(client));
                _useLegacyClient = useLegacyClient;

                if (useLegacyClient)
                {
                    _innerLegacy = new Bot.Streaming.Transport.WebSockets.WebSocketClient(url, requestHandler);
                }
                else
                {
                    _inner = new WebSocketClient(url, requestHandler, logger: logger);
                }
            }

            public event DisconnectedEventHandler Disconnected;

            public bool IsConnected => _useLegacyClient ? _innerLegacy.IsConnected : _inner.IsConnected;

            public Task ConnectAsync()
            {
                return _useLegacyClient
                    ? _innerLegacy.ConnectInternalAsync(_client)
                    : _inner.ConnectInternalAsync(_client, CancellationToken.None);
            }

            public Task ConnectAsync(IDictionary<string, string> requestHeaders)
            {
                throw new NotImplementedException();
            }

            public Task<ReceiveResponse> SendAsync(StreamingRequest message, CancellationToken cancellationToken = default(CancellationToken))
            {
                return _useLegacyClient
                    ? _innerLegacy.SendAsync(message, cancellationToken)
                    : _inner.SendAsync(message, cancellationToken);
            }

            public void Disconnect()
            {
                if (_useLegacyClient)
                {
                    _innerLegacy.Disconnect();
                }
                else
                {
                    _inner.Disconnect();
                }
            }

            public void Dispose()
            {
                if (_useLegacyClient)
                {
                    _innerLegacy.Dispose();
                }
                else
                {
                    _inner.Dispose();
                }

                if (Disconnected != null)
                {
                    Disconnected(_useLegacyClient ? _innerLegacy : _inner, DisconnectedEventArgs.Empty);
                }
            }
        }

        private class TestUserTokenClient : UserTokenClient
        {
            public override Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken)
            {
                return Task.FromResult(new TokenResponse());
            }

            public override Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Dictionary<string, TokenResponse>());
            }

            public override Task<SignInResource> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken)
            {
                return Task.FromResult(new SignInResource());
            }

            public override Task<TokenStatus[]> GetTokenStatusAsync(string userId, string channelId, string includeFilter, CancellationToken cancellationToken)
            {
                return Task.FromResult(Array.Empty<TokenStatus>());
            }

            public override Task<TokenResponse> GetUserTokenAsync(string userId, string connectionName, string channelId, string magicCode, CancellationToken cancellationToken)
            {
                return Task.FromResult(new TokenResponse());
            }

            public override Task SignOutUserAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
