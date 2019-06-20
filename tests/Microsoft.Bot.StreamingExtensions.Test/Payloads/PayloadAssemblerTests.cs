using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;
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
            var a = new MockPayloadAssembler(id);

            Assert.AreEqual(id, a.Id);
        }
        
        [TestMethod]
        public void PayloadAssembler_ctor_End_false()
        {
            var id = Guid.NewGuid();
            var a = new MockPayloadAssembler(id);

            Assert.IsFalse(a.End);
        }

        [TestMethod]
        public void PayloadAssembler_GetStream()
        {
            var id = Guid.NewGuid();
            var a = new MockPayloadAssembler(id);
            var s = a.GetPayloadStream();

            Assert.IsNotNull(a);
        }

        [TestMethod]
        public void PayloadAssembler_GetStream_DoesNotMakeNewEachTime()
        {
            var id = Guid.NewGuid();
            var a = new MockPayloadAssembler(id);
            var s = a.GetPayloadStream();
            var s2 = a.GetPayloadStream();

            Assert.AreEqual(s, s2);
        }

        [TestMethod]
        public void PayloadAssembler_OnReceive_SetsEnd()
        {
            var id = Guid.NewGuid();
            var a = new MockPayloadAssembler(id);

            var header = new Header() { End = true };

            a.OnReceive(header, null, 100);

            Assert.IsTrue(a.End);
        }

        [TestMethod]
        public void PayloadAssembler_Close__DoesNotSetEnd()
        {
            var id = Guid.NewGuid();
            var a = new MockPayloadAssembler(id);

            a.Close();

            Assert.IsFalse(a.End);
        }
    }
}
