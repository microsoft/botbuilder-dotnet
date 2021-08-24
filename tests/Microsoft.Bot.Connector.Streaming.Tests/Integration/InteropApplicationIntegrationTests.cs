// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class InteropApplicationIntegrationTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public InteropApplicationIntegrationTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Integration_Interop_LegacyClient()
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

                var connection = new WebSocketStreamingConnection(logger);

                var socket = await webSocketFeature.AcceptAsync().ConfigureAwait(false);
                var serverTask = Task.Run(() => connection.ListenInternalAsync(socket, botRequestHandler.Object));

                // Client / channel setup
                var clientRequestHandler = new Mock<RequestHandler>();

                clientRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                using (var client = new Microsoft.Bot.Streaming.Transport.WebSockets.WebSocketClient("wss://test", clientRequestHandler.Object))
                {
                    await client.ConnectInternalAsync(webSocketFeature.Client).ConfigureAwait(false);

                    // Send request bot (server) -> channel (client)
                    const string path = "api/version";
                    const string botToClientPayload = "Hello human, I'm Bender!";
                    var request = StreamingRequest.CreatePost(path, new StringContent(botToClientPayload));

                    var responseFromClient = await connection.SendStreamingRequestAsync(request).ConfigureAwait(false);

                    Assert.Equal(200, responseFromClient.StatusCode);

                    const string clientToBotPayload = "Hello bot, I'm Calculon!";
                    var clientRequest = StreamingRequest.CreatePost(path, new StringContent(clientToBotPayload));

                    // Send request bot channel (client) -> (server) 
                    var clientToBotResult = await client.SendAsync(clientRequest).ConfigureAwait(false);

                    Assert.Equal(200, clientToBotResult.StatusCode);
                    client.Disconnect();
                }

                await serverTask.ConfigureAwait(false);
            }
        }
    }
}
