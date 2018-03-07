using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class BotContextTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConstructorNullAdapter()
        {
            BotContext c = new BotContext(null, new Activity());
            Assert.Fail("Should Fail due to null Adapter");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConstructorNullActivity()
        {
            TestAdapter a = new TestAdapter(); 
            BotContext c = new BotContext(a, null);
            Assert.Fail("Should Fail due to null Activty");
        }
        [TestMethod]        
        public async Task Constructor()
        {            
            BotContext c = new BotContext(new TestAdapter(), new Activity());
            Assert.IsNotNull(c); 
        }
    }
}
