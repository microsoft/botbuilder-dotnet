// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Tests.Tools;
using Microsoft.Bot.Streaming;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    public class ApplicationToApplicationIntegrationTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ApplicationToApplicationIntegrationTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Integration_ListenSendShutDownServer()
        {
            // TODO: Transform this test into a theory and do multi-message, multi-thread, multi-client, etc.
            var logger = XUnitLogger.CreateLogger(_outputHelper);

            using (var webSocketFeature = new TestWebSocketConnectionFeature())            
            {
                // Bot / server setup
                var botRequestHandler = new Mock<RequestHandler>();
                
                botRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                var socket = await webSocketFeature.AcceptAsync();
                var connection = new WebSocketStreamingConnection(socket, logger);

                var serverTask = Task.Run(() => connection.ListenAsync(botRequestHandler.Object));

                // Client / channel setup
                var clientRequestHandler = new Mock<RequestHandler>();
                
                clientRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                var client = new WebSocketClient(webSocketFeature.Client, "wss://test", clientRequestHandler.Object, logger: logger);
                
                var clientTask = Task.Run(() => client.ConnectInternalAsync(CancellationToken.None));

                // Send request bot (server) -> channel (client)
                const string path = "api/version";
                const string botToClientPayload = "Hello human, I'm Bender!";
                var request = StreamingRequest.CreatePost(path, new StringContent(botToClientPayload));

                var responseFromClient = await connection.SendStreamingRequestAsync(request);

                Assert.Equal(200, responseFromClient.StatusCode);

                const string clientToBotPayload = "Hello bot, I'm Calculon!";
                var clientRequest = StreamingRequest.CreatePost(path, new StringContent(clientToBotPayload));

                // Send request bot channel (client) -> (server) 
                var clientToBotResult = await client.SendAsync(clientRequest);

                Assert.Equal(200, clientToBotResult.StatusCode);

                await client.DisconnectAsync();

                await clientTask;
                await serverTask;
            }
        }

        [Fact]
        public async Task Integration_KeepAlive()
        {
            // TODO: Transform this test into a theory and do multi-message, multi-thread, multi-client, etc.
            var logger = XUnitLogger.CreateLogger(_outputHelper);
            var cts = new CancellationTokenSource();

            using (var webSocketFeature = new TestWebSocketConnectionFeature())
            {
                // Bot / server setup
                var botRequestHandler = new Mock<RequestHandler>();

                botRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                var socket = await webSocketFeature.AcceptAsync();
                var connection = new WebSocketStreamingConnection(socket, logger);
                var serverTask = connection.ListenAsync(botRequestHandler.Object, cts.Token);

                // Client / channel setup
                var clientRequestHandler = new Mock<RequestHandler>();

                clientRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                var client = new WebSocketClient(webSocketFeature.Client, "wss://test", clientRequestHandler.Object, logger: logger, closeTimeOut: TimeSpan.FromSeconds(10), keepAlive: TimeSpan.FromMilliseconds(200));

                var clientTask = client.ConnectInternalAsync(CancellationToken.None);

                // Send request bot (server) -> channel (client)
                const string path = "api/version";
                const string botToClientPayload = "Hello human, I'm Bender!";
                var request = StreamingRequest.CreatePost(path, new StringContent(botToClientPayload));

                var responseFromClient = await connection.SendStreamingRequestAsync(request);

                Assert.Equal(200, responseFromClient.StatusCode);

                const string clientToBotPayload = "Hello bot, I'm Calculon!";
                var clientRequest = StreamingRequest.CreatePost(path, new StringContent(clientToBotPayload));

                // Send request bot channel (client) -> (server) 
                var clientToBotResult = await client.SendAsync(clientRequest);

                Assert.Equal(200, clientToBotResult.StatusCode);

                await Task.Delay(TimeSpan.FromSeconds(3));

                Assert.True(client.IsConnected);
            }
        }
    }
}
