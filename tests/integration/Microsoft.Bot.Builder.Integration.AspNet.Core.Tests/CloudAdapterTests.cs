// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class CloudAdapterTests
    {
        [Fact]
        public async Task BasicMessageActivity()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var adapter = new CloudAdapter();
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
        }

        [Fact]
        public async Task InvokeActivity()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateInvokeActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var response = new MemoryStream();
            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.Setup(r => r.Body).Returns(response);

            var bot = new InvokeResponseBot();

            // Act
            var adapter = new CloudAdapter();
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            using (var stream = new MemoryStream(response.GetBuffer()))
            using (var reader = new StreamReader(stream))
            {
                var s = reader.ReadToEnd();
                var json = JObject.Parse(s);
                Assert.Equal("im.feeling.really.attacked.right.now", json["quite.honestly"]);
            }
        }

        [Theory]
        [InlineData("DELETE")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("PATCH")]
        public async Task MethodNotAllowed(string httpMethod)
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var webSocketManagerMock = new Mock<WebSocketManager>();
            webSocketManagerMock.Setup(w => w.IsWebSocketRequest).Returns(false);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.WebSockets).Returns(webSocketManagerMock.Object);
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(httpMethod);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);
            httpRequestMock.Setup(r => r.HttpContext).Returns(httpContextMock.Object);

            var httpResponseMock = new Mock<HttpResponse>().SetupAllProperties();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(CreateInternalHttpResponse()));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var bot = new MessageBot();

            // Act
            var cloudEnvironment = BotFrameworkAuthenticationFactory.Create(null, false, null, null, null, null, null, null, null, new PasswordServiceClientCredentialFactory(), new AuthenticationConfiguration(), httpClientFactoryMock.Object, null);
            var adapter = new CloudAdapter(cloudEnvironment);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            Assert.Equal((int)HttpStatusCode.MethodNotAllowed, httpResponseMock.Object.StatusCode);
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task WebSocketRequestShouldCallAuthenticateStreamingRequestAsync()
        {
            // Note this test only checks that a GET request will result in an auth call and a socket accept
            // it doesn't valid that activities over that socket get to the bot or back

            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var webSocketReceiveResult = new Mock<WebSocketReceiveResult>(MockBehavior.Strict, new object[] { 1, WebSocketMessageType.Binary, false });

            var webSocketMock = new Mock<WebSocket>();
            webSocketMock.Setup(ws => ws.State).Returns(WebSocketState.Open);
            webSocketMock.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(webSocketReceiveResult.Object));
            webSocketMock.Setup(ws => ws.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            webSocketMock.Setup(ws => ws.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            webSocketMock.Setup(ws => ws.Dispose());

            var webSocketManagerMock = new Mock<WebSocketManager>();
            webSocketManagerMock.Setup(w => w.IsWebSocketRequest).Returns(true);
            webSocketManagerMock.Setup(w => w.AcceptWebSocketAsync()).Returns(Task.FromResult(webSocketMock.Object));
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.WebSockets).Returns(webSocketManagerMock.Object);
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns("GET");
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);
            httpRequestMock.Setup(r => r.HttpContext).Returns(httpContextMock.Object);

            var httpResponseMock = new Mock<HttpResponse>().SetupAllProperties();

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                Audience = "audience",
            };

            var botFrameworkAuthenticationMock = new Mock<BotFrameworkAuthentication>();
            botFrameworkAuthenticationMock.Setup(
                x => x.AuthenticateStreamingRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(authenticateRequestResult);

            var bot = new MessageBot();

            // Act
            var adapter = new CloudAdapter(botFrameworkAuthenticationMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            botFrameworkAuthenticationMock.Verify(x => x.AuthenticateStreamingRequestAsync(It.Is<string>(v => true), It.Is<string>(v => true), It.Is<CancellationToken>(ct => true)), Times.Once());
        }

        [Fact]
        public void CanContinueConversationOverWebSocket()
        {
            // Arrange
            var continueConversationWaiter = new AutoResetEvent(false);
            var verifiedValidContinuation = false;

            var appId = "testAppId";
            var tenantId = "testTenantId";
            var token = "Bearer testjwt";
            var channelId = "testChannel";
            var audience = "testAudience";
            var callerId = "testCallerId";

            var authResult = new AuthenticateRequestResult
            {
                Audience = audience,
                CallerId = callerId,
                ClaimsIdentity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("aud", audience),
                    new Claim("iss", $"https://login.microsoftonline.com/{tenantId}/"),
                    new Claim("azp", appId),
                    new Claim("tid", tenantId),
                    new Claim("ver", "2.0")
                })
            };

            var userTokenClient = new TestUserTokenClient(appId);

            var nullUrlActivity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = null,
                ChannelId = channelId,
                Text = "hi",
            };
            var nullUrlContent = new StringContent(JsonConvert.SerializeObject(nullUrlActivity), Encoding.UTF8, "application/json");

            var validActivity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = channelId,
                Text = "hi",
            };
            var validContent = new StringContent(JsonConvert.SerializeObject(validActivity), Encoding.UTF8, "application/json");

            var invalidActivity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = channelId,
                Text = "hi",
            };

            var nullUrlConnection = new Mock<StreamingConnection>(null);
            nullUrlConnection
                .Setup(c => c.ListenAsync(It.IsAny<RequestHandler>(), It.IsAny<CancellationToken>()))
                .Returns<RequestHandler, CancellationToken>((handler, cancellationToken) => handler.ProcessRequestAsync(
                    new ReceiveRequest
                    {
                        Verb = "POST",
                        Path = "/api/messages",
                        Streams = new List<IContentStream>
                        {
                            new TestContentStream
                            {
                                Id = Guid.NewGuid(),
                                ContentType = "application/json",
                                Length = (int?)nullUrlContent.Headers.ContentLength,
                                Stream = nullUrlContent.ReadAsStreamAsync().GetAwaiter().GetResult()
                            }
                        }
                    },
                    null,
                    cancellationToken: cancellationToken));

            var streamingConnection = new Mock<StreamingConnection>(null);
            streamingConnection
                .Setup(c => c.ListenAsync(It.IsAny<RequestHandler>(), It.IsAny<CancellationToken>()))
                .Returns<RequestHandler, CancellationToken>((handler, cancellationToken) => handler.ProcessRequestAsync(
                    new ReceiveRequest
                    {
                        Verb = "POST",
                        Path = "/api/messages",
                        Streams = new List<IContentStream>
                        {
                            new TestContentStream
                            {
                                Id = Guid.NewGuid(),
                                ContentType = "application/json",
                                Length = (int?)validContent.Headers.ContentLength,
                                Stream = validContent.ReadAsStreamAsync().GetAwaiter().GetResult()
                            }
                        }
                    },
                    null,
                    cancellationToken: cancellationToken));

            var auth = new Mock<BotFrameworkAuthentication>();
            auth.Setup(a => a.AuthenticateStreamingRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(authResult));
            auth.Setup(a => a.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var webSocket = new ClientWebSocket();
            var webSocketManager = new Mock<WebSocketManager>();
            webSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
            webSocketManager.Setup(m => m.AcceptWebSocketAsync()).Returns(Task.FromResult<WebSocket>(webSocket));
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.WebSockets).Returns(webSocketManager.Object);
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(r => r.Method).Returns("GET");
            httpRequest.Setup(r => r.HttpContext).Returns(httpContext.Object);
            httpRequest.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                { "Authorization", new StringValues(token) },
                { "channelid", new StringValues(channelId) }
            });

            var httpResponse = new Mock<HttpResponse>();

            var nullUrlWebSocket = new ClientWebSocket();
            var nullUrlWebSocketManager = new Mock<WebSocketManager>();
            nullUrlWebSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
            nullUrlWebSocketManager.Setup(m => m.AcceptWebSocketAsync()).Returns(Task.FromResult<WebSocket>(nullUrlWebSocket));
            var nullUrlHttpContext = new Mock<HttpContext>();
            nullUrlHttpContext.Setup(c => c.WebSockets).Returns(nullUrlWebSocketManager.Object);
            var nullUrlHttpRequest = new Mock<HttpRequest>();
            nullUrlHttpRequest.Setup(r => r.Method).Returns("GET");
            nullUrlHttpRequest.Setup(r => r.HttpContext).Returns(httpContext.Object);
            nullUrlHttpRequest.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                { "Authorization", new StringValues(token) },
                { "channelid", new StringValues(channelId) }
            });

            var nullUrlHttpResponse = new Mock<HttpResponse>();

            var bot = new Mock<IBot>();
            bot.Setup(b => b.OnTurnAsync(It.IsAny<ITurnContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => { continueConversationWaiter.WaitOne(); })); // Simulate listening on web socket

            // Act
            var adapter = new StreamingTestCloudAdapter(auth.Object, new Dictionary<WebSocket, StreamingConnection> { { nullUrlWebSocket, nullUrlConnection.Object }, { webSocket, streamingConnection.Object } });
            var nullUrlProcessRequest = adapter.ProcessAsync(nullUrlHttpRequest.Object, nullUrlHttpResponse.Object, bot.Object, CancellationToken.None);
            var processRequest = adapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, CancellationToken.None);

            var validContinuation = adapter.ContinueConversationAsync(
                authResult.ClaimsIdentity,
                validActivity,
                (turn, cancellationToken) =>
                {
                    var connectorFactory = turn.TurnState.Get<ConnectorFactory>();
                    Assert.NotNull(connectorFactory);
                    var connectorFactoryTypeName = connectorFactory.GetType().FullName ?? string.Empty;
                    Assert.EndsWith("StreamingConnectorFactory", connectorFactoryTypeName);
                    verifiedValidContinuation = true;

                    return Task.CompletedTask;
                },
                CancellationToken.None);

            var invalidContinuation = adapter.ContinueConversationAsync(
                authResult.ClaimsIdentity, invalidActivity, (turn, cancellationToken) => Task.CompletedTask, CancellationToken.None);

            continueConversationWaiter.Set();
            nullUrlProcessRequest.Wait();
            processRequest.Wait();

            // Assert
            Assert.True(processRequest.IsCompletedSuccessfully);
            Assert.True(verifiedValidContinuation);
            Assert.True(validContinuation.IsCompletedSuccessfully);
            Assert.Null(validContinuation.Exception);
            Assert.True(invalidContinuation.IsFaulted);
            Assert.NotEmpty(invalidContinuation.Exception.InnerExceptions);
            Assert.True(invalidContinuation.Exception.InnerExceptions[0] is ApplicationException);
        }

        [Fact]
        public async Task MessageActivityWithHttpClient()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(CreateInternalHttpResponse()));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var bot = new MessageBot();

            // Act
            var cloudEnvironment = BotFrameworkAuthenticationFactory.Create(null, false, null, null, null, null, null, null, null, new PasswordServiceClientCredentialFactory(), new AuthenticationConfiguration(), httpClientFactoryMock.Object, null);
            var adapter = new CloudAdapter(cloudEnvironment);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void ConstructorWithConfiguration()
        {
            // Arrange
            var appSettings = new Dictionary<string, string>
            {
                { "MicrosoftAppId", "appId" },
                { "MicrosoftAppPassword", "appPassword" },
                { "ChannelService", GovernmentAuthenticationConstants.ChannelService }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();

            // Act
            _ = new CloudAdapter(configuration);

            // Assert

            // TODO: work out what might be a reasonable side effect
        }

        [Fact]
        public async Task BadRequest()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateBadRequestStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.SetupProperty(x => x.StatusCode);

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var adapter = new CloudAdapter();
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Never());
            Assert.Equal((int)HttpStatusCode.BadRequest, httpResponseMock.Object.StatusCode);
        }

        [Fact]
        public async Task InjectCloudEnvironment()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = new ClaimsIdentity(),
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient("appId");

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var httpClient = new Mock<HttpClient>();

            // Act
            var adapter = new CloudAdapter(cloudEnvironmentMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
            cloudEnvironmentMock.Verify(ce => ce.AuthenticateRequestAsync(It.Is<Activity>(tc => true), It.Is<string>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
        }

        [Fact]
        public async Task CloudAdapterProvidesUserTokenClient()
        {
            // this is just a basic test to verify the wire-up of a UserTokenClient in the CloudAdapter
            // there is also some coverage for the internal code that creates the TokenExchangeState string

            // Arrange
            string appId = "appId";
            string userId = "userId";
            string channelId = "channelId";
            string conversationId = "conversationId";
            string recipientId = "botId";
            string relatesToActivityId = "relatesToActivityId";
            string connectionName = "connectionName";

            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream(userId, channelId, conversationId, recipientId, relatesToActivityId));
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = new ClaimsIdentity(),
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient(appId);

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var bot = new UserTokenClientBot(connectionName);

            // Act
            var adapter = new CloudAdapter(cloudEnvironmentMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            var args_ExchangeTokenAsync = userTokenClient.Record["ExchangeTokenAsync"];
            Assert.Equal(userId, (string)args_ExchangeTokenAsync[0]);
            Assert.Equal(connectionName, (string)args_ExchangeTokenAsync[1]);
            Assert.Equal(channelId, (string)args_ExchangeTokenAsync[2]);
            Assert.Equal("TokenExchangeRequest", args_ExchangeTokenAsync[3].GetType().Name);

            var args_GetAadTokensAsync = userTokenClient.Record["GetAadTokensAsync"];
            Assert.Equal(userId, (string)args_GetAadTokensAsync[0]);
            Assert.Equal(connectionName, (string)args_GetAadTokensAsync[1]);
            Assert.Equal("x", ((string[])args_GetAadTokensAsync[2])[0]);
            Assert.Equal("y", ((string[])args_GetAadTokensAsync[2])[1]);

            Assert.Equal(channelId, (string)args_GetAadTokensAsync[3]);

            var args_GetSignInResourceAsync = userTokenClient.Record["GetSignInResourceAsync"];

            // this code is testing the internal CreateTokenExchangeState function by doing the work in reverse
            var state = (string)args_GetSignInResourceAsync[0];
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(state));
            var tokenExchangeState = JsonConvert.DeserializeObject<TokenExchangeState>(json);
            Assert.Equal(connectionName, tokenExchangeState.ConnectionName);
            Assert.Equal(appId, tokenExchangeState.MsAppId);
            Assert.Equal(conversationId, tokenExchangeState.Conversation.Conversation.Id);
            Assert.Equal(recipientId, tokenExchangeState.Conversation.Bot.Id);
            Assert.Equal(relatesToActivityId, tokenExchangeState.RelatesTo.ActivityId);

            Assert.Equal("finalRedirect", (string)args_GetSignInResourceAsync[1]);

            var args_GetTokenStatusAsync = userTokenClient.Record["GetTokenStatusAsync"];
            Assert.Equal(userId, (string)args_GetTokenStatusAsync[0]);
            Assert.Equal(channelId, (string)args_GetTokenStatusAsync[1]);
            Assert.Equal("includeFilter", (string)args_GetTokenStatusAsync[2]);

            var args_GetUserTokenAsync = userTokenClient.Record["GetUserTokenAsync"];
            Assert.Equal(userId, (string)args_GetUserTokenAsync[0]);
            Assert.Equal(connectionName, (string)args_GetUserTokenAsync[1]);
            Assert.Equal(channelId, (string)args_GetUserTokenAsync[2]);
            Assert.Equal("magicCode", (string)args_GetUserTokenAsync[3]);

            var args_SignOutUserAsync = userTokenClient.Record["SignOutUserAsync"];
            Assert.Equal(userId, (string)args_SignOutUserAsync[0]);
            Assert.Equal(connectionName, (string)args_SignOutUserAsync[1]);
            Assert.Equal(channelId, (string)args_SignOutUserAsync[2]);
        }

        [Fact]
        public async Task CloudAdapterConnectorFactory()
        {
            // this is just a basic test to verify the wire-up of a ConnectorFactory in the CloudAdapter

            // Arrange

            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var claimsIdentity = new ClaimsIdentity();

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = claimsIdentity,
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient("appId");

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateConnectorFactory(It.IsAny<ClaimsIdentity>())).Returns(new TestConnectorFactory());
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var bot = new ConnectorFactoryBot();

            // Act
            var adapter = new CloudAdapter(cloudEnvironmentMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            Assert.Equal("audience", bot.Authorization.Parameter);
            Assert.Equal(claimsIdentity, bot.Identity);
            Assert.Equal(userTokenClient, bot.UserTokenClient);
            Assert.True(bot.ConnectorClient != null);
            Assert.True(bot.BotCallbackHandler != null);
        }

        [Fact]
        public async Task CloudAdapterContinueConversation()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity();

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = claimsIdentity,
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient("appId");

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateConnectorFactory(It.IsAny<ClaimsIdentity>())).Returns(new TestConnectorFactory());
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var bot = new ConnectorFactoryBot();

            var expectedServiceUrl = "http://serviceUrl";

            var conversationAccount = new ConversationAccount { Id = "conversation Id" };
            var continuationActivity = new Activity { Type = ActivityTypes.Event, ServiceUrl = expectedServiceUrl, Conversation = conversationAccount };
            var conversationReference = new ConversationReference { ServiceUrl = expectedServiceUrl, Conversation = conversationAccount };

            var actualServiceUrl1 = string.Empty;
            var actualServiceUrl2 = string.Empty;
            var actualServiceUrl3 = string.Empty;
            var actualServiceUrl4 = string.Empty;
            var actualServiceUrl5 = string.Empty;
            var actualServiceUrl6 = string.Empty;

            BotCallbackHandler callback1 = (t, c) =>
            {
                actualServiceUrl1 = t.Activity.ServiceUrl;
                return Task.CompletedTask;
            };
            BotCallbackHandler callback2 = (t, c) =>
            {
                actualServiceUrl2 = t.Activity.ServiceUrl;
                return Task.CompletedTask;
            };
            BotCallbackHandler callback3 = (t, c) =>
            {
                actualServiceUrl3 = t.Activity.ServiceUrl;
                return Task.CompletedTask;
            };
            BotCallbackHandler callback4 = (t, c) =>
            {
                actualServiceUrl4 = t.Activity.ServiceUrl;
                return Task.CompletedTask;
            };
            BotCallbackHandler callback5 = (t, c) =>
            {
                actualServiceUrl5 = t.Activity.ServiceUrl;
                return Task.CompletedTask;
            };
            BotCallbackHandler callback6 = (t, c) =>
            {
                actualServiceUrl6 = t.Activity.ServiceUrl;
                return Task.CompletedTask;
            };

            // Act
            var adapter = new CloudAdapter(cloudEnvironmentMock.Object);
            await adapter.ContinueConversationAsync("botAppId", continuationActivity, callback1, CancellationToken.None);
            await adapter.ContinueConversationAsync(claimsIdentity, continuationActivity, callback2, CancellationToken.None);
            await adapter.ContinueConversationAsync(claimsIdentity, continuationActivity, "audience", callback3, CancellationToken.None);
            await adapter.ContinueConversationAsync("botAppId", conversationReference, callback4, CancellationToken.None);
            await adapter.ContinueConversationAsync(claimsIdentity, conversationReference, callback5, CancellationToken.None);
            await adapter.ContinueConversationAsync(claimsIdentity, conversationReference, "audience", callback6, CancellationToken.None);

            // Assert
            Assert.Equal(expectedServiceUrl, actualServiceUrl1);
            Assert.Equal(expectedServiceUrl, actualServiceUrl2);
            Assert.Equal(expectedServiceUrl, actualServiceUrl3);
            Assert.Equal(expectedServiceUrl, actualServiceUrl4);
            Assert.Equal(expectedServiceUrl, actualServiceUrl5);
            Assert.Equal(expectedServiceUrl, actualServiceUrl6);
        }

        [Fact]
        public async Task CloudAdapterDelay()
        {
            await DelayHelper.Test(new CloudAdapter());
        }

        [Fact]
        public async Task CloudAdapterCreateConversation()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity();

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = claimsIdentity,
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient("appId");

            var conversationResourceResponse = new ConversationResourceResponse();
            var createResponse = new HttpOperationResponse<ConversationResourceResponse> { Body = conversationResourceResponse };

            // note Moq doesn't support extension methods used in the implementation so we are actually mocking the underlying CreateConversationWithHttpMessagesAsync method
            var conversationsMock = new Mock<IConversations>();
            conversationsMock.Setup(cm => cm.CreateConversationWithHttpMessagesAsync(It.IsAny<ConversationParameters>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(createResponse));

            var connectorMock = new Mock<IConnectorClient>();
            connectorMock.SetupGet(m => m.Conversations).Returns(conversationsMock.Object);

            var expectedServiceUrl = "http://serviceUrl";
            var expectedAudience = "audience";

            var connectorFactoryMock = new Mock<ConnectorFactory>();
            connectorFactoryMock.Setup(cf => cf.CreateAsync(It.Is<string>(serviceUrl => serviceUrl == expectedServiceUrl), It.Is<string>(audience => audience == expectedAudience), It.IsAny<CancellationToken>())).Returns(Task.FromResult(connectorMock.Object));

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateConnectorFactory(It.IsAny<ClaimsIdentity>())).Returns(connectorFactoryMock.Object);
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var expectedChannelId = "expected-channel-id";
            var actualChannelId = string.Empty;

            BotCallbackHandler callback1 = (t, c) =>
            {
                actualChannelId = t.Activity.ChannelId;

                return Task.CompletedTask;
            };

            var conversationParameters = new ConversationParameters
            {
                IsGroup = false,
                Bot = new ChannelAccount { },
                Members = new ChannelAccount[] { },
                TenantId = "tenantId",
            };

            // Act
            var adapter = new CloudAdapter(cloudEnvironmentMock.Object);
            await adapter.CreateConversationAsync("botAppId", expectedChannelId, expectedServiceUrl, expectedAudience, conversationParameters, callback1, CancellationToken.None);

            // Assert
            Assert.Equal(expectedChannelId, actualChannelId);
        }

        [Fact(Skip = "Expired token not working anymore, disabling it until fixed.")]
        public async Task ExpiredTokenShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();

            // Expired token with removed AppID
            // This token will be validated against real endpoint https://login.microsoftonline.com/common/discovery/v2.0/keys
            // So when the signing key is removed in endpoint, it will not be Expired exception, we need to generate a new token
            // - create a new app registration: https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/CreateApplicationBlade/quickStartType~/null/isMSAApp~/false
            // - create an access token via powershell script (need to wait for one day so it is expired)
            // $Form = @{
            //     client_id = "YOUR_APP_ID"
            //     scope = "https://api.botframework.com/.default"
            //     client_secret = "YOUR_APP_SECRET"
            //     grant_type = "client_credentials"
            // }
            // (Invoke-WebRequest -Uri 'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token' -Method Post -Form $Form).Content
            // - delete the app
            var token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiJodHRwczovL2FwaS5ib3RmcmFtZXdvcmsuY29tIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvZDZkNDk0MjAtZjM5Yi00ZGY3LWExZGMtZDU5YTkzNTg3MWRiLyIsImlhdCI6MTY5Mjg3MDMwMiwibmJmIjoxNjkyODcwMzAyLCJleHAiOjE2OTI5NTcwMDIsImFpbyI6IkUyRmdZUGhhdFZ6czVydGFFYTlWbDN2ZnIyQ2JBZ0E9IiwiYXBwaWQiOiIxNWYwMTZmZS00ODhjLTQwZTktOWNiZS00Yjk0OGY5OGUyMmMiLCJhcHBpZGFjciI6IjEiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9kNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIvIiwicmgiOiIwLkFXNEFJSlRVMXB2ejkwMmgzTldhazFoeDIwSXpMWTBwejFsSmxYY09EcS05RnJ4dUFBQS4iLCJ0aWQiOiJkNmQ0OTQyMC1mMzliLTRkZjctYTFkYy1kNTlhOTM1ODcxZGIiLCJ1dGkiOiJkenVwa1dWd2FVT2x1RldkbnlvLUFBIiwidmVyIjoiMS4wIn0.sbQH997Q2GDKiiYd6l5MIz_XNfXypJd6zLY9xjtvEgXMBB0x0Vu3fv9W0nM57_ZipQiZDTZuSQA5BE30KBBwU-ZVqQ7MgiTkmE9eF6Ngie_5HwSr9xMK3EiDghHiOP9pIj3oEwGOSyjR5L9n-7tLSdUbKVyV14nS8OQtoPd1LZfoZI3e7tVu3vx8Lx3KzudanXX8Vz7RKaYndj3RyRi4wEN5hV9ab40d7fQsUzygFd5n_PXC2rs0OhjZJzjCOTC0VLQEn1KwiTkSH1E-OSzkrMltn1sbhD2tv_H-4rqQd51vAEJ7esC76qQjz_pfDRLs6T2jvJyhd5MZrN_MT0TqlA";

            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>((_) => token);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateInvokeActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var response = new MemoryStream();
            var httpResponseMock = new Mock<HttpResponse>().SetupAllProperties();
            httpResponseMock.Setup(r => r.Body).Returns(response);

            var loggerMock = new Mock<ILogger<CloudAdapter>>();

            var bot = new InvokeResponseBot();

            // Act
            var adapter = new CloudAdapter(BotFrameworkAuthenticationFactory.Create(), loggerMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, httpResponseMock.Object.StatusCode);

            loggerMock.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("The token has expired")),
                   It.IsAny<Exception>(),
                   (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
               Times.Once);
        }

        private static Stream CreateMessageActivityStream(string userId, string channelId, string conversationId, string recipient, string relatesToActivityId)
        {
            return CreateStream(new Activity
            {
                Type = ActivityTypes.Message,
                Text = "hi",
                ServiceUrl = "http://localhost",
                ChannelId = channelId,
                Conversation = new ConversationAccount { Id = conversationId },
                From = new ChannelAccount { Id = userId },
                Locale = "locale",
                Recipient = new ChannelAccount { Id = recipient },
                RelatesTo = new ConversationReference { ActivityId = relatesToActivityId }
            });
        }

        private static Stream CreateMessageActivityStream()
        {
            return CreateMessageActivityStream("userId", "channelId", "conversationId", "botId", "relatesToActivityId");
        }

        private static Stream CreateBadRequestStream()
        {
            var stream = new MemoryStream();
            var textWriter = new StreamWriter(stream);
            textWriter.Write("this.is.not.json");
            textWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private static HttpResponseMessage CreateInternalHttpResponse()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(new JObject { { "id", "SendActivityId" } }.ToString());
            return response;
        }

        private static Stream CreateInvokeActivityStream()
        {
            return CreateStream(new Activity { Type = ActivityTypes.Invoke, ServiceUrl = "http://localhost" });
        }

        private static Stream CreateStream(Activity activity)
        {
            string json = SafeJsonConvert.SerializeObject(activity, MessageSerializerSettings.Create());
            var stream = new MemoryStream();
            var textWriter = new StreamWriter(stream);
            textWriter.Write(json);
            textWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private class InvokeResponseBot : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                await turnContext.SendActivityAsync(CreateInvokeResponseActivity());
            }

            private static Activity CreateInvokeResponseActivity()
            {
                return new Activity
                {
                    Type = ActivityTypesEx.InvokeResponse,
                    Value = new InvokeResponse
                    {
                        Status = 200,
                        Body = new JObject { { "quite.honestly", "im.feeling.really.attacked.right.now" } },
                    },
                };
            }
        }

        private class MessageBot : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("rage.rage.against.the.dying.of.the.light"));
            }
        }

        private class UserTokenClientBot : IBot
        {
            private string _connectionName;

            public UserTokenClientBot(string connectionName)
            {
                _connectionName = connectionName;
            }

            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
            {
                // in the product the following calls ae made from witin the sign-in prompt begin and continue methods

                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();

                _ = await userTokenClient.ExchangeTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, new TokenExchangeRequest { }, cancellationToken);

                _ = await userTokenClient.GetAadTokensAsync(turnContext.Activity.From.Id, _connectionName, new string[] { "x", "y" }, turnContext.Activity.ChannelId, cancellationToken);

                _ = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity, "finalRedirect", cancellationToken);

                _ = await userTokenClient.GetTokenStatusAsync(turnContext.Activity.From.Id, turnContext.Activity.ChannelId, "includeFilter", cancellationToken);

                _ = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, "magicCode", cancellationToken);

                // in the product code the sign-out call is generally run as a general intercept before any dialog logic

                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken);
            }
        }

        private class TestUserTokenClient : UserTokenClient
        {
            private string _appId;

            public TestUserTokenClient(string appId)
            {
                _appId = appId;
            }

            public IDictionary<string, object[]> Record { get; } = new Dictionary<string, object[]>();

            public override Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken)
            {
                Capture(MethodBase.GetCurrentMethod().Name, userId, connectionName, channelId, exchangeRequest);
                return Task.FromResult(new TokenResponse { });
            }

            public override Task<Dictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, string[] resourceUrls, string channelId, CancellationToken cancellationToken)
            {
                Capture(MethodBase.GetCurrentMethod().Name, userId, connectionName, resourceUrls, channelId);
                return Task.FromResult(new Dictionary<string, TokenResponse> { });
            }

            public override Task<SignInResource> GetSignInResourceAsync(string connectionName, Activity activity, string finalRedirect, CancellationToken cancellationToken)
            {
                var state = CreateTokenExchangeState(_appId, connectionName, activity);
                Capture(MethodBase.GetCurrentMethod().Name, state, finalRedirect);
                return Task.FromResult(new SignInResource { });
            }

            public override Task<TokenStatus[]> GetTokenStatusAsync(string userId, string channelId, string includeFilter, CancellationToken cancellationToken)
            {
                Capture(MethodBase.GetCurrentMethod().Name, userId, channelId, includeFilter);
                return Task.FromResult(new TokenStatus[0]);
            }

            public override Task<TokenResponse> GetUserTokenAsync(string userId, string connectionName, string channelId, string magicCode, CancellationToken cancellationToken)
            {
                Capture(MethodBase.GetCurrentMethod().Name, userId, connectionName, channelId, magicCode);
                return Task.FromResult(new TokenResponse());
            }

            public override Task SignOutUserAsync(string userId, string connectionName, string channelId, CancellationToken cancellationToken)
            {
                Capture(MethodBase.GetCurrentMethod().Name, userId, connectionName, channelId);
                return Task.CompletedTask;
            }

            private void Capture(string name, params object[] args)
            {
                Record.Add(name, args);
            }
        }

        private class ConnectorFactoryBot : IBot
        {
            public IIdentity Identity { get; private set; }

            public IConnectorClient ConnectorClient { get; private set; }

            public UserTokenClient UserTokenClient { get; private set; }

            public BotCallbackHandler BotCallbackHandler { get; private set; }

            public string OAuthScope { get; private set; }

            public AuthenticationHeaderValue Authorization { get; private set; }

            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
            {
                // verify the bot-framework protocol TurnState has been setup by the adapter
                Identity = turnContext.TurnState.Get<IIdentity>("BotIdentity");
                ConnectorClient = turnContext.TurnState.Get<IConnectorClient>();
                UserTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                BotCallbackHandler = turnContext.TurnState.Get<BotCallbackHandler>();
                OAuthScope = turnContext.TurnState.Get<string>("Microsoft.Bot.Builder.BotAdapter.OAuthScope");

                var connectorFactory = turnContext.TurnState.Get<ConnectorFactory>();

                var connector = await connectorFactory.CreateAsync("http://localhost/originalServiceUrl", OAuthScope, cancellationToken);

                var request = new HttpRequestMessage();
                await connector.Credentials.ProcessHttpRequestAsync(request, cancellationToken);
                Authorization = request.Headers.Authorization;
            }
        }

        private class TestCredentials : ServiceClientCredentials
        {
            private string _testToken;

            public TestCredentials(string testToken)
            {
                _testToken = testToken;
            }

            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _testToken);
                return Task.CompletedTask;
            }
        }

        private class TestConnectorFactory : ConnectorFactory
        {
            public override Task<IConnectorClient> CreateAsync(string serviceUrl, string audience, CancellationToken cancellationToken)
            {
                var credentials = new TestCredentials(audience ?? "test-token");
                return Task.FromResult((IConnectorClient)new ConnectorClient(new Uri(serviceUrl), credentials, null, disposeHttpClient: true));
            }
        }

        private class TestContentStream : IContentStream
        {
            public Guid Id { get; set; }

            public string ContentType { get; set; }

            public int? Length { get; set; }

            public Stream Stream { get; set; }
        }

        private class StreamingTestCloudAdapter : CloudAdapter
        {
            private readonly Dictionary<WebSocket, StreamingConnection> _connections;

            public StreamingTestCloudAdapter(BotFrameworkAuthentication auth, Dictionary<WebSocket, StreamingConnection> connections)
                : base(auth)
            {
                _connections = connections;
            }

            protected override StreamingConnection CreateWebSocketConnection(WebSocket socket, ILogger logger)
            {
                return _connections[socket];
            }
        }
    }
}
