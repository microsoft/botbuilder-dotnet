// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class CardActionTest
    {
        [Fact]
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
                Assert.Equal("x", actions[0].Title);
                Assert.Equal("x", actions[0].Value);
                Assert.Equal("y", actions[1].Title);
                Assert.Equal("y", actions[1].Value);
                Assert.Equal("z", actions[2].Title);
                Assert.Equal("z", actions[2].Value);
            }
        }

        [Fact]
        public void FromString()
        {
            var sut = CardAction.FromString("my action");
            Assert.Equal("my action", sut.Title);
            Assert.Equal("my action", sut.Value);
        }
    }
}
