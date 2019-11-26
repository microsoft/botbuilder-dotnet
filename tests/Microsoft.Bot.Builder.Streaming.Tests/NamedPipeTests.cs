using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Streaming.Tests.Utilities;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
{
    public class NamedPipeTests
    {
        [Fact]
        public void Close_DisconnectsStream()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            new StreamingRequestHandler(new Microsoft.Bot.Streaming.UnitTests.Mocks.MockBot(), new BotFrameworkHttpAdapter(), pipeName);
            var reader = new NamedPipeClient(pipeName);
            var writer = new NamedPipeServer(pipeName, new StreamingRequestHandler(new Microsoft.Bot.Streaming.UnitTests.Mocks.MockBot(), new BotFrameworkHttpAdapter(), pipeName));

            try
            {
                reader.ConnectAsync();
                writer.StartAsync();
                var tasks = new List<Task>();
                tasks.Add(Task.Run(
                    async () =>
                    {
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                    }));

                Task.WaitAll(tasks.ToArray());

                Assert.Equal(true, reader.IsConnected);

                reader.Disconnect();

                Assert.Equal(false, reader.IsConnected);
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }
    }
}
