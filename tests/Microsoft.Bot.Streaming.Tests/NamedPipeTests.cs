// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class NamedPipeTests
    {
        [Fact]
        public void Close_DisconnectsStreamAsync()
        {
            var pipeName = Guid.NewGuid().ToString();
            var appId = Guid.NewGuid().ToString();
            var appPassword = "password123";
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            new StreamingRequestHandler(new Microsoft.Bot.Streaming.UnitTests.Mocks.MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            var reader = new NamedPipeClient(pipeName);
            var writer = new NamedPipeServer(pipeName, new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), pipeName));

            try
            {
                reader.ConnectAsync();
                writer.StartAsync();

                readStream.WaitForConnectionAsync().ConfigureAwait(false);
                writeStream.ConnectAsync(500).ConfigureAwait(false);
                writer.Disconnect();
                reader.Disconnect();
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }

            Assert.False(reader.IsConnected);
        }
    }
}
