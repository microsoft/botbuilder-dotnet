// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.PayloadTransport;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Payloads
{
    [TestClass]
    public class PayloadSenderTests
    {
        [TestMethod]
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
            TaskCompletionSource<string> done = new TaskCompletionSource<string>();

            sender.SendPayload(header, stream, false, (Header sentHeader) =>
            {
                Assert.AreEqual(100, sentHeader.PayloadLength);
                Assert.IsFalse(sentHeader.End);
                done.SetResult("done");
                return Task.CompletedTask;
            });

            await done.Task;

            Assert.AreEqual(2, transport.Buffers.Count);
        }

        [TestMethod]
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
            TaskCompletionSource<string> done = new TaskCompletionSource<string>();

            sender.SendPayload(header, stream, false, (Header sentHeader) =>
            {
                Assert.AreEqual(0, sentHeader.PayloadLength);
                Assert.IsTrue(sentHeader.End);
                done.SetResult("done");
                return Task.CompletedTask;
            });

            await done.Task;

            Assert.AreEqual(1, transport.Buffers.Count);
        }

        [TestMethod]
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
            TaskCompletionSource<string> done = new TaskCompletionSource<string>();

            sender.SendPayload(header, stream, true, (Header sentHeader) =>
            {
                Assert.AreEqual(55, sentHeader.PayloadLength);
                Assert.IsFalse(sentHeader.End);
                done.SetResult("done");
                return Task.CompletedTask;
            });

            await done.Task;

            Assert.AreEqual(2, transport.Buffers.Count);
            Assert.AreEqual(55, stream.Position);
        }

        [TestMethod]
        public async Task HttpContentStreamDisassembler_StringContent_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var content = new ResponseMessageStream(Guid.NewGuid())
            {
                Content = new StringContent("blah blah blah", Encoding.ASCII),
            };

            TaskCompletionSource<string> done = new TaskCompletionSource<string>();

            var disassembler = new ResponseMessageStreamDisassembler(sender, content);

            await disassembler.Disassemble();

            Assert.AreEqual(2, transport.Buffers.Count);
        }

        [TestMethod]
        public async Task HttpContentStreamDisassembler_ObjectContent_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var content = new ResponseMessageStream(Guid.NewGuid())
            {
                Content = new ObjectContent(typeof(string), "{'a': 55}", new JsonMediaTypeFormatter()),
            };

            var disassembler = new ResponseMessageStreamDisassembler(sender, content);

            await disassembler.Disassemble();

            Assert.AreEqual(2, transport.Buffers.Count);
        }

        [TestMethod]
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

            await disassembler.Disassemble();

            Assert.AreEqual(3, transport.Buffers.Count);
        }

        [TestMethod]
        public async Task RequestDisassembler_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var disassembler = new RequestDisassembler(sender, Guid.NewGuid(), StreamingRequest.CreateGet("/a/b/c"));

            await disassembler.Disassemble();

            Assert.AreEqual(2, transport.Buffers.Count);
        }

        [TestMethod]
        public async Task ResponseDisassembler_SendsAsFixedLength()
        {
            var sender = new PayloadSender();
            var transport = new MockTransportSender();
            sender.Connect(transport);

            var disassembler = new ResponseDisassembler(sender, Guid.NewGuid(), StreamingResponse.OK());

            await disassembler.Disassemble();

            Assert.AreEqual(2, transport.Buffers.Count);
        }
    }
}
