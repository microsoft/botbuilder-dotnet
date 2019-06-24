// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Payloads
{
    [TestClass]
    public class HeaderSerializerTests
    {
        [TestMethod]
        public void HeaderSerializer_CanRoundTrip()
        {
            var header = new Header()
            {
                Type = PayloadTypes.Request,
                PayloadLength = 168,
                Id = Guid.Parse("68e999ca-a651-40f4-ad8f-3aaf781862b4"),
                End = true,
            };

            var buffer = new byte[1024];
            var offset = 0;

            var length = HeaderSerializer.Serialize(header, buffer, offset);

            var result = HeaderSerializer.Deserialize(buffer, 0, length);

            Assert.AreEqual(header.Type, result.Type);
            Assert.AreEqual(header.PayloadLength, result.PayloadLength);
            Assert.AreEqual(header.Id, result.Id);
            Assert.AreEqual(header.End, result.End);
        }

        [TestMethod]
        public void HeaderSerializer_SerializesToAscii()
        {
            var header = new Header()
            {
                Type = PayloadTypes.Request,
                PayloadLength = 168,
                Id = Guid.Parse("68e999ca-a651-40f4-ad8f-3aaf781862b4"),
                End = true,
            };

            var buffer = new byte[1024];
            var offset = 0;

            var length = HeaderSerializer.Serialize(header, buffer, offset);

            var str = Encoding.ASCII.GetString(buffer, offset, length);

            Assert.AreEqual("A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n", str);
        }

        [TestMethod]
        public void HeaderSerializer_DeserializesFromAscii()
        {
            var header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);

            Assert.AreEqual('A', result.Type);
            Assert.AreEqual(168, result.PayloadLength);
            Assert.AreEqual(Guid.Parse("68e999ca-a651-40f4-ad8f-3aaf781862b4"), result.Id);
            Assert.AreEqual(true, result.End);
        }

        [TestMethod]
        public void HeaderSerializer_DeserializeUnknownType()
        {
            var header = "Z.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);

            Assert.AreEqual('Z', result.Type);
            Assert.AreEqual(168, result.PayloadLength);
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_LengthTooShort_Throws()
        {
            var header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<ArgumentException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, 5);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_LengthTooLong_Throws()
        {
            var header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<ArgumentException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, 55);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadTypeDelimeter_Throws()
        {
            var header = "Ax000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadLengthDelimeter_Throws()
        {
            var header = "A.000168x68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadIdDelimeter_Throws()
        {
            var header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4x1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadTerminator_Throws()
        {
            var header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1c";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadLength_Throws()
        {
            var header = "A.00p168.68e999ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadId_Throws()
        {
            var header = "A.000168.68e9p9ca-a651-40f4-ad8f-3aaf781862b4.1\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }

        [TestMethod]
        public void HeaderSerializer_Deserialize_BadEnd_Throws()
        {
            var header = "A.000168.68e999ca-a651-40f4-ad8f-3aaf781862b4.z\n";
            var bytes = Encoding.ASCII.GetBytes(header);

            Assert.ThrowsException<InvalidDataException>(() =>
            {
                var result = HeaderSerializer.Deserialize(bytes, 0, bytes.Length);
            });
        }
    }
}
