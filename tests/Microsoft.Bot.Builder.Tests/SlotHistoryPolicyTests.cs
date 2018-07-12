// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State")]
    [TestCategory("POCO")]
    public class SlotHistoryPolicyTests
    {
        [TestMethod]
        public void SetAndTest()
        {
            const int maxCount = 42;
            const int expiresSeconds = 4242;

            var slotHistoryPolicy = new SlotHistoryPolicy()
            {
                MaxCount = maxCount,
                ExpiresAfterSeconds = expiresSeconds,
            };

            Assert.AreEqual(maxCount, slotHistoryPolicy.MaxCount);
            Assert.AreEqual(expiresSeconds, slotHistoryPolicy.ExpiresAfterSeconds); 
        }
    }
}
