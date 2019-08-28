// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Recognizers.Text.Number;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains methods for matching user input against a list of choices.
    /// </summary>
    public class ChoiceRecognizers
    {
        /// <summary>
        /// Matches user input against a list of choices.
        /// </summary>
        /// <param name="utterance">The input.</param>
        /// <param name="choices">The list of choices.</param>
        /// <param name="options">Optional, options to control the recognition strategy.</param>
        /// <returns>A list of found choices, sorted by most relevant first.</returns>
        public static List<ModelResult<FoundChoice>> RecognizeChoices(string utterance, IList<string> choices, FindChoicesOptions options = null)
        {
            return RecognizeChoices(utterance, choices.Select(s => new Choice { Value = s }).ToList(), options);
        }

        /// <summary>
        /// Matches user input against a list of choices.
        /// </summary>
        /// <param name="utterance">The input.</param>
        /// <param name="list">The list of choices.</param>
        /// <param name="options">Optional, options to control the recognition strategy.</param>
        /// <returns>A list of found choices, sorted by most relevant first.</returns>
        public static List<ModelResult<FoundChoice>> RecognizeChoices(string utterance, IList<Choice> list, FindChoicesOptions options = null)
        {
            if (utterance == null)
            {
                utterance = string.Empty;
            }

            // Try finding choices by text search first
            // - We only want to use a single strategy for returning results to avoid issues where utterances
            //   like the "the third one" or "the red one" or "the first division book" would miss-recognize as
            //   a numerical index or ordinal as well.
            var locale = options?.Locale ?? Recognizers.Text.Culture.English;
            var matched = Find.FindChoices(utterance, list, options);
            if (matched.Count == 0)
            {
                // Next try finding by ordinal
                var matches = RecognizeOrdinal(utterance, locale);
                if (matches.Any())
                {
                    foreach (var match in matches)
                    {
                        MatchChoiceByIndex(list, matched, match);
                    }
                }
                else
                {
                    // Finally try by numerical index
                    matches = RecognizeNumber(utterance, locale);

                    foreach (var match in matches)
                    {
                        MatchChoiceByIndex(list, matched, match);
                    }
                }

                // Sort any found matches by their position within the utterance.
                // - The results from findChoices() are already properly sorted so we just need this
                //   for ordinal & numerical lookups.
                matched.Sort((a, b) => a.Start - b.Start);
            }

            return matched;
        }

        private static void MatchChoiceByIndex(IList<Choice> list, List<ModelResult<FoundChoice>> matched, ModelResult<FoundChoice> match)
        {
            try
            {
                var index = int.Parse(match.Resolution.Value) - 1;
                if (index >= 0 && index < list.Count)
                {
                    var choice = list[index];
                    matched.Add(new ModelResult<FoundChoice>
                    {
                        Start = match.Start,
                        End = match.End,
                        TypeName = "choice",
                        Text = match.Text,
                        Resolution = new FoundChoice
                        {
                            Value = choice.Value,
                            Index = index,
                            Score = 1.0f,
                        },
                    });
                }
            }
            catch (Exception)
            {
            }
        }

        private static List<ModelResult<FoundChoice>> RecognizeOrdinal(string utterance, string culture)
        {
            var model = new NumberRecognizer(culture, NumberOptions.SuppressExtendedTypes).GetOrdinalModel(culture);
            var result = model.Parse(utterance);
            return result.Select(r =>
                new ModelResult<FoundChoice>
                {
                    Start = r.Start,
                    End = r.End,
                    Text = r.Text,
                    Resolution = new FoundChoice
                    {
                        Value = r.Resolution["value"].ToString(),
                    },
                }).ToList();
        }

        private static List<ModelResult<FoundChoice>> RecognizeNumber(string utterance, string culture)
        {
            var model = new NumberRecognizer(culture, NumberOptions.SuppressExtendedTypes).GetNumberModel(culture);
            var result = model.Parse(utterance);
            return result.Select(r =>
                new ModelResult<FoundChoice>
                {
                    Start = r.Start,
                    End = r.End,
                    Text = r.Text,
                    Resolution = new FoundChoice
                    {
                        Value = r.Resolution["value"].ToString(),
                    },
                }).ToList();
        }
    }
}
