// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class CardActionTests
    {
        [Fact]
        public void TestImplicitConversation()
        {
            SuggestedActions(new CardAction[]
            {
                "x",
                "y",
                "z"
            });

            void SuggestedActions(IList<CardAction> actions)
            {
                Assert.Equal("x", actions[0].Title);
                Assert.Equal("x", actions[0].Value);
                Assert.Equal("y", actions[1].Title);
                Assert.Equal("y", actions[1].Value);
                Assert.Equal("z", actions[2].Title);
                Assert.Equal("z", actions[2].Value);
            }
        }

        [Fact]
        public void FromString()
        {
            var sut = CardAction.FromString("my action");
            Assert.Equal("my action", sut.Title);
            Assert.Equal("my action", sut.Value);
        }

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
    }
}
