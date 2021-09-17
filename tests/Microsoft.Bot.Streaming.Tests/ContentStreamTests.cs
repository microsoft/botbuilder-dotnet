// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Streaming.Payloads;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class ContentStreamTests
    {
        [Fact]
        public void ContentStream_ctor_NullAssembler_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var c = new ContentStream(Guid.Empty, null);
            });
        }

        [Fact]
        public void ContentStream_Id()
        {
            var id = Guid.NewGuid();
            var assembler = new PayloadStreamAssembler(null, id);
            var c = new ContentStream(id, assembler);
            var stream = assembler.GetPayloadAsStream();

            Assert.Equal(id, c.Id);
            Assert.Equal(stream, c.Stream);

            c.Cancel();
        }

        [Fact]
        public void ContentStream_Type()
        {
            var id = Guid.NewGuid();
            var assembler = new PayloadStreamAssembler(null, id);
            var c = new ContentStream(id, assembler);
            var type = "foo/bar";

            c.ContentType = type;

            Assert.Equal(type, c.ContentType);

            c.Cancel();
        }

        [Fact]
        public void ContentStream_Length()
        {
            var id = Guid.NewGuid();
            var assembler = new PayloadStreamAssembler(null, id);
            var content = new ContentStream(id, assembler);
            var length = 3;

            content.Length = 3;

            Assert.Equal(length, content.Length);

            content.Cancel();
        }
    }
}
