// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Tests.Tools;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Microsoft.Bot.Connector.Streaming.Tests.Features.TestWebSocketConnectionFeature;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    public class WebSocketTransportTests
    {
        [Fact]
        public async Task WebSocketTransport_WhatIsReceivedIsWritten()
        {
            var pipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

            using (var webSocketFeature = new TestWebSocketConnectionFeature())
            {
                // Build transport
                var transport = new WebSocketTransport(pipePair.Application, NullLogger.Instance);

                // Accept web socket, start receiving / sending at the transport level
                var processTask = transport.ProcessSocketAsync(await webSocketFeature.AcceptAsync(), CancellationToken.None);

                // Start a socket client that will capture traffic for posterior analysis
                var clientTask = webSocketFeature.Client.ExecuteAndCaptureFramesAsync();

                // Send a frame, then close
                await webSocketFeature.Client.SendAsync(
                    buffer: new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello")),
                    messageType: WebSocketMessageType.Binary,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
                await webSocketFeature.Client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                var result = await pipePair.Transport.Input.ReadAsync();
                var buffer = result.Buffer;
                Assert.Equal("Hello", Encoding.UTF8.GetString(buffer.ToArray()));
                pipePair.Transport.Input.AdvanceTo(buffer.End);

                pipePair.Transport.Output.Complete();

                // The transport should finish now
                await processTask;

                // The connection should close after this, which means the client will get a close frame.
                var clientSummary = await clientTask;

                Assert.Equal(WebSocketCloseStatus.NormalClosure, clientSummary.CloseResult.CloseStatus);
            }
        }

        [Fact]
        public async Task TransportCommunicatesErrorToApplicationWhenClientDisconnectsAbnormally()
        {
            //using (StartVerifiableLog())
            {
                var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                using (var feature = new TestWebSocketConnectionFeature())
                {
                    async Task CompleteApplicationAfterTransportCompletes()
                    {
                        try
                        {
                            // Wait until the transport completes so that we can end the application
                            var result = await pair.Transport.Input.ReadAsync();
                            pair.Transport.Input.AdvanceTo(result.Buffer.End);
                        }
                        catch (Exception ex)
                        {
                            Assert.IsType<WebSocketError>(ex);
                        }
                        finally
                        {
                            // Complete the application so that the connection unwinds without aborting
                            pair.Transport.Output.Complete();
                        }
                    }

                    var transport = new WebSocketTransport(pair.Application, NullLogger.Instance);

                    // Accept web socket, start receiving / sending at the transport level
                    var processTask = transport.ProcessSocketAsync(await feature.AcceptAsync(), CancellationToken.None);

                    // Start a socket client that will capture traffic for posterior analysis
                    var clientTask = feature.Client.ExecuteAndCaptureFramesAsync();

                    // When the close frame is received, we complete the application so the send
                    // loop unwinds
                    _ = CompleteApplicationAfterTransportCompletes();

                    // Terminate the client to server channel with an exception
                    feature.Client.SendAbort();

                    // Wait for the transport
                    await processTask.TimeoutAfter(TimeSpan.FromSeconds(5));

                    await clientTask.TimeoutAfter(TimeSpan.FromSeconds(5));
                }
            }
        }

        [Fact]
        public async Task ClientReceivesInternalServerErrorWhenTheApplicationFails()
        {
            //using (StartVerifiableLog())
            {
                var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                using (var feature = new TestWebSocketConnectionFeature())
                {
                    var transport = new WebSocketTransport(pair.Application, NullLogger.Instance);

                    // Accept web socket, start receiving / sending at the transport level
                    var processTask = transport.ProcessSocketAsync(await feature.AcceptAsync(), CancellationToken.None);

                    // Start a socket client that will capture traffic for posterior analysis
                    var clientTask = feature.Client.ExecuteAndCaptureFramesAsync();

                    // Fail in the app
                    pair.Transport.Output.Complete(new InvalidOperationException("Catastrophic failure."));
                    var clientSummary = await clientTask.TimeoutAfter<WebSocketConnectionSummary>(TimeSpan.FromSeconds(5));
                    Assert.Equal(WebSocketCloseStatus.InternalServerError, clientSummary.CloseResult.CloseStatus);

                    // Close from the client
                    await feature.Client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                    await processTask.TimeoutAfter(TimeSpan.FromSeconds(5));
                }
            }
        }

        [Fact]
        public async Task TransportClosesOnCloseTimeoutIfClientDoesNotSendCloseFrame()
        {
            //using (StartVerifiableLog())
            {
                var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                using (var feature = new TestWebSocketConnectionFeature())
                {
                    var transport = new WebSocketTransport(pair.Application, NullLogger.Instance);
                    var serverSocket = await feature.AcceptAsync();

                    // Accept web socket, start receiving / sending at the transport level
                    var processTask = transport.ProcessSocketAsync(serverSocket, CancellationToken.None);

                    // Start a socket client that will capture traffic for posterior analysis
                    var clientTask = feature.Client.ExecuteAndCaptureFramesAsync();

                    // End the app
                    pair.Transport.Output.Complete();

                    await processTask.TimeoutAfter(TimeSpan.FromSeconds(10));

                    // Now we're closed
                    Assert.Equal(WebSocketState.Aborted, serverSocket.State);

                    serverSocket.Dispose();
                }
            }
        }

        [Fact]
        public async Task TransportFailsOnTimeoutWithErrorWhenApplicationFailsAndClientDoesNotSendCloseFrame()
        {
            //using (StartVerifiableLog())
            {
                var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                using (var feature = new TestWebSocketConnectionFeature())
                {
                    var transport = new WebSocketTransport(pair.Application, NullLogger.Instance);
                    var serverSocket = await feature.AcceptAsync();

                    // Accept web socket, start receiving / sending at the transport level
                    var processTask = transport.ProcessSocketAsync(serverSocket, CancellationToken.None);

                    // Start a socket client that will capture traffic for posterior analysis
                    var clientTask = feature.Client.ExecuteAndCaptureFramesAsync();

                    // fail the client to server channel
                    pair.Transport.Output.Complete(new Exception());

                    await processTask.TimeoutAfter(TimeSpan.FromSeconds(10));

                    Assert.Equal(WebSocketState.Aborted, serverSocket.State);
                }
            }
        }

        [Fact]
        public async Task ServerGracefullyClosesWhenApplicationEndsThenClientSendsCloseFrame()
        {
            //using (StartVerifiableLog())
            {
                var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                using (var feature = new TestWebSocketConnectionFeature())
                {
                    var transport = new WebSocketTransport(pair.Application, NullLogger.Instance);
                    var serverSocket = await feature.AcceptAsync();

                    // Accept web socket, start receiving / sending at the transport level
                    var processTask = transport.ProcessSocketAsync(serverSocket, CancellationToken.None);

                    // Start a socket client that will capture traffic for posterior analysis
                    var clientTask = feature.Client.ExecuteAndCaptureFramesAsync();

                    // close the client to server channel
                    pair.Transport.Output.Complete();

                    _ = await clientTask.TimeoutAfter(TimeSpan.FromSeconds(5));

                    await feature.Client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).TimeoutAfter(TimeSpan.FromSeconds(5));

                    await processTask.TimeoutAfter(TimeSpan.FromSeconds(5));

                    Assert.Equal(WebSocketCloseStatus.NormalClosure, serverSocket.CloseStatus);
                }
            }
        }

        [Fact]
        public async Task ServerGracefullyClosesWhenClientSendsCloseFrameThenApplicationEnds()
        {
            //using (StartVerifiableLog())
            {
                var pair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

                using (var feature = new TestWebSocketConnectionFeature())
                {
                    var transport = new WebSocketTransport(pair.Application, NullLogger.Instance);
                    var serverSocket = await feature.AcceptAsync();

                    // Accept web socket, start receiving / sending at the transport level
                    var processTask = transport.ProcessSocketAsync(serverSocket, CancellationToken.None);

                    // Start a socket client that will capture traffic for posterior analysis
                    var clientTask = feature.Client.ExecuteAndCaptureFramesAsync();

                    await feature.Client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).TimeoutAfter(TimeSpan.FromSeconds(5));

                    // close the client to server channel
                    pair.Transport.Output.Complete();

                    _ = await clientTask.TimeoutAfter(TimeSpan.FromSeconds(5));

                    await processTask.TimeoutAfter(TimeSpan.FromSeconds(5));

                    Assert.Equal(WebSocketCloseStatus.NormalClosure, serverSocket.CloseStatus);
                }
            }
        }

        [Fact]
        public async Task MultiSegmentSendWillNotSendEmptyEndOfMessageFrame()
        {
            using (var feature = new TestWebSocketConnectionFeature())
            {
                var serverSocket = await feature.AcceptAsync();

                var firstSegment = new byte[] { 1 };
                var secondSegment = new byte[] { 15 };

                var first = new MemorySegment<byte>(firstSegment);
                var last = first.Append(secondSegment);

                var sequence = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);

                Assert.False(sequence.IsSingleSegment);

                await serverSocket.SendAsync(sequence, WebSocketMessageType.Text);

                // Run the client socket
                var client = feature.Client.ExecuteAndCaptureFramesAsync();

                await serverSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default);

                var messages = await client.TimeoutAfter(TimeSpan.FromSeconds(5));
                Assert.Equal(2, messages.Received.Count);

                // First message: 1 byte, endOfMessage false
                Assert.Single(messages.Received[0].Buffer);
                Assert.Equal(1, messages.Received[0].Buffer[0]);
                Assert.False(messages.Received[0].EndOfMessage);

                // Second message: 1 byte, endOfMessage true
                Assert.Single(messages.Received[1].Buffer);
                Assert.Equal(15, messages.Received[1].Buffer[0]);
                Assert.True(messages.Received[1].EndOfMessage);
            }
        }
    }
}
