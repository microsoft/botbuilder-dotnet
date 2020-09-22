// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Choices;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [Trait("TestCategory", "Prompts")]
    [Trait("TestCategory", "Choice Tests")]
    public class ChoicesTokenizerTests
    {
        [Fact]
        public void ShouldBreakOnSpaces()
        {
            var tokens = Tokenizer.DefaultTokenizer("how now brown cow");
            Assert.Equal(4, tokens.Count);
            AssertToken(tokens[0], 0, 2, "how");
            AssertToken(tokens[1], 4, 6, "now");
            AssertToken(tokens[2], 8, 12, "brown");
            AssertToken(tokens[3], 14, 16, "cow");
        }

        [Fact]
        public void ShouldBreakOnPunctuation()
        {
            var tokens = Tokenizer.DefaultTokenizer("how-now.brown:cow ?");
            Assert.Equal(4, tokens.Count);
            AssertToken(tokens[0], 0, 2, "how");
            AssertToken(tokens[1], 4, 6, "now");
            AssertToken(tokens[2], 8, 12, "brown");
            AssertToken(tokens[3], 14, 16, "cow");
        }

        [Fact]
        public void ShouldTokenizeSingleCharacterTokens()
        {
            var tokens = Tokenizer.DefaultTokenizer("a b c d");
            Assert.Equal(4, tokens.Count);
            AssertToken(tokens[0], 0, 0, "a");
            AssertToken(tokens[1], 2, 2, "b");
            AssertToken(tokens[2], 4, 4, "c");
            AssertToken(tokens[3], 6, 6, "d");
        }

        [Fact]
        public void ShouldReturnASingleToken()
        {
            var tokens = Tokenizer.DefaultTokenizer("food");
            Assert.Single(tokens);
            AssertToken(tokens[0], 0, 3, "food");
        }

        [Fact]
        public void ShouldReturnNoTokens()
        {
            var tokens = Tokenizer.DefaultTokenizer(".?; -()");
            Assert.Empty(tokens);
        }

        [Fact]
        public void ShouldReturnTheNormalizedAndOriginalTextForAToken()
        {
            var tokens = Tokenizer.DefaultTokenizer("fOoD");
            Assert.Single(tokens);
            AssertToken(tokens[0], 0, 3, "fOoD", "food");
        }

        [Fact]
        public void ShouldBreakOnEmojis()
        {
            var tokens = Tokenizer.DefaultTokenizer("food 💥👍😀");
            Assert.Equal(4, tokens.Count);
            AssertToken(tokens[0], 0, 3, "food");
            AssertToken(tokens[1], 5, 6, "💥");
            AssertToken(tokens[2], 7, 8, "👍");
            AssertToken(tokens[3], 9, 10, "😀");
        }

        private static void AssertToken(Token token, int start, int end, string text, string normalized = null)
        {
            Assert.Equal(start, token.Start);
            Assert.Equal(end, token.End);
            Assert.Equal(text, token.Text);
            Assert.Equal(normalized ?? text, token.Normalized);
        }
    }
}
