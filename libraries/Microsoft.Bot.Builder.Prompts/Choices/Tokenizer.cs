// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class Token
    {
        public int Start { get; set; }
        public int End { get; set; }
        public string Text { get; set; }
        public string Normalized { get; set; }
    }

    public delegate List<Token> TokenizerFunction(string text, string locale = null);

    public class Tokenizer
    {
        public static TokenizerFunction DefaultTokenizer = DefaultTokenizerImpl;

        ///<summary>
        /// Simple tokenizer that breaks on spaces and punctuation. The only normalization done is to lowercase.
        /// This is an exact port of the JavaScript implementation of the algorithm except that here
        /// the .NET library functions are used in place of the JavaScript string code point functions.
        ///</summary>
        public static List<Token> DefaultTokenizerImpl(string text, string locale = null)
        {
            var tokens = new List<Token>();
            Token token = null;

            // Parse text
            var length = text != null  ? text.Length : 0;
            var i = 0;

            while (i<length)
            {
                // Get both the UNICODE value of the current character and the complete character itself
                // which can potentially be multiple segments.

                int codePoint = char.IsSurrogatePair(text, i) 
                        ?
                    char.ConvertToUtf32(text, i)
                        :
                    Convert.ToInt32(text[i]);

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
                        Normalized = chr
                    }); 
                }
                else if (token == null)
                {
                    // Start a new token
                    token = new Token
                    {
                        Start = i,
                        Text = chr
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
                token.Normalized = token.Text.ToLower();
                tokens.Add(token);
            }
        }

        private static bool IsBreakingChar(int codePoint)
        {
            return (IsBetween(codePoint, 0x0000, 0x002F) || 
                    IsBetween(codePoint, 0x003A, 0x0040) ||
                    IsBetween(codePoint, 0x005B, 0x0060) ||
                    IsBetween(codePoint, 0x007B, 0x00BF) ||
                    IsBetween(codePoint, 0x02B9, 0x036F) ||
                    IsBetween(codePoint, 0x2000, 0x2BFF) ||
                    IsBetween(codePoint, 0x2E00, 0x2E7F)); 
        }

        private static bool IsBetween(int value, int from, int to)
        {
            return (value >= from && value <= to);
        }
    }
}
