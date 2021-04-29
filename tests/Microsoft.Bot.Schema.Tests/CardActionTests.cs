// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class CardActionTests
    {
        [Fact]
        public void CardActionInits()
        {
            var type = "type";
            var title = "title";
            var image = "image";
            var text = "text";
            var displayText = "displayText";
            var value = new { };
            var channelData = new { };
            var imageAltText = "imageAltText";

            var cardAction = new CardAction(type, title, image, text, displayText, value, channelData)
            {
                ImageAltText = imageAltText
            };

            Assert.NotNull(cardAction);
            Assert.IsType<CardAction>(cardAction);
            Assert.Equal(type, cardAction.Type);
            Assert.Equal(title, cardAction.Title);
            Assert.Equal(image, cardAction.Image);
            Assert.Equal(text, cardAction.Text);
            Assert.Equal(displayText, cardAction.DisplayText);
            Assert.Equal(value, cardAction.Value);
            Assert.Equal(channelData, cardAction.ChannelData);
            Assert.Equal(imageAltText, cardAction.ImageAltText);
        }

        [Fact]
        public void CardActionInitsWithNoArgs()
        {
            var cardAction = new CardAction();

            Assert.NotNull(cardAction);
            Assert.IsType<CardAction>(cardAction);
        }

        [Fact]
        public void FromString()
        {
            var text = "mesmerize";

            var cardAction = CardAction.FromString(text);

            Assert.Equal(text, cardAction.Title);
            Assert.Equal(text, cardAction.Value);
        }
    }
}
