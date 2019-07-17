// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Transport.NamedPipes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class NamedPipeTransportTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanWriteAndRead()
        {
            var tasks = new List<Task>();

            byte[] data = new byte[100];
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
                        TestContext.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        TestContext.WriteLine("After WaitForConnectAsync");

                        var readBuffer = new byte[data.Length];
                        var length = await reader.ReceiveAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);

                        TestContext.WriteLine("After Read");
                        Assert.AreEqual(length, data.Length);
                        for (var b = 0; b < data.Length; b++)
                        {
                            Assert.AreEqual(readBuffer[b], data[b]);
                        }
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        TestContext.WriteLine("After ConnectAsync");
                        await writer.SendAsync(data, 0, data.Length).ConfigureAwait(false);
                        TestContext.WriteLine("After Write");
                    }));

                Task.WaitAll(tasks.ToArray());
            }
            finally
            {
                readStream.Disconnect();
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [TestMethod]
        public void ClosedStream_Not_IsConnected()
        {
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var reader = new NamedPipeTransport(readStream);

            Assert.AreEqual(false, reader.IsConnected);
            readStream.Dispose();
        }

        [TestMethod]
        public void ActiveStream_IsConnected()
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
                        TestContext.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        TestContext.WriteLine("After WaitForConnectAsync");
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        TestContext.WriteLine("After ConnectAsync");
                    }));

                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(true, reader.IsConnected);
                Assert.AreEqual(true, writer.IsConnected);
            }
            finally
            {
                readStream.Disconnect();
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [TestMethod]
        public void Dispose_DisconnectsStream()
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
                        TestContext.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        TestContext.WriteLine("After WaitForConnectAsync");
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        TestContext.WriteLine("After ConnectAsync");
                    }));

                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(true, reader.IsConnected);

                reader.Dispose();

                Assert.AreEqual(false, reader.IsConnected);
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [TestMethod]
        public void Close_DisconnectsStream()
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
                        TestContext.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        TestContext.WriteLine("After WaitForConnectAsync");
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        TestContext.WriteLine("After ConnectAsync");
                    }));

                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(true, reader.IsConnected);

                reader.Close();

                Assert.AreEqual(false, reader.IsConnected);
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [TestMethod]
        public void Read_ReturnsZeroLength_WhenClosedDuringRead()
        {
            var tasks = new List<Task>();
            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            try
            {
                var reader = new NamedPipeTransport(readStream);
                var writer = new NamedPipeTransport(writeStream);

                TaskCompletionSource<string> waiter = new TaskCompletionSource<string>();

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        TestContext.WriteLine("After WaitForConnectAsync");

                        Task.WaitAll(Task.Run(() => waiter.SetResult("go")));
                        var readBuffer = new byte[100];
                        var length = await reader.ReceiveAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);

                        Assert.AreEqual(0, length);
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        TestContext.WriteLine("After ConnectAsync");

                        var r = await waiter.Task.ConfigureAwait(false);

                        writer.Close();

                        TestContext.WriteLine("After Close");
                    }));

                Task.WaitAll(tasks.ToArray());
            }
            finally
            {
                readStream.Disconnect();
                readStream.Dispose();
                writeStream.Dispose();
            }
        }

        [TestMethod]
        public void Write_ReturnsZeroLength_WhenClosedDuringWrite()
        {
            var tasks = new List<Task>();

            byte[] data = new byte[100];
            for (var b = 0; b < data.Length; b++)
            {
                data[b] = (byte)b;
            }

            var pipeName = Guid.NewGuid().ToString();
            var readStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
            var writeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            TaskCompletionSource<string> waiter = new TaskCompletionSource<string>();

            try
            {
                var reader = new NamedPipeTransport(readStream);
                var writer = new NamedPipeTransport(writeStream);

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before WaitForConnectAsync");
                        await readStream.WaitForConnectionAsync().ConfigureAwait(false);
                        TestContext.WriteLine("After WaitForConnectAsync");

                        reader.Close();

                        Task.WaitAll(Task.Run(() => waiter.SetResult("go")));
                    }));

                tasks.Add(Task.Run(
                    async () =>
                    {
                        TestContext.WriteLine("Before ConnectAsync");
                        await writeStream.ConnectAsync(500).ConfigureAwait(false);
                        TestContext.WriteLine("After ConnectAsync");

                        var r = await waiter.Task.ConfigureAwait(false);

                        var length = await writer.SendAsync(data, 0, data.Length).ConfigureAwait(false);

                        Assert.AreEqual(0, length);
                    }));

                Task.WaitAll(tasks.ToArray());
            }
            finally
            {
                readStream.Dispose();
                writeStream.Dispose();
            }
        }
    }
}
