// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains methods for matching user input against a list of choices.
    /// </summary>
    public static class Find
    {
        /// <summary>
        /// Matches user input against a list of choices.
        /// </summary>
        /// <param name="utterance">The input.</param>
        /// <param name="choices">The list of choices.</param>
        /// <param name="options">Optional, options to control the recognition strategy.</param>
        /// <returns>A list of found choices, sorted by most relevant first.</returns>
#pragma warning disable CA1002 // Do not expose generic lists
        public static List<ModelResult<FoundChoice>> FindChoices(string utterance, IList<string> choices, FindChoicesOptions options = null)
#pragma warning restore CA1002 // Do not expose generic lists
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            return FindChoices(utterance, choices.Select(s => new Choice { Value = s }).ToList(), options);
        }

        /// <summary>
        /// Matches user input against a list of choices.
        /// </summary>
        /// <param name="utterance">The input.</param>
        /// <param name="choices">The list of choices.</param>
        /// <param name="options">Optional, options to control the recognition strategy.</param>
        /// <returns>A list of found choices, sorted by most relevant first.</returns>
#pragma warning disable CA1002 // Do not expose generic lists
        public static List<ModelResult<FoundChoice>> FindChoices(string utterance, IList<Choice> choices, FindChoicesOptions options = null)
#pragma warning restore CA1002 // Do not expose generic lists
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            var opt = options ?? new FindChoicesOptions();

            // Build up full list of synonyms to search over.
            // - Each entry in the list contains the index of the choice it belongs to which will later be
            //   used to map the search results back to their choice.
            var synonyms = new List<SortedValue>();

            for (var index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];

                if (!opt.NoValue)
                {
                    synonyms.Add(new SortedValue { Value = choice.Value, Index = index });
                }

                if (choice.Action != null && choice.Action.Title != null && !opt.NoAction)
                {
                    synonyms.Add(new SortedValue { Value = choice.Action.Title, Index = index });
                }

                if (choice.Synonyms.Any())
                {
                    foreach (var synonym in choice.Synonyms)
                    {
                        synonyms.Add(new SortedValue { Value = synonym, Index = index });
                    }
                }
            }

            // Find synonyms in utterance and map back to their choices
            return FindValues(utterance, synonyms, options).Select((v) =>
             {
                 var choice = choices[v.Resolution.Index];
                 return new ModelResult<FoundChoice>
                 {
                     Start = v.Start,
                     End = v.End,
                     TypeName = "choice",
                     Text = v.Text,
                     Resolution = new FoundChoice
                     {
                         Value = choice.Value,
                         Index = v.Resolution.Index,
                         Score = v.Resolution.Score,
                         Synonym = v.Resolution.Value,
                     },
                 };
             }).ToList();
        }

        /// <summary>This method is internal and should not be used.</summary>
        /// <remarks>Please use <see cref="FindChoices(string, IList{Choice}, FindChoicesOptions)"/> or
        /// <see cref="FindChoices(string, IList{string}, FindChoicesOptions)"/> instead.</remarks>
        /// <param name="utterance">The input.</param>
        /// <param name="values">The values.</param>
        /// <param name="options">The options for the search.</param>
        /// <returns>A list of found values.</returns>
#pragma warning disable CA1002 // Do not expose generic lists
        public static List<ModelResult<FoundValue>> FindValues(string utterance, List<SortedValue> values, FindValuesOptions options = null)
#pragma warning restore CA1002 // Do not expose generic lists
        {
            if (FindExactMatch(utterance, values) is ModelResult<FoundValue> exactMatch)
            {
                return new List<ModelResult<FoundValue>> { exactMatch };
            }

            // Sort values in descending order by length so that the longest value is searched over first.
            var list = values;
            list.Sort((a, b) => b.Value.Length - a.Value.Length);

            // Search for each value within the utterance.
            var matches = new List<ModelResult<FoundValue>>();
            var opt = options ?? new FindValuesOptions();
            var tokenizer = opt.Tokenizer ?? Tokenizer.DefaultTokenizer;
            var tokens = tokenizer(utterance, opt.Locale);
            var maxDistance = opt.MaxTokenDistance ?? 2;

            foreach (var entry in list)
            {
                // Find all matches for a value
                // - To match "last one" in "the last time I chose the last one" we need
                //   to re-search the string starting from the end of the previous match.
                // - The start & end position returned for the match are token positions.
                var startPos = 0;
                var searchedTokens = tokenizer(entry.Value.Trim(), opt.Locale);
                while (startPos < tokens.Count)
                {
                    var match = MatchValue(tokens, maxDistance, opt, entry.Index, entry.Value, searchedTokens, startPos);
                    if (match != null)
                    {
                        startPos = match.End + 1;
                        matches.Add(match);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Sort matches by score descending
            matches.Sort((a, b) => b.Resolution.Score.CompareTo(a.Resolution.Score));

            // Filter out duplicate matching indexes and overlapping characters.
            // - The start & end positions are token positions and need to be translated to
            //   character positions before returning. We also need to populate the "text"
            //   field as well.
            var results = new List<ModelResult<FoundValue>>();
            var foundIndexes = new HashSet<int>();
            var usedTokens = new HashSet<int>();

            foreach (var match in matches)
            {
                // Apply filters
                var add = !foundIndexes.Contains(match.Resolution.Index);
                for (var i = match.Start; i <= match.End; i++)
                {
                    if (usedTokens.Contains(i))
                    {
                        add = false;
                        break;
                    }
                }

                // Add to results
                if (add)
                {
                    // Update filter info
                    foundIndexes.Add(match.Resolution.Index);

                    for (var i = match.Start; i <= match.End; i++)
                    {
                        usedTokens.Add(i);
                    }

                    // Translate start & end and populate text field
                    match.Start = tokens[match.Start].Start;
                    match.End = tokens[match.End].End;

                    // Note: JavaScript Substring is (start, end) whereas .NET is (start, len)
                    match.Text = utterance.Substring(match.Start, (match.End + 1) - match.Start);
                    results.Add(match);
                }
            }

            // Return the results sorted by position in the utterance
            results.Sort((a, b) => a.Start - b.Start);
            return results;
        }

        private static int IndexOfToken(IList<Token> tokens, Token token, int startPos)
        {
            for (var i = startPos; i < tokens.Count; i++)
            {
                if (tokens[i].Normalized == token.Normalized)
                {
                    return i;
                }
            }

            return -1;
        }

        private static ModelResult<FoundValue> MatchValue(IList<Token> sourceTokens, int maxDistance, FindValuesOptions options, int index, string value, List<Token> searchedTokens, int startPos)
        {
            // Match value to utterance and calculate total deviation.
            // - The tokens are matched in order so "second last" will match in
            //   "the second from last one" but not in "the last from the second one".
            // - The total deviation is a count of the number of tokens skipped in the
            //   match so for the example above the number of tokens matched would be
            //   2 and the total deviation would be 1.
            var matched = 0;
            var totalDeviation = 0;
            var start = -1;
            var end = -1;
            foreach (var token in searchedTokens)
            {
                // Find the position of the token in the utterance.
                var pos = IndexOfToken(sourceTokens, token, startPos);
                if (pos >= 0)
                {
                    // Calculate the distance between the current tokens position and the previous tokens distance.
                    var distance = matched > 0 ? pos - startPos : 0;
                    if (distance <= maxDistance)
                    {
                        // Update count of tokens matched and move start pointer to search for next token after
                        // the current token.
                        matched++;
                        totalDeviation += distance;
                        startPos = pos + 1;

                        // Update start & end position that will track the span of the utterance that's matched.
                        if (start < 0)
                        {
                            start = pos;
                        }

                        end = pos;
                    }
                }
            }

            // Calculate score and format result
            // - The start & end positions and the results text field will be corrected by the caller.
            ModelResult<FoundValue> result = null;

            if (matched > 0 && (matched == searchedTokens.Count || options.AllowPartialMatches))
            {
                // Percentage of tokens matched. If matching "second last" in
                // "the second from last one" the completeness would be 1.0 since
                // all tokens were found.
                var completeness = matched / searchedTokens.Count;

                // Accuracy of the match. The accuracy is reduced by additional tokens
                // occurring in the value that weren't in the utterance. So an utterance
                // of "second last" matched against a value of "second from last" would
                // result in an accuracy of 0.5.
                var accuracy = (float)matched / (matched + totalDeviation);

                // The final score is simply the completeness multiplied by the accuracy.
                var score = completeness * accuracy;

                // Format result
                result = new ModelResult<FoundValue>
                {
                    Start = start,
                    End = end,
                    TypeName = "value",
                    Resolution = new FoundValue
                    {
                        Value = value,
                        Index = index,
                        Score = score,
                    },
                };
            }

            return result;
        }

        private static ModelResult<FoundValue> FindExactMatch(string utterance, List<SortedValue> values)
        {
            foreach (var entry in values)
            {
                if (entry.Value.Equals(utterance, StringComparison.OrdinalIgnoreCase))
                {
                    return new ModelResult<FoundValue>
                    {
                        Text = utterance,
                        Start = 0,
                        End = utterance.Length - 1,
                        TypeName = "value",
                        Resolution = new FoundValue
                        {
                            Value = entry.Value,
                            Index = entry.Index,
                            Score = 1,
                        },
                    };
                }
            }

            return null;
        }
    }
}
