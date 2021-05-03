// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class MediaTests
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

        [Fact]
        public void MediaEventValueInits()
        {
            var key = "key";
            var value = "value";
            var obj = new Dictionary<string, string>() { { key, value } };
            var mediaEventValue = new MediaEventValue(obj);

            Assert.NotNull(mediaEventValue);
            Assert.Equal(obj, mediaEventValue.CardValue);
            var cardVal = (Dictionary<string, string>)mediaEventValue.CardValue;
            Assert.Equal(value, cardVal[key]);
        }
        
        [Fact]
        public void MediaEventValueInitsWithNoArgs()
        {
            var mediaEventValue = new MediaEventValue();

            Assert.NotNull(mediaEventValue);
            Assert.IsType<MediaEventValue>(mediaEventValue);
        }

        [Fact]
        public void MediaUrlInits()
        {
            var url = "http://example.com";
            var profile = "myProfile";

            var mediaUrl = new MediaUrl(url, profile);

            Assert.NotNull(mediaUrl);
            Assert.IsType<MediaUrl>(mediaUrl);
            Assert.Equal(url, mediaUrl.Url);
            Assert.Equal(profile, mediaUrl.Profile);
        }

        [Fact]
        public void MediaUrlInitsWithNoArgs()
        {
            var mediaUrl = new MediaUrl();

            Assert.NotNull(mediaUrl);
            Assert.IsType<MediaUrl>(mediaUrl);
        }
    }
}
