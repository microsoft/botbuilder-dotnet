// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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