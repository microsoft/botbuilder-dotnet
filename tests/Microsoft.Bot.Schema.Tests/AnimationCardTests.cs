// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AnimationCardTests
    {
        [Fact]
        public void SuccessfullyInitAnimationCard()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "text";
            var image = new ThumbnailUrl("http://example.com", "example image");
            var media = new List<MediaUrl>() { new MediaUrl("http://fakeMediaUrl.com", "media url profile") };
            var buttons = new List<CardAction>()
            {
                new CardAction("cardActionType", "cardActionTitle", "image", "text", "displayText", new { }, new { }),
            };
            var shareable = true;
            var autoloop = true;
            var autostart = true;
            var aspect = "aspect";
            var value = new { };
            var duration = "1000";

            var animationCard = new AnimationCard(
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

            Assert.NotNull(animationCard);
            Assert.IsType<AnimationCard>(animationCard);
            Assert.Equal(title, animationCard.Title);
            Assert.Equal(subtitle, animationCard.Subtitle);
            Assert.Equal(text, animationCard.Text);
            Assert.Equal(image, animationCard.Image);
            Assert.Equal(media, animationCard.Media);
            Assert.Equal(buttons, animationCard.Buttons);
            Assert.Equal(shareable, animationCard.Shareable);
            Assert.Equal(autoloop, animationCard.Autoloop);
            Assert.Equal(autostart, animationCard.Autostart);
            Assert.Equal(aspect, animationCard.Aspect);
            Assert.Equal(value, animationCard.Value);
            Assert.Equal(duration, animationCard.Duration);
        }
    }
}
