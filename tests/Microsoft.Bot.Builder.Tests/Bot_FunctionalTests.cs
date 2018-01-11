using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using System;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Adapters;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Bot")]
    [TestCategory("Functional Spec")]
    public class Bot_FunctionalTests
    {
        [TestMethod]
        public async Task SingleParameterConstructor()
        {
            ActivityAdapterBase adapter = new TestAdapter();
            Bot bot = new Bot(adapter);

            // If this compiles, the test has passed. :) 
        }

        [TestMethod]
        public async Task AdapterProperty()
        {
            TestAdapter adapter = new TestAdapter();
            Bot bot = new Bot(adapter);

            ActivityAdapterBase retrievedAdapter = bot.Adapter;

            // Verify the Bot a property to allow retrieving the Adapter. 
            Assert.AreSame(adapter, retrievedAdapter);
        }
    }
}