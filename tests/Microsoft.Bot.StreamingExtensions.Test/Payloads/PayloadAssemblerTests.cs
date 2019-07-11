// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Payloads
{
    [TestClass]
    public class PayloadAssemblerTests
    {
        [TestMethod]
        public void PayloadAssembler_ctor_Id()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            Assert.AreEqual(id, a.Id);
        }

        [TestMethod]
        public void PayloadAssembler_ctor_End_false()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            Assert.IsFalse(a.End);
        }

        [TestMethod]
        public void PayloadAssembler_GetStream()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);
            var s = a.GetPayloadAsStream();

            Assert.IsNotNull(a);
        }

        [TestMethod]
        public void PayloadAssembler_GetStream_DoesNotMakeNewEachTime()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);
            var s = a.GetPayloadAsStream();
            var s2 = a.GetPayloadAsStream();

            Assert.AreEqual(s, s2);
        }

        [TestMethod]
        public void PayloadAssembler_OnReceive_SetsEnd()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            var header = new Header() { End = true };

            a.OnReceive(header, new PayloadStream(a), 100);

            Assert.IsTrue(a.End);
        }

        [TestMethod]
        public void PayloadAssembler_Close__DoesNotSetEnd()
        {
            var id = Guid.NewGuid();
            var a = new PayloadStreamAssembler(new StreamManager(), id);

            a.Close();

            Assert.IsFalse(a.End);
        }
    }
}
