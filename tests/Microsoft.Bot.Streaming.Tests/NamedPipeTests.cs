// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class NamedPipeTests
    {
        [Fact]
        public async Task DisconnectWorksAsIntendedAsync()
        {
            // Truncating GUID to make sure the full path does not exceed 104 characters.
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            var reader = new NamedPipeClient(pipeName);
            var writer = new NamedPipeServer(pipeName, new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName));

            try
            {
                // The ConnectAsync task returns only after the connection has been disposed.
                // In this context it makes for ugly code, but in the context of blocking an HTTP
                // response until the the streaming connection has ended it creates an easier to
                // follow user experience.
                var connectTask = reader.ConnectAsync();
                await writer.StartAsync();

                // The writeStream can only connect to the readStream if the readStream is listening for new connections.
                // This creates a dependency between the two tasks below, requiring the two to be running in parallel until
                // the readStream receives an incoming connection and the writeStream establishes an outgoing connection.
                await Task.WhenAll(readStream.WaitForConnectionAsync(), writeStream.ConnectAsync());

                // Assert that the reader is now connected.
                Assert.True(reader.IsConnected, "Reader failed to connect.");

                // The line we're actually testing.                
                reader.Disconnect();

                // Assert that the reader and writer are no longer connected
                Assert.False(reader.IsConnected, "Reader did not disconnect.");
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [Fact]
        public void NamedPipeClient_ctor_With_Empty_BaseName()
        {
            Assert.Throws<ArgumentNullException>(() => new NamedPipeClient(string.Empty));
        }

        [Fact]
        public async Task NamedPipeClient_SendAsync_With_No_Message()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var pipe = new NamedPipeClient(pipeName);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pipe.SendAsync(null));
        }

        [Fact]
        public async Task NamedPipeClient_SendAsync_With_No_Connected_Client()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var pipe = new NamedPipeClient(pipeName);
            var message = new StreamingRequest();

            await Assert.ThrowsAsync<InvalidOperationException>(() => pipe.SendAsync(message));
        }

        [Fact]
        public void NamedPipeServer_IsConnected()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var requestHandler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            var pipe = new NamedPipeServer(pipeName, requestHandler);

            Assert.False(pipe.IsConnected);
        }

        [Fact]
        public void NamedPipeServer_ctor_With_Empty_BaseName()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var requestHandler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            Assert.Throws<ArgumentNullException>(() => new NamedPipeServer(string.Empty, requestHandler));
        }

        [Fact]
        public void NamedPipeServer_ctor_With_No_RequestHandler()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            Assert.Throws<ArgumentNullException>(() => new NamedPipeServer(pipeName, null));
        }

        [Fact]
        public async Task NamedPipeServer_SendAsync_With_No_Message()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var requestHandler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            var pipe = new NamedPipeServer(pipeName, requestHandler);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pipe.SendAsync(null));
        }

        [Fact]
        public async Task NamedPipeServer_SendAsync_With_No_Connected_Client()
        {
            var pipeName = Guid.NewGuid().ToString().Substring(0, 18);
            var requestHandler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            var pipe = new NamedPipeServer(pipeName, requestHandler);
            var message = new StreamingRequest();

            await Assert.ThrowsAsync<InvalidOperationException>(() => pipe.SendAsync(message));
        }
    }
}
