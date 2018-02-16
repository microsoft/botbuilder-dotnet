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
            var adapter = new TestBot();

            // If this compiles, the test has passed. :) 
        }

    }
}