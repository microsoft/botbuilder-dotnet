// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class O365ConnectorCardSectionTests
    {
        [Fact]
        public void O365ConnectorCardSectionInits()
        {
            var title = "Donut Selection";
            var text = "Choose a Donut";
            var activityTitle = "Donut";
            var activitySubtitle = "Select an image below.";
            var activityText = "Select the image of the donut you wish to order.";
            var activityImage = "https://example-of-donut.com";
            var activityImageType = "article";
            var markdown = false;
            var facts = new List<O365ConnectorCardFact>()
            {
                new O365ConnectorCardFact("jelly"),
                new O365ConnectorCardFact("powdered"),
            };
            var images = new List<O365ConnectorCardImage>()
            { 
                new O365ConnectorCardImage("https://jelly.com"),
                new O365ConnectorCardImage("https://powdered.com") 
            };
            var potentialAction = new List<O365ConnectorCardActionBase>() { new O365ConnectorCardActionBase("OpenUri") };

            var section = new O365ConnectorCardSection(title, text, activityTitle, activitySubtitle, activityText, activityImage, activityImageType, markdown, facts, images, potentialAction);

            Assert.NotNull(section);
            Assert.IsType<O365ConnectorCardSection>(section);
            Assert.Equal(title, section.Title);
            Assert.Equal(text, section.Text);
            Assert.Equal(activityTitle, section.ActivityTitle);
            Assert.Equal(activitySubtitle, section.ActivitySubtitle);
            Assert.Equal(activityText, section.ActivityText);
            Assert.Equal(activityImage, section.ActivityImage);
            Assert.Equal(activityImageType, section.ActivityImageType);
            Assert.Equal(markdown, section.Markdown);
            Assert.Equal(facts, section.Facts);
            Assert.Equal(2, section.Facts.Count);
            Assert.Equal(images, section.Images);
            Assert.Equal(2, section.Images.Count);
            Assert.Equal(potentialAction, section.PotentialAction);
            Assert.Equal(1, section.PotentialAction.Count);
        }
        
        [Fact]
        public void O365ConnectorCardSectionInitsWithNoArgs()
        {
            var section = new O365ConnectorCardSection();

            Assert.NotNull(section);
            Assert.IsType<O365ConnectorCardSection>(section);
        }
    }
}
