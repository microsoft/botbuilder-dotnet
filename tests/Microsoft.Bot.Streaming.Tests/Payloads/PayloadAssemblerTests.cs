// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Streaming.Payloads;
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
    }
}
