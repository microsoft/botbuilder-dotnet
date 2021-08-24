// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Tests.Features;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Bot.Connector.Streaming.Tests
{
    public class WebSocketTransportClientServerTests
    {
        [Fact]
        public async Task WebSocketTransport_ClientServer_WhatIsSentIsReceived()
        {
            var serverPipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            var clientPipePair = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);

            using (var webSocketFeature = new TestWebSocketConnectionFeature())
            {
                // Build server transport
                var serverTransport = new WebSocketTransport(serverPipePair.Application, NullLogger.Instance);

                // Accept server web socket, start receiving / sending at the transport level
                var serverTask = serverTransport.ProcessSocketAsync(await webSocketFeature.AcceptAsync(), CancellationToken.None);

                var clientTransport = new WebSocketTransport(clientPipePair.Application, NullLogger.Instance);
                var clientTask = clientTransport.ProcessSocketAsync(webSocketFeature.Client, CancellationToken.None);

                // Send a frame client -> server
                await clientPipePair.Transport.Output.WriteAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Hello")));
                await clientPipePair.Transport.Output.FlushAsync();

                var result = await serverPipePair.Transport.Input.ReadAsync();
                var buffer = result.Buffer;

                Assert.Equal("Hello", Encoding.UTF8.GetString(buffer.ToArray()));
                serverPipePair.Transport.Input.AdvanceTo(buffer.End);

                // Send a frame server -> client
                await serverPipePair.Transport.Output.WriteAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("World")));
                await serverPipePair.Transport.Output.FlushAsync();

                var clientResult = await clientPipePair.Transport.Input.ReadAsync();
                buffer = clientResult.Buffer;

                Assert.Equal("World", Encoding.UTF8.GetString(buffer.ToArray()));
                clientPipePair.Transport.Input.AdvanceTo(buffer.End);

                clientPipePair.Transport.Output.Complete();
                serverPipePair.Transport.Output.Complete();

                // The transport should finish now
                await serverTask;
                await clientTask;
            }
        }
    }
}
