// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming.Payloads;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests.Payloads
{
    public class PayloadAssemblerTests
    {
        [Fact]
        public void PayloadAssembler_ctor_Id()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            Assert.Equal(id, a.Id);
        }

        [Fact]
        public void PayloadAssembler_ctor_ContentType()
        {
            var id = Guid.NewGuid();
            var assembler = new PayloadStreamAssembler(new StreamManager(), id);
            const string contentType = "content-type";

            assembler.ContentType = contentType;

            Assert.Equal(id, assembler.Id);
            Assert.Equal(contentType, assembler.ContentType);
        }

        [Fact]
        public void PayloadAssembler_ctor_End_false()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            Assert.False(a.End);
        }

        [Fact]
        public void PayloadAssembler_GetStream()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);
            var s = a.GetPayloadAsStream();

            Assert.NotNull(s);
        }

        [Fact]
        public void PayloadAssembler_GetStream_DoesNotMakeNewEachTime()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);
            var s = a.GetPayloadAsStream();
            var s2 = a.GetPayloadAsStream();

            Assert.Equal(s, s2);
        }

        [Fact]
        public void PayloadAssembler_OnReceive_SetsEnd()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            var header = new Header { End = true };

            a.OnReceive(header, new PayloadStream(a), 100);

            Assert.True(a.End);
        }

        [Fact]
        public void PayloadAssembler_Close__DoesNotSetEnd()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            a.Close();

            Assert.False(a.End);
        }

        [Fact]
        public void PayloadAssemblerManager_GetPayloadStream_Request_Returns_Stream()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, Type = PayloadTypes.Request };

            static Task OnReceiveRequest(Guid id, ReceiveRequest req) => Task.CompletedTask;
            static Task OnReceiveResponse(Guid id, ReceiveResponse res) => Task.CompletedTask;

            var assembler = new PayloadAssemblerManager(streamManager, OnReceiveRequest, OnReceiveResponse);
            var stream = assembler.GetPayloadStream(header);

            Assert.NotNull(stream);
        }

        [Fact]
        public void PayloadAssemblerManager_GetPayloadStream_Response_Returns_Stream()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, Type = PayloadTypes.Response };

            static Task OnReceiveRequest(Guid id, ReceiveRequest req) => Task.CompletedTask;
            static Task OnReceiveResponse(Guid id, ReceiveResponse res) => Task.CompletedTask;

            var assembler = new PayloadAssemblerManager(streamManager, OnReceiveRequest, OnReceiveResponse);
            var stream = assembler.GetPayloadStream(header);

            Assert.NotNull(stream);
        }

        [Fact]
        public void PayloadAssemblerManager_GetPayloadStream_With_No_Header_Type_Returns_Null()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id };

            static Task OnReceiveRequest(Guid id, ReceiveRequest req) => Task.CompletedTask;
            static Task OnReceiveResponse(Guid id, ReceiveResponse res) => Task.CompletedTask;

            var assembler = new PayloadAssemblerManager(streamManager, OnReceiveRequest, OnReceiveResponse);
            var stream = assembler.GetPayloadStream(header);

            Assert.Null(stream);
        }

        [Fact]
        public void PayloadAssemblerManager_GetPayloadStream_With_Active_Assembler_Returns_Null()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, Type = PayloadTypes.Request };

            static Task OnReceiveRequest(Guid id, ReceiveRequest req) => Task.CompletedTask;
            static Task OnReceiveResponse(Guid id, ReceiveResponse res) => Task.CompletedTask;

            var assembler = new PayloadAssemblerManager(streamManager, OnReceiveRequest, OnReceiveResponse);
            var stream = assembler.GetPayloadStream(header);
            stream = assembler.GetPayloadStream(header);

            Assert.Null(stream);
        }

        [Fact]
        public async Task PayloadAssemblerManager_OnReceiveRequest()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();
            var done = new TaskCompletionSource<string>();

            var headerGetPayloadStream = new Header { Id = id, Type = PayloadTypes.Request };
            var headerOnReceive = new Header { Id = id, Type = PayloadTypes.Request, End = true };

            Task OnReceiveRequest(Guid guid, ReceiveRequest req)
            {
                Assert.Equal(id, guid);
                Assert.NotEmpty(req.Streams);

                done.SetResult("done");
                return Task.CompletedTask;
            }

            static Task OnReceiveResponse(Guid id, ReceiveResponse res) => Task.CompletedTask;

            var assembler = new PayloadAssemblerManager(streamManager, OnReceiveRequest, OnReceiveResponse);

            var payload = new RequestPayload
            {
                Verb = "verb",
                Path = "path",
                Streams = new List<StreamDescription>
                {
                    new StreamDescription { Id = id.ToString(), ContentType = "content-type", Length = 3 }
                }
            };
            var payloadStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, SerializationSettings.DefaultDeserializationSettings)));
            
            assembler.GetPayloadStream(headerGetPayloadStream);
            assembler.OnReceive(headerOnReceive, payloadStream, 3);

            await done.Task;
        }

        [Fact]
        public async Task PayloadAssemblerManager_OnReceiveResponse()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();
            var done = new TaskCompletionSource<string>();

            var headerGetPayloadStream = new Header { Id = id, Type = PayloadTypes.Response };
            var headerOnReceive = new Header { Id = id, Type = PayloadTypes.Response, End = true };

            static Task OnReceiveRequest(Guid guid, ReceiveRequest req) => Task.CompletedTask;

            Task OnReceiveResponse(Guid guid, ReceiveResponse res)
            {
                Assert.Equal(id, guid);
                Assert.NotEmpty(res.Streams);

                done.SetResult("done");
                return Task.CompletedTask;
            }

            var assembler = new PayloadAssemblerManager(streamManager, OnReceiveRequest, OnReceiveResponse);

            var payload = new ResponsePayload
            {
                StatusCode = 3,
                Streams = new List<StreamDescription>
                {
                    new StreamDescription { Id = id.ToString(), ContentType = "content-type", Length = 3 }
                }
            };
            var payloadStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, SerializationSettings.DefaultDeserializationSettings)));

            assembler.GetPayloadStream(headerGetPayloadStream);
            assembler.OnReceive(headerOnReceive, payloadStream, 3);

            await done.Task;
        }

        [Fact]
        public void ReceiveRequestAssembler_ctor_With_Header_Null_Should_Fail()
        {
            var streamManager = new StreamManager();

            static Task OnCompleted(Guid id, ReceiveRequest req) => Task.CompletedTask;

            Assert.Throws<ArgumentNullException>(() => new ReceiveRequestAssembler(null, streamManager, OnCompleted));
        }

        [Fact]
        public void ReceiveRequestAssembler_ctor_With_OnCompleted_Null_Should_Fail()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, Type = PayloadTypes.Request };

            Assert.Throws<ArgumentNullException>(() => new ReceiveRequestAssembler(header, streamManager, null));
        }

        [Fact]
        public void ReceiveRequestAssembler_ctor()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, End = true };

            static Task OnCompleted(Guid id, ReceiveRequest req) => Task.CompletedTask;

            var assembler = new ReceiveRequestAssembler(header, streamManager, OnCompleted);

            Assert.Equal(id, assembler.Id);
            Assert.False(assembler.End);
        }

        [Fact]
        public void ReceiveRequestAssembler_CreateStreamFromPayload_With_PayloadLength()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, End = true, PayloadLength = 3 };

            static Task OnCompleted(Guid id, ReceiveRequest req) => Task.CompletedTask;

            var assembler = new ReceiveRequestAssembler(header, streamManager, OnCompleted);
            var stream = (MemoryStream)assembler.CreateStreamFromPayload();

            Assert.NotNull(stream);
            Assert.Equal(header.PayloadLength, stream.Capacity);
        }

        [Fact]
        public async Task ReceiveRequestAssembler_Close()
        {
            var id = Guid.NewGuid();
            var header = new Header { Id = id, End = true };
            var done = new TaskCompletionSource<string>();

            void OnCancelStream(PayloadStreamAssembler streamAssembler)
            {
                Assert.Equal(id, streamAssembler.Id);
                done.SetResult("done");
            }

            var streamManager = new StreamManager(OnCancelStream);
            streamManager.GetPayloadAssembler(id);

            static Task OnCompleted(Guid id, ReceiveRequest req) => Task.CompletedTask;

            var assembler = new ReceiveRequestAssembler(header, streamManager, OnCompleted);
            assembler.Close();

            await done.Task;
        }

        [Fact]
        public void ReceiveResponseAssembler_ctor_With_Header_Null_Should_Fail()
        {
            var streamManager = new StreamManager();

            static Task OnCompleted(Guid id, ReceiveResponse res) => Task.CompletedTask;

            Assert.Throws<ArgumentNullException>(() => new ReceiveResponseAssembler(null, streamManager, OnCompleted));
        }

        [Fact]
        public void ReceiveResponseAssembler_ctor_With_OnCompleted_Null_Should_Fail()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, Type = PayloadTypes.Response };

            Assert.Throws<ArgumentNullException>(() => new ReceiveResponseAssembler(header, streamManager, null));
        }

        [Fact]
        public void ReceiveResponseAssembler_ctor()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, End = true };

            static Task OnCompleted(Guid id, ReceiveResponse req) => Task.CompletedTask;

            var assembler = new ReceiveResponseAssembler(header, streamManager, OnCompleted);

            Assert.Equal(id, assembler.Id);
            Assert.False(assembler.End);
        }

        [Fact]
        public void ReceiveResponseAssembler_CreateStreamFromPayload_With_PayloadLength()
        {
            var id = Guid.NewGuid();
            var streamManager = new StreamManager();

            var header = new Header { Id = id, End = true, PayloadLength = 3 };

            static Task OnCompleted(Guid id, ReceiveResponse req) => Task.CompletedTask;

            var assembler = new ReceiveResponseAssembler(header, streamManager, OnCompleted);
            var stream = (MemoryStream)assembler.CreateStreamFromPayload();

            Assert.NotNull(stream);
            Assert.Equal(header.PayloadLength, stream.Capacity);
        }

        [Fact]
        public async Task ReceiveResponseAssembler_Close()
        {
            var id = Guid.NewGuid();
            var header = new Header { Id = id, End = true };
            var done = new TaskCompletionSource<string>();

            void OnCancelStream(PayloadStreamAssembler streamAssembler)
            {
                Assert.Equal(id, streamAssembler.Id);
                done.SetResult("done");
            }

            var streamManager = new StreamManager(OnCancelStream);
            streamManager.GetPayloadAssembler(id);

            static Task OnCompleted(Guid id, ReceiveResponse req) => Task.CompletedTask;

            var assembler = new ReceiveResponseAssembler(header, streamManager, OnCompleted);
            assembler.Close();

            await done.Task;
        }
    }
}
