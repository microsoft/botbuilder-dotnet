// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Tests.Tools;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Activity = Microsoft.Bot.Schema.Activity;

namespace Microsoft.Bot.Connector.Streaming.Tests.Integration
{
    public class EndToEndMiniLoadTests
    {
        private readonly ITestOutputHelper _testOutput;

        public EndToEndMiniLoadTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void SimpleActivityTest()
        {
            var logger = XUnitLogger.CreateLogger(_testOutput);

            // Arrange
            var activities = new[]
            {
                new Activity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Type = ActivityTypes.Message,
                    From = new ChannelAccount { Id = "testUser" },
                    Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                    Recipient = new ChannelAccount { Id = "testBot" },
                    ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                    ChannelId = "test",
                    Text = "hi"
                }
            };

            var verifiedResponses = activities.ToDictionary(a => a.Id, a => false);

            var bot = new StreamingTestBot((turnContext, cancellationToken) =>
            {
                var activityClone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(turnContext.Activity));
                activityClone.Text = $"Echo: {turnContext.Activity.Text}";

                return turnContext.SendActivityAsync(activityClone, cancellationToken);
            });

            var server = new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);

            var clientRequestHandler = new Mock<RequestHandler>();
            clientRequestHandler
                .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns<ReceiveRequest, ILogger<RequestHandler>, object, CancellationToken>((request, anonLogger, context, cancellationToken) =>
                {
                    var body = request.ReadBodyAsString();
                    var response = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                    Assert.NotNull(response);
                    Assert.Equal("Echo: hi", response.Text);

                    verifiedResponses[response.ReplyToId] = true;

                    return Task.FromResult(StreamingResponse.OK());
                });

            // Act
            RunActivityStreamingTest(activities, bot, server, clientRequestHandler.Object, logger);

            // Assert
            Assert.True(verifiedResponses.Values.All(verifiedResponse => verifiedResponse));
        }

        [Fact]
        public void ActivityWithSuggestedActionsTest()
        {
            var logger = XUnitLogger.CreateLogger(_testOutput);

            var actions = new List<CardAction>
            {
                new CardAction() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red", Image = "https://via.placeholder.com/20/FF0000?text=R", ImageAltText = "R" },
                new CardAction() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow", Image = "https://via.placeholder.com/20/FFFF00?text=Y", ImageAltText = "Y" },
                new CardAction() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue", Image = "https://via.placeholder.com/20/0000FF?text=B", ImageAltText = "B" }
            };

            // Arrange
            var activities = new[]
            {
                new Activity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Type = ActivityTypes.Message,
                    From = new ChannelAccount { Id = "testUser" },
                    Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                    Recipient = new ChannelAccount { Id = "testBot" },
                    ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                    ChannelId = "test",
                    Text = "hi",
                    SuggestedActions = new SuggestedActions(actions: actions)
                }
            };

            var verifiedResponses = activities.ToDictionary(a => a.Id, a => false);

            var bot = new StreamingTestBot((turnContext, cancellationToken) =>
            {
                var activityClone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(turnContext.Activity));
                activityClone.Text = $"Echo: {turnContext.Activity.Text}";

                return turnContext.SendActivityAsync(activityClone, cancellationToken);
            });

            var server = new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);

            var clientRequestHandler = new Mock<RequestHandler>();
            clientRequestHandler
                .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns<ReceiveRequest, ILogger<RequestHandler>, object, CancellationToken>((request, anonLogger, context, cancellationToken) =>
                {
                    var body = request.ReadBodyAsString();
                    var response = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                    Assert.NotNull(response);
                    Assert.Equal("Echo: hi", response.Text);
                    Assert.Equal(3, response.SuggestedActions.Actions.Count);

                    verifiedResponses[response.ReplyToId] = true;

                    return Task.FromResult(StreamingResponse.OK());
                });

            // Act
            RunActivityStreamingTest(activities, bot, server, clientRequestHandler.Object, logger);

            // Assert
            Assert.True(verifiedResponses.Values.All(verifiedResponse => verifiedResponse));
        }

        [Fact]
        public void ActivityWithAttachmentsTest()
        {
            var logger = XUnitLogger.CreateLogger(_testOutput);

            // Arrange
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = "test",
                Text = "1"
            };
            var activityWithAttachment = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = "test",
                Text = "2",
            };
            activityWithAttachment.Attachments.Add(new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png")))}",
            });

            var activities = new[] { activity, activityWithAttachment };

            var verifiedResponses = activities.ToDictionary(a => a.Id, a => false);

            var bot = new StreamingTestBot((turnContext, cancellationToken) =>
            {
                switch (turnContext.Activity.Text)
                {
                    case "1":
                        var response1 = MessageFactory.Text("Echo: 1");
                        response1.Attachments.Add(
                            new Attachment
                            {
                                Name = @"Resources\architecture-resize.png",
                                ContentType = "image/png",
                                ContentUrl = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png")))}",
                            });
                        return turnContext.SendActivityAsync(response1, cancellationToken);

                    case "2":
                        var response2 = MessageFactory.Text("Echo: 2");
                        return turnContext.SendActivityAsync(response2, cancellationToken);

                    default:
                        throw new ApplicationException("Unknown Activity!");
                }
            });

            var server = new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);

            var clientRequestHandler = new Mock<RequestHandler>();
            clientRequestHandler
                .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns<ReceiveRequest, ILogger<RequestHandler>, object, CancellationToken>((request, anonLogger, context, cancellationToken) =>
                {
                    try
                    {
                        var body = request.ReadBodyAsString();
                        var response = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                        Assert.NotNull(response);
                        Assert.Equal($"Echo: {activities.FirstOrDefault(a => a.Id == response.ReplyToId)?.Text}", response.Text);

                        verifiedResponses[response.ReplyToId] = true;

                        return Task.FromResult(StreamingResponse.OK());
                    }
                    catch (Exception e)
                    {
                        return Task.FromResult(StreamingResponse.InternalServerError(new StringContent(e.ToString())));
                    }
                });

            // Act
            RunActivityStreamingTest(activities, bot, server, clientRequestHandler.Object, logger);

            // Assert
            Assert.True(verifiedResponses.Values.All(verifiedResponse => verifiedResponse));
        }

        [Fact]
        public void ServerStopsGracefullyOnClientCrash()
        {
            RunStreamingCrashTest((webSocket, clientWebSocket, client, serverCts, clientCts) => clientWebSocket.Abort());
        }

        [Fact]
        public void ServerStopsGracefullyOnClientDisconnect()
        {
            RunStreamingCrashTest((webSocket, clientWebSocket, client, serverCts, clientCts) => client.Disconnect());
        }

        [Fact]
        public void ServerStopsGracefullyOnClientCancellation()
        {
            RunStreamingCrashTest((webSocket, clientWebSocket, client, serverCts, clientCts) => clientCts.Cancel());
        }

        [Fact]
        public void ClientStopsGracefullyOnServerCrash()
        {
            RunStreamingCrashTest((webSocket, clientWebSocket, client, serverCts, clientCts) => webSocket.Abort());
        }

        [Fact]
        public void ClientStopsGracefullyOnServerCancellation()
        {
            RunStreamingCrashTest((webSocket, clientWebSocket, client, serverCts, clientCts) => serverCts.Cancel());
        }

        [Theory]
        [InlineData(3, 50, 4)]
        [InlineData(3, 100, 32)]
        public void ConcurrencyTest(int connectionCount, int messageCount, int threadCount)
        {
            var logger = XUnitLogger.CreateLogger(_testOutput);

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "wss://InvalidServiceUrl/api/messages",
                ChannelId = "test",
                Text = "hi",
            };
            activity.Attachments.Add(new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png")))}",
            });

            var activities = new[] { activity };

            var bot = new StreamingTestBot((turnContext, cancellationToken) =>
            {
                var response = MessageFactory.Text("Echo: hi");
                response.Attachments.Add(
                    new Attachment
                    {
                        Name = @"Resources\architecture-resize.png",
                        ContentType = "image/png",
                        ContentUrl = $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png")))}",
                    });
                return turnContext.SendActivityAsync(response, cancellationToken);
            });

            var server = new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);

            var clientRequestHandler = new Mock<RequestHandler>();
            clientRequestHandler
                .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns<ReceiveRequest, ILogger<RequestHandler>, object, CancellationToken>((request, anonLogger, context, cancellationToken) =>
                {
                    try
                    {
                        var body = request.ReadBodyAsString();
                        var response = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);
                        Assert.NotNull(response);

                        Assert.Equal($"Echo: {activities.FirstOrDefault(a => a.Id == response.ReplyToId)?.Text}", response.Text);
                        Assert.Equal(1, response.Attachments.Count);

                        return Task.FromResult(StreamingResponse.OK());
                    }
                    catch (Exception e)
                    {
                        return Task.FromResult(StreamingResponse.InternalServerError(new StringContent(e.ToString())));
                    }
                });

            var connections = new Task[connectionCount];

            for (var i = 0; i < connectionCount; i++)
            {
                connections[i] = Task.Factory.StartNew(() =>
                    RunActivityStreamingTest(activities, bot, server, clientRequestHandler.Object, logger, messageCount, threadCount));
            }

            Task.WhenAll(connections).Wait();
        }

        [Fact]
        public void NamedPipeActivityTest()
        {
            const string pipeName = "test.pipe";
            var logger = XUnitLogger.CreateLogger(_testOutput);

            // Arrange
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = "testUser" },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString("N") },
                Recipient = new ChannelAccount { Id = "testBot" },
                ServiceUrl = "unknown",
                ChannelId = "test",
                Text = "hi"
            };

            var bot = new StreamingTestBot((turnContext, cancellationToken) =>
            {
                var activityClone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(turnContext.Activity));
                activityClone.Text = $"Echo: {turnContext.Activity.Text}";

                return turnContext.SendActivityAsync(activityClone, cancellationToken);
            });

            var verifiedResponse = false;
            var clientRequestHandler = new Mock<RequestHandler>();
            clientRequestHandler
                .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns<ReceiveRequest, ILogger<RequestHandler>, object, CancellationToken>((request, anonLogger, context, cancellationToken) =>
                {
                    var body = request.ReadBodyAsString();
                    var response = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                    Assert.NotNull(response);
                    Assert.Equal("Echo: hi", response.Text);
                    verifiedResponse = true;

                    return Task.FromResult(StreamingResponse.OK());
                });

            // Act
            var server = new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);
            var serverRunning = server.ConnectNamedPipeAsync(pipeName, bot, "testAppId", "testAudience", "testCallerId");
            var client = new NamedPipeClient(pipeName, ".", clientRequestHandler.Object, logger: logger);
            var clientRunning = client.ConnectAsync();
            SimulateMultiTurnConversation(1, new[] { activity }, client, logger);

            // Assert
            Assert.True(verifiedResponse);
        }

        private static HttpRequest CreateWebSocketUpgradeRequest(WebSocket webSocket)
        {
            var webSocketManager = new Mock<WebSocketManager>();
            webSocketManager.Setup(m => m.IsWebSocketRequest).Returns(true);
            webSocketManager.Setup(m => m.AcceptWebSocketAsync()).Returns(Task.FromResult(webSocket));

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

        private static void SimulateMultiTurnConversation(int conversationId, Activity[] activities, IStreamingTransportClient client, ILogger logger, SemaphoreSlim throttler = null)
        {
            try
            {
                foreach (var activity in activities)
                {
                    var timer = Stopwatch.StartNew();

                    var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");
                    var response = client.SendAsync(StreamingRequest.CreatePost("/api/messages", content)).Result;

                    logger.LogInformation($"Conversation {conversationId} latency: {timer.ElapsedMilliseconds}. Status code: {response.StatusCode}");
                }
            }
            finally
            {
                if (throttler != null)
                {
                    throttler.Release();
                }
            }
        }

        private static void RunActivityStreamingTest(Activity[] activities, IBot bot, IBotFrameworkHttpAdapter server, RequestHandler clientRequestHandler, ILogger logger, int messageCount = 1, int threadCount = 1)
        {
            using (var connection = new TestWebSocketConnectionFeature())
            {
                var webSocket = connection.AcceptAsync().Result;
                var clientWebSocket = connection.Client;
                var client = new WebSocketClient(clientWebSocket, "wss://test", clientRequestHandler, logger: logger);

                var serverRunning = server.ProcessAsync(CreateWebSocketUpgradeRequest(webSocket), new Mock<HttpResponse>().Object, bot, CancellationToken.None);
                var clientRunning = client.ConnectInternalAsync(CancellationToken.None);

                if (messageCount == 1)
                {
                    SimulateMultiTurnConversation(1, activities, client, logger);
                }
                else
                {
                    var conversations = new Task[messageCount];
                    var throttler = new SemaphoreSlim(threadCount);

                    for (var i = 0; i < messageCount; i++)
                    {
                        throttler.Wait();

                        var conversationId = i;
                        conversations[i] = Task.Factory.StartNew(() => SimulateMultiTurnConversation(conversationId, activities, client, logger, throttler));
                    }

                    Task.WhenAny(Task.WhenAll(conversations), Task.Delay(new TimeSpan(0, 0, 5, 0))).Wait();
                }

                clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of test", CancellationToken.None).Wait();

                clientRunning.Wait();
                Assert.Equal(TaskStatus.RanToCompletion, clientRunning.Status);

                serverRunning.Wait();
                Assert.Equal(TaskStatus.RanToCompletion, serverRunning.Status);
            }
        }

        private void RunStreamingCrashTest(Action<WebSocket, TestWebSocketConnectionFeature.WebSocketChannel, WebSocketClient, CancellationTokenSource, CancellationTokenSource> induceCrash)
        {
            var logger = XUnitLogger.CreateLogger(_testOutput);

            var serverCts = new CancellationTokenSource();
            var clientCts = new CancellationTokenSource();

            using (var connection = new TestWebSocketConnectionFeature())
            {
                var webSocket = connection.AcceptAsync().Result;
                var clientWebSocket = connection.Client;

                var bot = new StreamingTestBot((turnContext, cancellationToken) => Task.CompletedTask);

                var server = new CloudAdapter(new StreamingTestBotFrameworkAuthentication(), logger);
                var serverRunning = server.ProcessAsync(CreateWebSocketUpgradeRequest(webSocket), new Mock<HttpResponse>().Object, bot, serverCts.Token);

                var clientRequestHandler = new Mock<RequestHandler>();
                clientRequestHandler
                    .Setup(h => h.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), It.IsAny<ILogger<RequestHandler>>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(StreamingResponse.OK()));
                using (var client = new WebSocketClient(clientWebSocket, "wss://test", clientRequestHandler.Object, logger: logger))
                {
                    var clientRunning = client.ConnectInternalAsync(clientCts.Token);

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

                    var content = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");
                    var response = client.SendAsync(StreamingRequest.CreatePost("/api/messages", content)).Result;
                    Assert.Equal(200, response.StatusCode);

                    induceCrash(webSocket, clientWebSocket, client, serverCts, clientCts);

                    clientRunning.Wait();
                    Assert.True(clientRunning.IsCompletedSuccessfully);
                }

                serverRunning.Wait();
                Assert.True(serverRunning.IsCompletedSuccessfully);
            }
        }

        private class StreamingTestBot : ActivityHandler
        {
            private readonly Func<ITurnContext<IMessageActivity>, CancellationToken, Task> _onMessageActivityAsync;

            public StreamingTestBot(Func<ITurnContext<IMessageActivity>, CancellationToken, Task> onMessageActivityAsync)
            {
                _onMessageActivityAsync = onMessageActivityAsync ?? throw new ArgumentNullException(nameof(onMessageActivityAsync));
            }

            protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
            {
                return _onMessageActivityAsync(turnContext, cancellationToken);
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
