// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests.Payloads
{
    public class PayloadSenderTests
    {
        [Fact]
        public async Task PayloadSender_WhenLengthNotSet_Sends()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var header = new Header()
            {
                Type = PayloadTypes.Stream,
                Id = Guid.NewGuid(),
                PayloadLength = 0,
                End = false,
            };

            var stream = new MemoryStream(new byte[100]);
            var done = new TaskCompletionSource<string>();

            sender.SendPayload(header, stream, false, (Header sentHeader) =>
            {
                Assert.Equal(100, sentHeader.PayloadLength);
                Assert.False(sentHeader.End);
                done.SetResult("done");
                return Task.CompletedTask;
            });

            await done.Task;

            Assert.Equal(2, transport.Buffers.Count);
        }

        [Fact]
        public async Task PayloadSender_WhenLengthNotSet_AndNoData_SendsZeroLengthEnd()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var header = new Header()
            {
                Type = PayloadTypes.Stream,
                Id = Guid.NewGuid(),
                PayloadLength = 555,
                End = false,
            };

            var stream = new MemoryStream(new byte[100]);
            stream.Position = 100;
            var done = new TaskCompletionSource<string>();

            sender.SendPayload(header, stream, false, (Header sentHeader) =>
            {
                Assert.Equal(0, sentHeader.PayloadLength);
                Assert.True(sentHeader.End);
                done.SetResult("done");
                return Task.CompletedTask;
            });

            await done.Task;

            Assert.Single(transport.Buffers);
        }

        [Fact]
        public async Task PayloadSender_WhenLengthSet_Sends()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var header = new Header()
            {
                Type = PayloadTypes.Stream,
                Id = Guid.NewGuid(),
                PayloadLength = 55,
                End = false,
            };

            var stream = new MemoryStream(new byte[100]);
            var done = new TaskCompletionSource<string>();

            sender.SendPayload(header, stream, true, (Header sentHeader) =>
            {
                Assert.Equal(55, sentHeader.PayloadLength);
                Assert.False(sentHeader.End);
                done.SetResult("done");
                return Task.CompletedTask;
            });

            await done.Task;

            Assert.Equal(2, transport.Buffers.Count);
            Assert.Equal(55, stream.Position);
        }

        [Fact]
        public void PayloadSender_Connect_ShouldFail()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            Assert.Throws<InvalidOperationException>(() => sender.Connect(transport));
        }

        [Fact]
        public void PayloadSender_Dispose()
        {
            var sender = new PayloadSender();

            var header = new Header()
            {
                Type = PayloadTypes.Stream,
                Id = Guid.NewGuid(),
                PayloadLength = 55,
                End = false,
            };

            var stream = new MemoryStream(new byte[100]);

            sender.Dispose();

            Assert.Throws<ObjectDisposedException>(() => sender.SendPayload(header, stream, true, (Header sentHeader) => Task.CompletedTask));
        }

        [Fact]
        public async Task HttpContentStreamDisassembler_StringContent_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var content = new ResponseMessageStream(Guid.NewGuid())
            {
                Content = new StringContent("blah blah blah", Encoding.ASCII),
            };

            var disassembler = new ResponseMessageStreamDisassembler(sender, content);

            await disassembler.DisassembleAsync();

            Assert.Equal(2, transport.Buffers.Count);
        }

        [Fact]
        public async Task HttpContentStreamDisassembler_ObjectContent_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var content = new ResponseMessageStream(Guid.NewGuid())
            {
                Content = new StringContent("{'a': 55}", Encoding.UTF8, "application/json"),
            };

            var disassembler = new ResponseMessageStreamDisassembler(sender, content);

            await disassembler.DisassembleAsync();

            Assert.Equal(2, transport.Buffers.Count);
        }

        [Fact]
        public async Task HttpContentStreamDisassembler_StreamContent_SendsAsVariableLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var stream = new PayloadStream(new PayloadStreamAssembler(null, Guid.NewGuid(), "blah", 100));

            var content = new ResponseMessageStream(Guid.NewGuid())
            {
                Content = new StreamContent(stream),
            };

            stream.Write(new byte[100], 0, 100);

            var disassembler = new ResponseMessageStreamDisassembler(sender, content);

            await disassembler.DisassembleAsync();

            Assert.Equal(3, transport.Buffers.Count);
        }

        [Fact]
        public async Task RequestDisassembler_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var disassembler = new RequestDisassembler(sender, Guid.NewGuid(), StreamingRequest.CreateGet("/a/b/c"));

            await disassembler.DisassembleAsync();

            Assert.Equal(2, transport.Buffers.Count);
        }

        [Fact]
        public async Task ResponseDisassembler_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var disassembler = new ResponseDisassembler(sender, Guid.NewGuid(), StreamingResponse.OK());

            await disassembler.DisassembleAsync();

            Assert.Equal(2, transport.Buffers.Count);
        }

        [Fact]
        public async Task ResponseDisassembler_With_HttpContent_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var content = new StringContent("{'a': 55}", Encoding.UTF8, "application/json");

            var response = StreamingResponse.CreateResponse(System.Net.HttpStatusCode.OK, content);

            var disassembler = new ResponseDisassembler(sender, Guid.NewGuid(), response);

            await disassembler.DisassembleAsync();

            Assert.Equal(2, transport.Buffers.Count);
        }
    }
}
