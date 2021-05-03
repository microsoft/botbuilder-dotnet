// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class MediaCardTests
    {
        [Fact]
        public void MediaCardInits()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "I am a media card";
            var image = new ThumbnailUrl("http://example.com", "media card image");
            var media = new List<MediaUrl>() { new MediaUrl("http://anotherExample.com", "profile") };
            var buttons = new List<CardAction>() { new CardAction("action1"), new CardAction("action2") };
            var shareable = true;
            var autoloop = true;
            var autostart = true;
            var aspect = "4:3";
            var value = new { };
            var duration = "1000";

            var mediaCard = new MediaCard(title, subtitle, text, image, media, buttons, shareable, autoloop, autostart, aspect, value, duration);

            Assert.NotNull(mediaCard);
            Assert.IsType<MediaCard>(mediaCard);
            Assert.Equal(title, mediaCard.Title);
            Assert.Equal(subtitle, mediaCard.Subtitle);
            Assert.Equal(text, mediaCard.Text);
            Assert.Equal(image, mediaCard.Image);
            Assert.Equal(media, mediaCard.Media);
            Assert.Equal(buttons, mediaCard.Buttons);
            Assert.Equal(shareable, mediaCard.Shareable);
            Assert.Equal(autoloop, mediaCard.Autoloop);
            Assert.Equal(autostart, mediaCard.Autostart);
            Assert.Equal(aspect, mediaCard.Aspect);
            Assert.Equal(value, mediaCard.Value);
            Assert.Equal(duration, mediaCard.Duration);
        }

        [Fact]
        public void MediaCardInitsWithNoArgs()
        {
            var mediaCard = new MediaCard();

            Assert.NotNull(mediaCard);
            Assert.IsType<MediaCard>(mediaCard);
        }
    }
}
