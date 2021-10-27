// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Tests.Tools;
using Microsoft.Bot.Streaming;
using Microsoft.Extensions.Logging;
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

        [Theory]
        [InlineData(32, 1024)]
        [InlineData(4, 1000)]
        [InlineData(4, 100)]
        [InlineData(8, 100)]
        [InlineData(16, 100)]
        [InlineData(32, 100)]
        public async Task Integration_Interop_LegacyClient_MiniLoad(int threadCount, int messageCount)
        {
            var logger = XUnitLogger.CreateLogger(_outputHelper);

            using (var webSocketFeature = new TestWebSocketConnectionFeature())
            {
                var botRequestHandler = new Mock<RequestHandler>();

                botRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                var connection = new WebSocketStreamingConnection(logger);

                var socket = await webSocketFeature.AcceptAsync().ConfigureAwait(false);
                var serverTask = Task.Run(() => connection.ListenInternalAsync(socket, botRequestHandler.Object));
                await Task.Delay(TimeSpan.FromSeconds(1));
                var clients = new List<Microsoft.Bot.Streaming.Transport.WebSockets.WebSocketClient>();

                var clientRequestHandler = new Mock<RequestHandler>();

                clientRequestHandler
                    .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                    .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                using (var client = new Microsoft.Bot.Streaming.Transport.WebSockets.WebSocketClient(
                        "wss://test",
                        clientRequestHandler.Object))
                {
                    await client.ConnectInternalAsync(webSocketFeature.Client).ConfigureAwait(false);
                    clients.Add(client);

                    // Send request bot (server) -> channel (client)
                    const string path = "api/version";
                    const string botToClientPayload = "Hello human, I'm Bender!";

                    Func<int, Task> testFlow = async (i) =>
                    {
                        var request = StreamingRequest.CreatePost(path, new StringContent(botToClientPayload));

                        var stopwatch = Stopwatch.StartNew();
                        var responseFromClient =
                            await connection.SendStreamingRequestAsync(request).ConfigureAwait(false);
                        stopwatch.Stop();

                        Assert.Equal(200, responseFromClient.StatusCode);
                        logger.LogInformation(
                            $"Server->Client {i} latency: {stopwatch.ElapsedMilliseconds}. Status code: {responseFromClient.StatusCode}");

                        const string clientToBotPayload = "Hello bot, I'm Calculon!";
                        var clientRequest = StreamingRequest.CreatePost(path, new StringContent(clientToBotPayload));

                        stopwatch = Stopwatch.StartNew();

                        // Send request bot channel (client) -> (server) 
                        var clientToBotResult = await client.SendAsync(clientRequest).ConfigureAwait(false);
                        stopwatch.Stop();

                        Assert.Equal(200, clientToBotResult.StatusCode);

                        logger.LogInformation(
                            $"Client->Server {i} latency: {stopwatch.ElapsedMilliseconds}. Status code: {responseFromClient.StatusCode}");
                    };

                    await testFlow(-1).ConfigureAwait(false);
                    var tasks = new List<Task>();

                    using (var throttler = new SemaphoreSlim(threadCount))
                    {
                        for (int j = 0; j < messageCount; j++)
                        {
                            await throttler.WaitAsync().ConfigureAwait(false);

                            // using Task.Run(...) to run the lambda in its own parallel
                            // flow on the threadpool
                            tasks.Add(
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        await testFlow(j).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }));
                        }

                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }

                    client.Disconnect();
                }

                await serverTask.ConfigureAwait(false);
            }
        }

        [Theory]
        [InlineData(32, 1000)]
        public async Task Integration_NewClient_MiniLoad(int threadCount, int messageCount)
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
                await Task.Delay(TimeSpan.FromSeconds(1));

                //Parallel.For(0, clientCount, async i => 
                {
                    // Client / channel setup
                    var clientRequestHandler = new Mock<RequestHandler>();

                    clientRequestHandler
                        .Setup(r => r.ProcessRequestAsync(It.IsAny<ReceiveRequest>(), null, null, CancellationToken.None))
                        .ReturnsAsync(() => new StreamingResponse() { StatusCode = 200 });

                    using (var client = new WebSocketClient($"wss://test", clientRequestHandler.Object, logger: logger))
                    {
                        var clientTask = client.ConnectInternalAsync(webSocketFeature.Client, CancellationToken.None);

                        // Send request bot (server) -> channel (client)
                        const string path = "api/version";
                        const string botToClientPayload = "Hello human, I'm Bender!";

                        Func<int, Task> testFlow = async (i) =>
                        {
                            var request = StreamingRequest.CreatePost(path, new StringContent(botToClientPayload));

                            var stopwatch = Stopwatch.StartNew();
                            var responseFromClient = await connection.SendStreamingRequestAsync(request).ConfigureAwait(false);
                            stopwatch.Stop();

                            Assert.Equal(200, responseFromClient.StatusCode);
                            logger.LogInformation($"Server->Client {i} latency: {stopwatch.ElapsedMilliseconds}. Status code: {responseFromClient.StatusCode}");

                            const string clientToBotPayload = "Hello bot, I'm Calculon!";
                            var clientRequest = StreamingRequest.CreatePost(path, new StringContent(clientToBotPayload));

                            stopwatch = Stopwatch.StartNew();

                            // Send request bot channel (client) -> (server) 
                            var clientToBotResult = await client.SendAsync(clientRequest).ConfigureAwait(false);
                            stopwatch.Stop();

                            Assert.Equal(200, clientToBotResult.StatusCode);

                            logger.LogInformation($"Client->Server {i} latency: {stopwatch.ElapsedMilliseconds}. Status code: {responseFromClient.StatusCode}");
                        };

                        await testFlow(-1).ConfigureAwait(false);
                        var tasks = new List<Task>();

                        using (var throttler = new SemaphoreSlim(threadCount))
                        {
                            for (int j = 0; j < messageCount; j++)
                            {
                                await throttler.WaitAsync().ConfigureAwait(false);

                                // using Task.Run(...) to run the lambda in its own parallel
                                // flow on the threadpool
                                tasks.Add(
                                    Task.Run(async () =>
                                    {
                                        try
                                        {
                                            await testFlow(j).ConfigureAwait(false);
                                        }
                                        finally
                                        {
                                            throttler.Release();
                                        }
                                    }));
                            }

                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }

                        await client.DisconnectAsync().ConfigureAwait(false);
                        await clientTask.ConfigureAwait(false);
                    }

                    await serverTask.ConfigureAwait(false);
                }
            }
        }

        private static void RunWithLimitedParalelism(List<Task> tasks, int maxTasksToRunInParallel, int timeoutInMilliseconds, CancellationToken cancellationToken = new CancellationToken())
        {
            // Convert to a list of tasks so that we don&#39;t enumerate over it multiple times needlessly.
            using (var throttler = new SemaphoreSlim(maxTasksToRunInParallel))
            {
                var postTaskTasks = new List<Task>();

                // Have each task notify the throttler when it completes so that it decrements the number of tasks currently running.
                tasks.ForEach(t => postTaskTasks.Add(t.ContinueWith(tsk => throttler.Release())));

                // Start running each task.
                foreach (var task in tasks)
                {
                    // Increment the number of tasks currently running and wait if too many are running.
                    throttler.Wait(timeoutInMilliseconds, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                    task.Start();
                }

                // Wait for all of the provided tasks to complete.
                // We wait on the list of "post" tasks instead of the original tasks, otherwise there is a potential race condition where the throttler&#39;s using block is exited before some Tasks have had their "post" action completed, which references the throttler, resulting in an exception due to accessing a disposed object.
                Task.WaitAll(postTaskTasks.ToArray(), cancellationToken);
            }
        }
    }
}
