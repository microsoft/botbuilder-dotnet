// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardTests
    {
        [Fact]
        public void O365ConnectorCardInits()
        {
            var title = "Grand Title";
            var text = "A grand adventure awaits!";
            var summary = "A card that details harrowing tails of our grand adventurer";
            var themeColor = "0078D7";
            var sections = new List<O365ConnectorCardSection>() { new O365ConnectorCardSection("David Claux") };
            var potentialAction = new List<O365ConnectorCardActionBase>() { new O365ConnectorCardActionBase("ActionCard") };

            var o365ConnectorCard = new O365ConnectorCard(title, text, summary, themeColor, sections, potentialAction);

            Assert.NotNull(o365ConnectorCard);
            Assert.IsType<O365ConnectorCard>(o365ConnectorCard);
            Assert.Equal(title, o365ConnectorCard.Title);
            Assert.Equal(text, o365ConnectorCard.Text);
            Assert.Equal(summary, o365ConnectorCard.Summary);
            Assert.Equal(themeColor, o365ConnectorCard.ThemeColor);
            Assert.Equal(sections, o365ConnectorCard.Sections);
            Assert.Equal(potentialAction, o365ConnectorCard.PotentialAction);
        }
        
        [Fact]
        public void O365ConnectorCardInitsWithNoArgs()
        {
            var o365ConnectorCard = new O365ConnectorCard();

            Assert.NotNull(o365ConnectorCard);
            Assert.IsType<O365ConnectorCard>(o365ConnectorCard);
        }
    }
}
