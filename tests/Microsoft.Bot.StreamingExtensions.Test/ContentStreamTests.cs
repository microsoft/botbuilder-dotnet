// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class ContentStreamTests
    {
        [TestMethod]
        public void ContentStream_ctor_NullAssembler_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var c = new ContentStream(Guid.Empty, null);
            });
        }

        [TestMethod]
        public void ContentStream_Id()
        {
            var id = Guid.NewGuid();
            var assembler = new PayloadStreamAssembler(null, id);
            var c = new ContentStream(id, assembler);

            Assert.AreEqual(id, c.Id);
        }

        [TestMethod]
        public void ContentStream_Type()
        {
            var id = Guid.NewGuid();
            var assembler = new PayloadStreamAssembler(null, id);
            var c = new ContentStream(id, assembler);
            var type = "foo/bar";

            c.ContentType = type;

            Assert.AreEqual(type, c.ContentType);
        }
    }
}
