// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Bot.Connector.Streaming.Transport;
using Xunit;

namespace Microsoft.Bot.Connector.Streaming.Tests.Payloads
{
    public class HeaderSerializerTests
    {
        [Fact]
        public void HeaderSerializer_CanRoundTrip()
        {
            var header = new Header
            {
                Type = PayloadTypes.Request,
                PayloadLength = 168,
                Id = Guid.Parse("68e999ca-a651-40f4-ad8f-3aaf781862b4"),
                End = true,
            };

            var buffer = new byte[1024];
            const int offset = 0;

            var length = HeaderSerializer.Serialize(header, buffer, offset);

            var result = HeaderSerializer.Deserialize(buffer, length);

            Assert.Equal(header.Type, result.Type);
            Assert.Equal(header.PayloadLength, result.PayloadLength);
            Assert.Equal(header.Id, result.Id);
            Assert.Equal(header.End, result.End);
        }

        [Fact]
        public void HeaderSerializer_SerializesToAscii()
        {
            var header = new Header
            {
                Type = PayloadTypes.Request,
                PayloadLength = 168,
                Id = Guid.Parse("68e999ca-a651-40f4-ad8f-3aaf781862b4"),
                End = true,
            };

            var buffer = new byte[1024];
            const int offset = 0;

            var length = HeaderSerializer.Serialize(header, buffer, offset);

            var str = Encoding.ASCII.GetString(buffer, offset, length);

            Assert.Equal("A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n", str);
        }

        [Fact]
        public void HeaderSerializer_DeserializesFromAscii()
        {
            const string header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            var result = HeaderSerializer.Deserialize(bytes, bytes.Length);

            Assert.Equal('A', result.Type);
            Assert.Equal(168, result.PayloadLength);
            Assert.Equal(Guid.Parse("68e999ca-a651-40f4-ad8f-3aaf781862b4"), result.Id);
            Assert.True(result.End);
        }

        [Fact]
        public void HeaderSerializer_DeserializeUnknownType()
        {
            const string header = "Z.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            var result = HeaderSerializer.Deserialize(bytes, bytes.Length);

            Assert.Equal('Z', result.Type);
            Assert.Equal(168, result.PayloadLength);
        }

        [Fact]
        public void HeaderSerializer_Deserialize_LengthTooShort_Throws()
        {
            const string header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<ArgumentException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, 5);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_LengthTooLong_Throws()
        {
            const string header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<ArgumentException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, 55);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadTypeDelimiter_Throws()
        {
            const string header = "Ax000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadLengthDelimiter_Throws()
        {
            const string header = "A.000168x68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadIdDelimiter_Throws()
        {
            const string header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4x1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadTerminator_Throws()
        {
            const string header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1c";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadLength_Throws()
        {
            const string header = "A.00p168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadId_Throws()
        {
            const string header = "A.000168.68e9p9ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void HeaderSerializer_Deserialize_BadEnd_Throws()
        {
            const string header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.z\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.Throws<InvalidDataException>(() =>
            {
                HeaderSerializer.Deserialize(bytes, bytes.Length);
            });
        }

        [Fact]
        public void Header_ClampLength_When_Value_Is_Greater_Than_Max_Throws()
        {
            Assert.Throws<InvalidDataException>(() =>
            {
                _ = new Header
                {
                    PayloadLength = TransportConstants.MaxLength + 1,
                };
            });
        }

        [Fact]
        public void Header_ClampLength_When_Value_Is_Lower_Than_Max_Throws()
        {
            Assert.Throws<InvalidDataException>(() =>
            {
                _ = new Header
                {
                    PayloadLength = TransportConstants.MinLength - 1,
                };
            });
        }
    }
}
