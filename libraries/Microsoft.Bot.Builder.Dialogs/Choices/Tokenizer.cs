// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Represents a callback method that can break a string into its component tokens.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="locale">Optional, identifies the locale of the input text.</param>
    /// <returns>The list of the found <see cref="Token"/> objects.</returns>
    public delegate List<Token> TokenizerFunction(string text, string locale = null);

    /// <summary>
    /// Provides a default tokenizer implementation.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class Tokenizer
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        /// <summary>
        /// Gets the default <see cref="TokenizerFunction"/> implementation.
        /// </summary>
        /// <value>The default <see cref="TokenizerFunction"/> implementation.</value>
        public static TokenizerFunction DefaultTokenizer => DefaultTokenizerImpl;

        /// <summary>
        /// Simple tokenizer that breaks on spaces and punctuation. The only normalization done is to lowercase.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="locale">Optional, identifies the locale of the input text.</param>
        /// <returns>A list of tokens.</returns>
        /// <remarks>This is an exact port of the JavaScript implementation of the algorithm except that here
        /// the .NET library functions are used in place of the JavaScript string code point functions.</remarks>
        public static List<Token> DefaultTokenizerImpl(string text, string locale = null)
        {
            var tokens = new List<Token>();
            Token token = null;

            // Parse text
            var length = text?.Length ?? 0;
            var i = 0;

            while (i < length)
            {
                // Get both the UNICODE value of the current character and the complete character itself
                // which can potentially be multiple segments.
                var codePoint = char.IsSurrogatePair(text, i)
                    ? char.ConvertToUtf32(text, i)
                    : Convert.ToInt32(text[i]);

                var chr = char.ConvertFromUtf32(codePoint);

                // Process current character
                if (IsBreakingChar(codePoint))
                {
                    // Character is in Unicode Plane 0 and is in an excluded block
                    AppendToken(tokens, token, i - 1);
                    token = null;
                }
                else if (codePoint > 0xFFFF)
                {
                    // Character is in a Supplementary Unicode Plane. This is where emoji live so
                    // we're going to just break each character in this range out as its own token.
                    AppendToken(tokens, token, i - 1);
                    token = null;
                    tokens.Add(new Token
                    {
                        Start = i,
                        End = i + (chr.Length - 1),
                        Text = chr,
                        Normalized = chr,
                    });
                }
                else if (token == null)
                {
                    // Start a new token
                    token = new Token
                    {
                        Start = i,
                        Text = chr,
                    };
                }
                else
                {
                    // Add on to current token
                    token.Text += chr;
                }

                i += chr.Length;
            }

            AppendToken(tokens, token, length - 1);
            return tokens;
        }

        private static void AppendToken(List<Token> tokens, Token token, int end)
        {
            if (token != null)
            {
                token.End = end;
                token.Normalized = token.Text.ToLowerInvariant();
                tokens.Add(token);
            }
        }

        private static bool IsBreakingChar(int codePoint) => IsBetween(codePoint, 0x0000, 0x002F) ||
                                                             IsBetween(codePoint, 0x003A, 0x0040) ||
                                                             IsBetween(codePoint, 0x005B, 0x0060) ||
                                                             IsBetween(codePoint, 0x007B, 0x00BF) ||
                                                             IsBetween(codePoint, 0x02B9, 0x036F) ||
                                                             IsBetween(codePoint, 0x2000, 0x2BFF) ||
                                                             IsBetween(codePoint, 0x2E00, 0x2E7F);

        private static bool IsBetween(int value, int from, int to) => value >= from && value <= to;
    }
}
