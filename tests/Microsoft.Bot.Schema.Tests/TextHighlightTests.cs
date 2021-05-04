// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class TextHighlightTests
    {
        [Fact]
        public void TextHighlightInits()
        {
            var text = "some highlighted text";
            var occurrence = 1;

            var textHighlights = new TextHighlight(text, occurrence);

            Assert.NotNull(textHighlights);
            Assert.IsType<TextHighlight>(textHighlights);
            Assert.Equal(text, textHighlights.Text);
            Assert.Equal(occurrence, textHighlights.Occurrence);
        }
        
        [Fact]
        public void TextHighlightInitsWithNoArgs()
        {
            var textHighlights = new TextHighlight();

            Assert.NotNull(textHighlights);
            Assert.IsType<TextHighlight>(textHighlights);
        }
    }
}
