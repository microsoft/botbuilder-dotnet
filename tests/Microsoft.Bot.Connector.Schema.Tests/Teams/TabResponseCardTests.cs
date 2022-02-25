// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Connector.Schema.Tests.Teams
{
    public class TabResponseCardTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("testResponseCard")]
        public void TabResponseCardInits(object card)
        {
            var responseCard = new TabResponseCard()
            {
                Card = card
            };

            Assert.NotNull(responseCard);
            Assert.IsType<TabResponseCard>(responseCard);
            Assert.Equal(card, responseCard.Card);
        }
    }
}
