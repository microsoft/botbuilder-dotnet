// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Connector.Schema.Teams;
using Xunit;
using static Microsoft.Bot.Connector.Schema.Tests.Teams.TabsTestData;

namespace Microsoft.Bot.Connector.Schema.Tests.Teams
{
    public class TabResponseCardsTests
    {
        [Theory]
        [ClassData(typeof(TabResponseCardsTestData))]
        public void TabResponseCardsInits(IList<TabResponseCard> cards)
        {
            var responseCards = new TabResponseCards()
            {
                Cards = cards
            };

            Assert.NotNull(responseCards);
            Assert.IsType<TabResponseCards>(responseCards);
            Assert.Equal(cards, responseCards.Cards);
        }
    }
}
