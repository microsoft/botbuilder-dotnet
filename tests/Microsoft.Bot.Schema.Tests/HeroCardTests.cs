// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class HeroCardTests
    {
        [Fact]
        public void HeroCardInits()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "Why yes longan fruit is delicious!";
            var images = new List<CardImage>() { new CardImage("http://longan.com", "longan bunch", new CardAction()) };
            var buttons = new List<CardAction>() { new CardAction() };
            var tap = new CardAction();

            var heroCard = new HeroCard(title, subtitle, text, images, buttons, tap);

            Assert.NotNull(heroCard);
            Assert.IsType<HeroCard>(heroCard);
            Assert.Equal(title, heroCard.Title);
            Assert.Equal(subtitle, heroCard.Subtitle);
            Assert.Equal(text, heroCard.Text);
            Assert.Equal(images, heroCard.Images);
            Assert.Equal(buttons, heroCard.Buttons);
            Assert.Equal(tap, heroCard.Tap);
        }

        [Fact]
        public void HeroCardInitsWithNoArgs()
        {
            var heroCard = new HeroCard();

            Assert.NotNull(heroCard);
            Assert.IsType<HeroCard>(heroCard);
        }
    }
}
