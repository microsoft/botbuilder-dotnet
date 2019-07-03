// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Payloads
{
    [TestClass]
    public class StreamManagerTests
    {
        [TestMethod]
        public void StreamManager_ctor_nullCancelOk()
        {
            var m = new StreamManager(null);
            Assert.IsNotNull(m);
        }

        [TestMethod]
        public void StreamManager_GetPayloadAssembler_NotExists_Ok()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);

            Assert.IsNotNull(a);
            Assert.AreEqual(id, a.Id);
        }

        [TestMethod]
        public void StreamManager_GetPayloadAssembler_Exists_Ok()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);
            var a2 = m.GetPayloadAssembler(id);

            Assert.AreEqual(a, a2);
        }

        [TestMethod]
        public void StreamManager_GetPayloadStream_NotExists_Ok()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadStream(new Header() { Id = id });

            Assert.IsNotNull(a);
        }

        [TestMethod]
        public void StreamManager_GetPayloadAStream_Exists_Ok()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadStream(new Header() { Id = id });
            var a2 = m.GetPayloadStream(new Header() { Id = id });

            Assert.AreEqual(a, a2);
        }

        [TestMethod]
        public void StreamManager_GetPayloadAStream__StreamsMatch()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);
            var s = m.GetPayloadStream(new Header() { Id = id });

            Assert.AreEqual(a.GetPayloadAsStream(), s);
        }

        [TestMethod]
        public void StreamManager_OnReceive_NotExists_NoOp()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            m.OnReceive(new Header() { Id = id }, null, 100);
        }

        [TestMethod]
        public void StreamManager_OnReceive_Exists()
        {
            var m = new StreamManager((c) => { });
            var id = Guid.NewGuid();

            var a = m.GetPayloadAssembler(id);
            var s = a.GetPayloadAsStream();

            m.OnReceive(new Header() { Id = id, End = true }, s, 100);

            Assert.IsTrue(a.End);
        }

        [TestMethod]
        public void StreamManager_CloseStream_NotExists_NoOp()
        {
            var m = new StreamManager((c) =>
            {
                Assert.Fail();
            });
            var id = Guid.NewGuid();

            m.CloseStream(id);
        }

        [TestMethod]
        public void StreamManager_CloseStream_NotEnd_Closed()
        {
            bool closed = false;
            var m = new StreamManager((c) =>
            {
                closed = true;
            });

            var id = Guid.NewGuid();
            var a = m.GetPayloadAssembler(id);

            m.CloseStream(id);

            Assert.IsTrue(closed);
        }

        [TestMethod]
        public void StreamManager_CloseStream_End_NoOp()
        {
            bool closed = false;
            var m = new StreamManager((c) =>
            {
                closed = true;
            });

            var id = Guid.NewGuid();
            var a = m.GetPayloadAssembler(id);
            var s = a.GetPayloadAsStream();

            // set it as ended
            a.OnReceive(new Header() { End = true }, s, 1);

            m.CloseStream(id);

            Assert.IsFalse(closed);
        }
    }
}
