using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class NamedPipeTests
    {
        [Fact]
        public void Close_DisconnectsStreamAsync()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(Guid.NewGuid().ToString(), pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            // new StreamingRequestHandler(null, new DirectLineAdapter(), pipeName);
            var reader = new NamedPipeClient(pipeName);
            var writer = new NamedPipeServer(pipeName, new StreamingRequestHandler(null, new DirectLineAdapter(), pipeName));

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
