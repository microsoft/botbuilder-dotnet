// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    /// <summary>
    /// Language related utilities.
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Language stop words.
        /// </summary>
        public static HashSet<string> StopWords = new HashSet<string>(Resources.LanguageStopWords.SplitList());

        /// <summary>
        /// Language articles.
        /// </summary>
        public static HashSet<string> Articles = new HashSet<string>(Resources.LanguageArticles.SplitList());

        /// <summary>
        /// Test to see if word is all punctuation or white space.
        /// </summary>
        /// <param name="word">Word to check.</param>
        /// <returns>True if word is all punctuation or white space.</returns>
        public static bool NonWord(string word)
        {
            bool nonWord = true;
            foreach (var ch in word)
            {
                if (!(char.IsControl(ch) || char.IsPunctuation(ch) || char.IsWhiteSpace(ch)))
                {
                    nonWord = false;
                    break;
                }
            }
            return nonWord;
        }

        /// <summary>
        /// Test to see if a word is all noise.
        /// </summary>
        /// <param name="word">Word to test.</param>
        /// <returns>True if word is a number, a <see cref="NonWord(string)"/> or a <see cref="StopWords"/>.</returns>
        public static bool NoiseWord(string word)
        {
            double number;
            bool noiseWord = double.TryParse(word, out number);
            if (!noiseWord) noiseWord = NonWord(word);
            if (!noiseWord) noiseWord = StopWords.Contains(word.ToLower());
            return noiseWord;
        }

        /// <summary>
        /// Test to see if a word can be ignored in a resposne.
        /// </summary>
        /// <param name="word">Word to test.</param>
        /// <returns>True if word is a <see cref="NonWord(string)"/> or a <see cref="StopWords"/>.</returns>
        public static bool NoiseResponse(string word)
        {
            bool noiseWord = NonWord(word);
            if (!noiseWord) noiseWord = StopWords.Contains(word.ToLower());
            return noiseWord;
        }

        /// <summary>
        /// Test a word for articles or noise.
        /// </summary>
        /// <param name="word">Word to test.</param>
        /// <returns>True if word is <see cref="NonWord(string)"/> or <see cref="Articles"/>.</returns>
        public static bool ArticleOrNone(string word)
        {
            return NonWord(word) || Articles.Contains(word);
        }

        /// <summary>
        /// Test words to see if they are all ignorable in a response.
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static IEnumerable<string> NonNoiseWords(IEnumerable<string> words)
        {
            return from word in words where !NoiseResponse(word) select word;
        }

        /// <summary>
        /// Regular expression to break a string into words.
        /// </summary>
        public static Regex WordBreaker = new Regex(@"\w+", RegexOptions.Compiled);

        /// <summary>
        /// Break input into words.
        /// </summary>
        /// <param name="input">String to be broken.</param>
        /// <returns>Enumeration of words.</returns>
        public static IEnumerable<string> WordBreak(string input)
        {
            foreach (Match match in WordBreaker.Matches(input))
            {
                yield return match.Value;
            }
        }

        /// <summary>
        /// Break a string into words based on _ and case changes.
        /// </summary>
        /// <param name="original">Original string.</param>
        /// <returns>String with words on case change or _ boundaries.</returns>
        public static string CamelCase(string original)
        {
            var builder = new StringBuilder();
            var name = original.Trim();
            var previousUpper = Char.IsUpper(name[0]);
            var previousLetter = Char.IsLetter(name[0]);
            bool first = true;
            for (int i = 0; i < name.Length; ++i)
            {
                var ch = name[i];
                if (!first && (ch == '_' || ch == ' '))
                {
                    // Non begin _ as space
                    builder.Append(' ');
                }
                else
                {
                    var isUpper = Char.IsUpper(ch);
                    var isLetter = Char.IsLetter(ch);
                    if ((!previousUpper && isUpper)
                        || (isLetter != previousLetter)
                        || (!first && isUpper && (i + 1) < name.Length && Char.IsLower(name[i + 1])))
                    {
                        // Break on lower to upper, number boundaries and Upper to lower
                        builder.Append(' ');
                    }
                    previousUpper = isUpper;
                    previousLetter = isLetter;
                    builder.Append(ch);
                    if (first)
                    {
                        first = false;
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Make sure all words end with an optional s.
        /// </summary>
        /// <param name="words">Words to pluralize.</param>
        /// <returns>Enumeration of plural word regex.</returns>
        public static IEnumerable<string> OptionalPlurals(IEnumerable<string> words)
        {
            bool addS = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en";
            foreach (var original in words)
            {
                var word = original.ToLower();
                var newWord = word;
                if (addS && !NoiseWord(word) && word.Length > 1)
                {
                    newWord = (word.EndsWith("s") ? word + "?" : word + "s?");
                }
                yield return newWord;
            }
        }

        /// <summary>
        /// Generate regular expressions to match word sequences in original string.
        /// </summary>
        /// <param name="phrase">Original string to be processed.</param>
        /// <param name="maxLength">Maximum phrase length to support.</param>
        /// <returns>Array of regular expressions to match subsequences in input.</returns>
        /// <remarks>
        /// This function will call <see cref="CamelCase(string)"/> and then will generate sub-phrases up to maxLength.  
        /// For example an enumeration of AngusBeefAndGarlicPizza would generate: 'angus?', 'beefs?', 'garlics?', 'pizzas?', 'angus? beefs?', 'garlics? pizzas?' and 'angus beef and garlic pizza'.
        /// You can call it directly, or it is used when <see cref="FieldReflector{T}"/> generates terms or when <see cref="TermsAttribute"/> is used with a <see cref="TermsAttribute.MaxPhrase"/> argument.
        /// </remarks>
        public static string[] GenerateTerms(string phrase, int maxLength)
        {
            var words = (from word in phrase.Split(' ') where word.Length > 0 select word.ToLower()).ToArray();
            var terms = new List<string>();
            for (var length = 1; length <= Math.Min(words.Length, maxLength); ++length)
            {
                for (var start = 0; start <= words.Length - length; ++start)
                {
                    var ngram = new ArraySegment<string>(words, start, length);
                    if (!ArticleOrNone(ngram.First()) && !ArticleOrNone(ngram.Last()))
                    {
                        terms.Add(string.Join(" ", OptionalPlurals(ngram)));
                    }
                }
            }
            if (words.Length > maxLength)
            {
                terms.Add(string.Join(" ", words));
            }
            return terms.ToArray();
        }

        private static Regex _aOrAn = new Regex(@"\b(a|an)(?:\s+)([aeiou])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Switch 'a' before consonants and 'an' before vowels.
        /// </summary>
        /// <param name="input">String to fix.</param>
        /// <returns>String with 'a' and 'an' normalized.</returns>
        /// <remarks>
        /// This is not perfect because English is complex, but does a reasonable job.
        /// </remarks>
        public static string ANormalization(string input)
        {
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en")
            {
                var builder = new StringBuilder();
                var last = 0;
                foreach (Match match in _aOrAn.Matches(input))
                {
                    var currentWord = match.Groups[1];
                    builder.Append(input.Substring(last, currentWord.Index - last));
                    if (match.Groups[2].Success)
                    {
                        builder.Append("an");
                    }
                    else
                    {
                        builder.Append("a");
                    }
                    last = currentWord.Index + currentWord.Length;
                }
                builder.Append(input.Substring(last));
                return builder.ToString();
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Given a list of string values generate a proper English list.
        /// </summary>
        /// <param name="values">Value in list.</param>
        /// <param name="separator">Separator between all elements except last.</param>
        /// <param name="lastSeparator">Last element separator.</param>
        /// <returns>Value in a proper English list.</returns>
        public static string BuildList(IEnumerable<string> values, string separator, string lastSeparator)
        {
            var builder = new StringBuilder();
            var pos = 0;
            var end = values.Count() - 1;
            foreach (var elt in values)
            {
                if (pos > 0)
                {
                    builder.Append(pos == end ? lastSeparator : separator);
                }
                builder.Append(elt);
                ++pos;
            }
            return builder.ToString();
        }

        /// <summary>   Normalize a string. </summary>
        /// <param name="value">            The value to normalize. </param>
        /// <param name="normalization">    The normalization to apply. </param>
        /// <returns>   A normalized string. </returns>
        public static string Normalize(string value, CaseNormalization normalization)
        {
            switch (normalization)
            {
                case CaseNormalization.InitialUpper:
                    value = string.Join(" ", (from word in Language.WordBreak(value)
                                              select char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                    break;
                case CaseNormalization.Lower: value = value.ToLower(); break;
                case CaseNormalization.Upper: value = value.ToUpper(); break;
            }
            return value;
        }
    }
}
