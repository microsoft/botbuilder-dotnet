// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class CardImageTests
    {
        [Fact]
        public void CardImageInits()
        {
            var url = "http://example.com";
            var alt = "example image";
            var tap = new CardAction("type", "title", "image", "text", "displayText", new { }, new { });

            var cardImage = new CardImage(url, alt, tap);

            Assert.NotNull(cardImage);
            Assert.IsType<CardImage>(cardImage);
            Assert.Equal(url, cardImage.Url);
            Assert.Equal(alt, cardImage.Alt);
            Assert.Equal(tap, cardImage.Tap);
        }

        [Fact]
        public void CardImageInitsWithNoArgs()
        {
            var cardImage = new CardImage();

            Assert.NotNull(cardImage);
            Assert.IsType<CardImage>(cardImage);
        }
    }
}
