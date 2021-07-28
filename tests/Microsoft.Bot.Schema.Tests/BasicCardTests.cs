// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class BasicCardTests
    {
        [Fact]
        public void BasicCardInits()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "text";
            var cardAction = new CardAction("type", "title", "image", "text", "displayText", new { }, new { });
            var images = new List<CardImage>() { new CardImage("http://example.com", "example image", cardAction) };
            var buttons = new List<CardAction>() { cardAction };
            var tap = cardAction;

            var basicCard = new BasicCard(title, subtitle, text, images, buttons, tap);

            Assert.NotNull(basicCard);
            Assert.IsType<BasicCard>(basicCard);
            Assert.Equal(title, basicCard.Title);
            Assert.Equal(subtitle, basicCard.Subtitle);
            Assert.Equal(text, basicCard.Text);
            Assert.Equal(images, basicCard.Images);
            Assert.Equal(buttons, basicCard.Buttons);
            Assert.Equal(tap, basicCard.Tap);
        }

        [Fact]
        public void BasicCardInitsWithNoArgs()
        {
            var basicCard = new BasicCard();

            Assert.NotNull(basicCard);
            Assert.IsType<BasicCard>(basicCard);
        }
    }
}
