// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AudioCardTests
    {
        [Fact]
        public void AudioCardInit()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "text";
            var image = new ThumbnailUrl("https://example.com", "example image");
            var media = new List<MediaUrl>() { new MediaUrl("http://exampleMedia.com", "profile") };
            var buttons = new List<CardAction>() { new CardAction("type", "title", "image", "text", "displayText", new { }, new { }) };
            var shareable = true;
            var autoloop = true;
            var autostart = true;
            var aspect = "aspect";
            var value = new { };
            var duration = "duration";

            var audioCard = new AudioCard(
                title,
                subtitle,
                text,
                image,
                media,
                buttons,
                shareable,
                autoloop,
                autostart,
                aspect,
                value,
                duration);

            Assert.NotNull(audioCard);
            Assert.IsType<AudioCard>(audioCard);
            Assert.Equal(title, audioCard.Title);
            Assert.Equal(subtitle, audioCard.Subtitle);
            Assert.Equal(text, audioCard.Text);
            Assert.Equal(image, audioCard.Image);
            Assert.Equal(media, audioCard.Media);
            Assert.Equal(buttons, audioCard.Buttons);
            Assert.Equal(shareable, audioCard.Shareable);
            Assert.Equal(autoloop, audioCard.Autoloop);
            Assert.Equal(autostart, audioCard.Autostart);
            Assert.Equal(aspect, audioCard.Aspect);
            Assert.Equal(value, audioCard.Value);
            Assert.Equal(duration, audioCard.Duration);
        }

        [Fact]
        public void AudioCardInitsWithNoArgs()
        {
            var audioCard = new AudioCard();

            Assert.NotNull(audioCard);
            Assert.IsType<AudioCard>(audioCard);
        }
    }
}
