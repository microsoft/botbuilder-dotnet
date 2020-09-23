// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Streaming.Payloads;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Streaming.UnitTests.Payloads
{
    public class StreamManagerTests
    {
        [Fact]
        public void StreamManager_ctor_nullCancelOk()
        {
            var m = new StreamManager(null);
            Assert.NotNull(m);
        }

        [Fact]
        public void StreamManager_GetPayloadAssembler_NotExists_Ok()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);

            Assert.NotNull(a);
            Assert.Equal(id, a.Id);
        }

        [Fact]
        public void StreamManager_GetPayloadAssembler_Exists_Ok()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);
            var a2 = m.GetPayloadAssembler(id);

            Assert.Equal(a, a2);
        }

        [Fact]
        public void StreamManager_GetPayloadStream_NotExists_Ok()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadStream(new Header { Id = id });

            Assert.NotNull(a);
        }

        [Fact]
        public void StreamManager_GetPayloadAStream_Exists_Ok()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadStream(new Header { Id = id });
            var a2 = m.GetPayloadStream(new Header { Id = id });

            Assert.Equal(a, a2);
        }

        [Fact]
        public void StreamManager_GetPayloadAStream__StreamsMatch()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);
            var s = m.GetPayloadStream(new Header { Id = id });

            Assert.Equal(a.GetPayloadAsStream(), s);
        }

        [Fact]
        public void StreamManager_OnReceive_NotExists_NoOp()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            m.OnReceive(new Header { Id = id }, null, 100);
        }

        [Fact]
        public void StreamManager_OnReceive_Exists()
        {
            var m = new StreamManager(c => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);
            var s = a.GetPayloadAsStream();

            m.OnReceive(new Header { Id = id, End = true }, s, 100);

            Assert.True(a.End);
        }

        [Fact]
        public void StreamManager_CloseStream_NotExists_NoOp()
        {
            var m = new StreamManager(c =>
            {
                throw new XunitException("Should have failed");
            });
            var id = Guid.NewGuid();

            m.CloseStream(id);
        }

        [Fact]
        public void StreamManager_CloseStream_NotEnd_Closed()
        {
            var closed = false;
            var m = new StreamManager(c =>
            {
                closed = true;
            });

            var id = Guid.NewGuid();
            var a = m.GetPayloadAssembler(id);

            m.CloseStream(id);

            Assert.True(closed);
        }

        [Fact]
        public void StreamManager_CloseStream_End_NoOp()
        {
            var closed = false;
            var m = new StreamManager(c =>
            {
                closed = true;
            });

            var id = Guid.NewGuid();
            var a = m.GetPayloadAssembler(id);
            var s = a.GetPayloadAsStream();

            // set it as ended
            a.OnReceive(new Header { End = true }, s, 1);

            m.CloseStream(id);

            Assert.False(closed);
        }
    }
}
