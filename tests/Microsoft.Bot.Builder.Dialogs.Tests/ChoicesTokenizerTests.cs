// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Choice Tests")]
    public class ChoicesTokenizerTests
    {
        [TestMethod]
        public void ShouldBreakOnSpaces()
        {
            var tokens = Tokenizer.DefaultTokenizer("how now brown cow");
            Assert.AreEqual(4, tokens.Count);
            AssertToken(tokens[0], 0, 2, "how");
            AssertToken(tokens[1], 4, 6, "now");
            AssertToken(tokens[2], 8, 12, "brown");
            AssertToken(tokens[3], 14, 16, "cow");
        }

        [TestMethod]
        public void ShouldBreakOnPunctuation()
        {
            var tokens = Tokenizer.DefaultTokenizer("how-now.brown:cow ?");
            Assert.AreEqual(4, tokens.Count);
            AssertToken(tokens[0], 0, 2, "how");
            AssertToken(tokens[1], 4, 6, "now");
            AssertToken(tokens[2], 8, 12, "brown");
            AssertToken(tokens[3], 14, 16, "cow");
        }

        [TestMethod]
        public void ShouldTokenizeSingleCharacterTokens()
        {
            var tokens = Tokenizer.DefaultTokenizer("a b c d");
            Assert.AreEqual(4, tokens.Count);
            AssertToken(tokens[0], 0, 0, "a");
            AssertToken(tokens[1], 2, 2, "b");
            AssertToken(tokens[2], 4, 4, "c");
            AssertToken(tokens[3], 6, 6, "d");
        }

        [TestMethod]
        public void ShouldReturnASingleToken()
        {
            var tokens = Tokenizer.DefaultTokenizer("food");
            Assert.AreEqual(1, tokens.Count);
            AssertToken(tokens[0], 0, 3, "food");
        }

        [TestMethod]
        public void ShouldReturnNoTokens()
        {
            var tokens = Tokenizer.DefaultTokenizer(".?; -()");
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void ShouldReturnTheNormalizedAndOriginalTextForAToken()
        {
            var tokens = Tokenizer.DefaultTokenizer("fOoD");
            Assert.AreEqual(1, tokens.Count);
            AssertToken(tokens[0], 0, 3, "fOoD", "food");
        }

        [TestMethod]
        public void ShouldBreakOnEmojis()
        {
            var tokens = Tokenizer.DefaultTokenizer("food 💥👍😀");
            Assert.AreEqual(4, tokens.Count);
            AssertToken(tokens[0], 0, 3, "food");
            AssertToken(tokens[1], 5, 6, "💥");
            AssertToken(tokens[2], 7, 8, "👍");
            AssertToken(tokens[3], 9, 10, "😀");
        }

        private static void AssertToken(Token token, int start, int end, string text, string normalized = null)
        {
            Assert.AreEqual(start, token.Start);
            Assert.AreEqual(end, token.End);
            Assert.AreEqual(text, token.Text);
            Assert.AreEqual((normalized ?? text), token.Normalized);
        }
    }
}
