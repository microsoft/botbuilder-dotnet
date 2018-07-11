// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State")]
    public class CachedFrameStateTests
    {
        [TestMethod]
        public void Values_Should_Round_Trip()
        {
            var state = new CachedFrameState()
            {
                Accessed = true,
                Hash = "someHash",
                State = "This is a test",
            };

            Assert.IsTrue(state.Accessed);
            Assert.AreEqual("someHash", state.Hash);
            Assert.AreEqual("This is a test", state.State);
        }

    }
}
