using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class ContentStreamTests
    {
        [TestMethod]
        public void ContentStream_ctor_NullAssembler_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => {
                var c = new ContentStream(Guid.Empty, null);
            });
        }

        [TestMethod]
        public void ContentStream_Id()
        {
            var id = Guid.NewGuid();
            var assembler = new ContentStreamAssembler(null, id);
            var c = new ContentStream(id, assembler);

            Assert.AreEqual(id, c.Id);
        }

        [TestMethod]
        public void ContentStream_Type()
        {
            var id = Guid.NewGuid();
            var assembler = new ContentStreamAssembler(null, id);
            var c = new ContentStream(id, assembler);
            var type = "foo/bar";

            c.Type = type;

            Assert.AreEqual(type, c.Type);
        }
    }
}
