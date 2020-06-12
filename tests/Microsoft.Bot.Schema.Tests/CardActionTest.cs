// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Schema.Tests
{
    [TestClass]
    public class CardActionTest
    {
        [TestMethod]
        public void TestImplicitConversation()
        {
            SuggestedActions(new CardAction[]
            {
                "x",
                "y",
                "z"
            });

            void SuggestedActions(IList<CardAction> actions)
            {
                Assert.AreEqual("x", actions[0].Title);
                Assert.AreEqual("x", actions[0].Value);
                Assert.AreEqual("y", actions[1].Title);
                Assert.AreEqual("y", actions[1].Value);
                Assert.AreEqual("z", actions[2].Title);
                Assert.AreEqual("z", actions[2].Value);
            }
        }

        [TestMethod]
        public void FromString()
        {
            var sut = CardAction.FromString("my action");
            Assert.AreEqual("my action", sut.Title);
            Assert.AreEqual("my action", sut.Value);
        }
    }
}
