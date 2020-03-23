// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Convert PCRE regex string to <see cref="Regex"/> object.
    /// PCRE ref: http://www.pcre.org/.
    /// PCRE antlr g4 file: CommonRegex.g4.
    /// </summary>
    public static class CommonRegex
    {
        private static readonly LRUCache<string, Regex> RegexCache = new LRUCache<string, Regex>(15);

        /// <summary>
        /// Create <see cref="Regex"/> object from PCRE pattern string.
        /// </summary>
        /// <param name="pattern">PCRE pattern string.</param>
        /// <returns>Regex object.</returns>
        public static Regex CreateRegex(string pattern)
        {
            Regex result;
            if (!string.IsNullOrEmpty(pattern) && RegexCache.TryGet(pattern, out var regex))
            {
                result = regex;
            }
            else
            {
                if (string.IsNullOrEmpty(pattern) || !IsCommonRegex(pattern))
                {
                    throw new ArgumentException($"'{pattern}' is not a valid regex.");
                }

                result = new Regex(pattern, RegexOptions.Compiled);
                RegexCache.Set(pattern, result);
            }

            return result;
        }

        private static bool IsCommonRegex(string pattern)
        {
            try
            {
                AntlrParse(pattern);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static IParseTree AntlrParse(string pattern)
        {
            var inputStream = new AntlrInputStream(pattern);
            var lexer = new CommonRegexLexer(inputStream);
            lexer.RemoveErrorListeners();
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new CommonRegexParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new RegexErrorListener());
            parser.BuildParseTree = true;

            return parser.parse();
        }
    }
}
