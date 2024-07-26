// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class NamedPipeTransportTests
    {
        private readonly ITestOutputHelper _output;

        public NamedPipeTransportTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task CanWriteAndReadAsync()
        {
            var tasks = new List<Task>();

            var data = new byte[100];
            for (var b = 0; b < data.Length; b++)
            {
                data[b] = (byte)b;
            }

            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            try
            {
                var reader = new NamedPipeTransport(readStream);
                var writer = new NamedPipeTransport(writeStream);

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        _output.WriteLine("After WaitForConnectAsync");

                        var readBuffer = new byte[data.Length];
                        var length = await reader.ReceiveAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);

                        _output.WriteLine("After Read");
                        Assert.Equal(length, data.Length);
                        for (var b = 0; b < data.Length; b++)
                        {
                            Assert.Equal(readBuffer[b], data[b]);
                        }
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        _output.WriteLine("After ConnectAsync");
                        await writer.SendAsync(data, 0, data.Length).ConfigureAwait(false);
                        _output.WriteLine("After Write");
                    }));

                await Task.WhenAll(tasks.ToArray());
            }
            finally
            {
                readStream.Disconnect();
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [Fact]
        public async Task ReceiveAsync_With_Null_Stream()
        {
            var pipe = new NamedPipeTransport(null);
            var result = await pipe.ReceiveAsync(new byte[0], 0, 0);

            Assert.Equal(0, result);
        }

        [Fact]
        public void ClosedStream_Not_IsConnected()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var reader = new NamedPipeTransport(readStream);

            Assert.False(reader.IsConnected);
            readStream.Dispose();
        }

        [Fact]
        public async Task ActiveStream_IsConnectedAsync()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            var reader = new NamedPipeTransport(readStream);
            var writer = new NamedPipeTransport(writeStream);

            try
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        _output.WriteLine("After WaitForConnectAsync");
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        _output.WriteLine("After ConnectAsync");
                    }));

                await Task.WhenAll(tasks.ToArray());

                Assert.True(reader.IsConnected);
                Assert.True(writer.IsConnected);
            }
            finally
            {
                readStream.Disconnect();
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [Fact]
        public async Task Dispose_DisconnectsStreamAsync()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            var reader = new NamedPipeTransport(readStream);
            var writer = new NamedPipeTransport(writeStream);

            try
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        _output.WriteLine("After WaitForConnectAsync");
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        _output.WriteLine("After ConnectAsync");
                    }));

                await Task.WhenAll(tasks.ToArray());

                Assert.True(reader.IsConnected);

                reader.Dispose();

                Assert.False(reader.IsConnected);
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [Fact]
        public async Task Close_DisconnectsStreamAsync()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            var reader = new NamedPipeTransport(readStream);
            var writer = new NamedPipeTransport(writeStream);

            try
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        _output.WriteLine("After WaitForConnectAsync");
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        _output.WriteLine("After ConnectAsync");
                    }));

                await Task.WhenAll(tasks.ToArray());

                Assert.True(reader.IsConnected);

                reader.Close();

                Assert.False(reader.IsConnected);
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [Fact]
        public async Task Read_ReturnsZeroLength_WhenClosedDuringReadAsync()
        {
            var tasks = new List<Task>();
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            try
            {
                var reader = new NamedPipeTransport(readStream);
                var writer = new NamedPipeTransport(writeStream);

                var waiter = new TaskCompletionSource<string>();

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        _output.WriteLine("After WaitForConnectAsync");

                        Task.WaitAll(Task.Run(() => waiter.SetResult("go")));
                        var readBuffer = new byte[100];
                        var length = await reader.ReceiveAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);

                        Assert.Equal(0, length);
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        _output.WriteLine("After ConnectAsync");

                        var r = await waiter.Task.ConfigureAwait(false);

                        writer.Close();

                        _output.WriteLine("After Close");
                    }));

                await Task.WhenAll(tasks.ToArray());
            }
            finally
            {
                readStream.Disconnect();
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [Fact]
        public async Task Write_ReturnsZeroLength_WhenClosedDuringWriteAsync()
        {
            var tasks = new List<Task>();

            var data = new byte[100];
            for (var b = 0; b < data.Length; b++)
            {
                data[b] = (byte)b;
            }

            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            var waiter = new TaskCompletionSource<string>();

            try
            {
                var reader = new NamedPipeTransport(readStream);
                var writer = new NamedPipeTransport(writeStream);

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        _output.WriteLine("After WaitForConnectAsync");

                        reader.Close();

                        Task.WaitAll(Task.Run(() => waiter.SetResult("go")));
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        _output.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        _output.WriteLine("After ConnectAsync");

                        var r = await waiter.Task.ConfigureAwait(false);

                        var length = await writer.SendAsync(data, 0, data.Length).ConfigureAwait(false);

                        Assert.Equal(0, length);
                    }));

                await Task.WhenAll(tasks.ToArray());
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }
    }
}
