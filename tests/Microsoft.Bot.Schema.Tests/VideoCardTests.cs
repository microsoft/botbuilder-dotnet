// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class VideoCardTests
    {
        [Fact]
        public void VideoCardInits()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "text";
            var image = new ThumbnailUrl("http://example.com", "example image");
            var media = new List<MediaUrl>() { new MediaUrl("http://example-media-url.com", "profile") };
            var buttons = new List<CardAction>() { new CardAction() };
            var shareable = true;
            var autoloop = true;
            var autostart = true;
            var aspect = "4:3";
            var value = new { };
            var duration = "1000";

            var videoCard = new VideoCard(title, subtitle, text, image, media, buttons, shareable, autoloop, autostart, aspect, value, duration);

            Assert.NotNull(videoCard);
            Assert.IsType<VideoCard>(videoCard);
            Assert.Equal(title, videoCard.Title);
            Assert.Equal(subtitle, videoCard.Subtitle);
            Assert.Equal(text, videoCard.Text);
            Assert.Equal(image, videoCard.Image);
            Assert.Equal(media, videoCard.Media);
            Assert.Equal(buttons, videoCard.Buttons);
            Assert.Equal(shareable, videoCard.Shareable);
            Assert.Equal(autoloop, videoCard.Autoloop);
            Assert.Equal(autostart, videoCard.Autostart);
            Assert.Equal(aspect, videoCard.Aspect);
            Assert.Equal(value, videoCard.Value);
            Assert.Equal(duration, videoCard.Duration);
        }
        
        [Fact]
        public void VideoCardInitsWithNoArgs()
        {
            var videoCard = new VideoCard();

            Assert.NotNull(videoCard);
            Assert.IsType<VideoCard>(videoCard);
        }
    }
}
