// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ThumbnailTests
    {
        [Fact]
        public void ThumbnailCardInits()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "text";
            var images = new List<CardImage>() { new CardImage("http://example.com", "example image") };
            var buttons = new List<CardAction>() { new CardAction("cardActionType") };
            var tap = new CardAction("type", "title", "image", "text", "displayText", new { }, new { });

            var thumbnailCard = new ThumbnailCard(title, subtitle, text, images, buttons, tap);

            Assert.NotNull(thumbnailCard);
            Assert.IsType<ThumbnailCard>(thumbnailCard);
            Assert.Equal(title, thumbnailCard.Title);
            Assert.Equal(subtitle, thumbnailCard.Subtitle);
            Assert.Equal(text, thumbnailCard.Text);
            Assert.Equal(images, thumbnailCard.Images);
            Assert.Equal(buttons, thumbnailCard.Buttons);
            Assert.Equal(tap, thumbnailCard.Tap);
        }

        [Fact]
        public void ThumbnailCardInitsWithNoArgs()
        {
            var thumbnailCard = new ThumbnailCard();

            Assert.NotNull(thumbnailCard);
            Assert.IsType<ThumbnailCard>(thumbnailCard);
        }

        [Fact]
        public void ThumbnailUrlInits()
        {
            var url = "http://example.com";
            var alt = "example url";

            var thumbnailUrl = new ThumbnailUrl(url, alt);

            Assert.NotNull(thumbnailUrl);
            Assert.IsType<ThumbnailUrl>(thumbnailUrl);
            Assert.Equal(url, thumbnailUrl.Url);
            Assert.Equal(alt, thumbnailUrl.Alt);
        }
        
        [Fact]
        public void ThumbnailUrlInitsWithNoArgs()
        {
            var thumbnailUrl = new ThumbnailUrl();

            Assert.NotNull(thumbnailUrl);
            Assert.IsType<ThumbnailUrl>(thumbnailUrl);
        }
    }
}
