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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    internal class MatchAnalyzer
    {
        internal static void PrintMatches(IEnumerable<TermMatch> matches, int offset = 0)
        {
            foreach (var match in matches)
            {
                string message = string.Empty;
                message = message.PadRight(match.Start + offset, ' ');
                message = message.PadRight(match.End + offset, '_');
                Console.WriteLine("{0} {1}", message, match.Value);
            }
        }

        internal static bool IsIgnorable(string input, int start, int end)
        {
            return Language.NonWord(input.Substring(start, end - start));
        }

        internal static bool IsSpecial(object value)
        {
            return value is SpecialValues && (SpecialValues)value == SpecialValues.Field;
        }

        // Collapse together subsequent matches for same value or value has same range as no preference
        internal static IEnumerable<TermMatch> Coalesce(IEnumerable<TermMatch> matches, string input)
        {
            var sorted = (from match in matches
                          orderby match.Start ascending
                          , match.End ascending
                          , match.Value == null ascending
                          select match).ToList();
            while (sorted.Count() > 0)
            {
                var current = sorted.First();
                sorted.Remove(current);
                bool emit = true;
                foreach (var next in sorted.ToList())
                {
                    if (next.Covers(current))
                    {
                        // Current is completely covered by a subsequent match
                        emit = false;
                        break;
                    }
                    else if (current.End < next.Start)
                    {
                        var gap = next.Start - current.End;
                        if (gap > 1 && !Language.NonWord(input.Substring(current.End, gap)))
                        {
                            // Unmatched word means we can't merge any more
                            emit = true;
                            break;
                        }
                        else if (current.Value == next.Value || IsSpecial(current.Value) || IsSpecial(next.Value))
                        {
                            // Compatible, extend current match
                            current = new TermMatch(current.Start, next.End - current.Start, Math.Max(current.Confidence, next.Confidence),
                                        IsSpecial(current.Value) ? next.Value : current.Value);
                            sorted.Remove(next);
                        }
                    }
                    else if (next.Value == null && current.Overlaps(next))
                    {
                        // Remove no preference if there is any overlapping meaning
                        sorted.Remove(next);
                    }
                }
                if (emit && !IsSpecial(current.Value))
                {
                    sorted = (from match in sorted where !current.Covers(match) select match).ToList();
                    yield return current;
                }
            }
        }

        internal static IEnumerable<TermMatch> HighestConfidence(IEnumerable<TermMatch> matches)
        {
            var sorted = (from match in matches orderby match.Start ascending, match.End ascending, match.Confidence descending select match);
            TermMatch last = null;
            foreach (var match in sorted)
            {
                if (last == null || !last.Same(match) || last.Confidence == match.Confidence)
                {
                    last = match;
                    yield return match;
                }
            }
        }

        // Full match if everything left is white space or punctuation
        internal static bool IsFullMatch(string input, IEnumerable<TermMatch> matches, double threshold = 1.0)
        {
            bool fullMatch = matches.Count() > 0;
            var sorted = from match in matches orderby match.Start ascending select match;
            var current = 0;
            var minConfidence = 1.0;
            foreach (var match in sorted)
            {
                if (match.Start > current)
                {
                    if (!IsIgnorable(input, current, match.Start))
                    {
                        fullMatch = false;
                        break;
                    }
                }
                if (match.Confidence < minConfidence)
                {
                    minConfidence = match.Confidence;
                }
                current = match.End;
            }
            if (fullMatch && current < input.Length)
            {
                fullMatch = IsIgnorable(input, current, input.Length);
            }
            return fullMatch && minConfidence >= threshold;
        }

        internal static IEnumerable<string> Unmatched(string input, IEnumerable<TermMatch> matches)
        {
            var unmatched = new List<string>();
            var sorted = from match in matches orderby match.Start ascending select match;
            var current = 0;
            foreach (var match in sorted)
            {
                if (match.Start > current)
                {
                    if (!IsIgnorable(input, current, match.Start))
                    {
                        yield return input.Substring(current, match.Start - current).Trim();
                    }
                }
                current = match.End;
            }
            if (input.Length > current)
            {
                yield return input.Substring(current).Trim();
            }
        }

        internal static double MinConfidence(IEnumerable<TermMatch> matches)
        {
            return matches.Count() == 0 ? 0.0 : (from match in matches select match.Confidence).Min();
        }

        internal static int Coverage(IEnumerable<TermMatch> matches)
        {
            // TODO: This does not handle partial overlaps
            return matches.Count() == 0 ? 0 : (from match in GroupedMatches(matches) select match.First().Length).Sum();
        }

        internal static int BestMatches(params IEnumerable<TermMatch>[] allMatches)
        {
            int bestMatch = 0;
            var confidences = (from matches in allMatches select MinConfidence(matches)).ToArray();
            int bestCoverage = 0;
            double bestConfidence = 0;
            for (var i = 0; i < allMatches.Length; ++i)
            {
                var confidence = confidences[i];
                var coverage = Coverage(allMatches[i]);
                if (coverage > bestCoverage)
                {
                    bestConfidence = confidence;
                    bestCoverage = coverage;
                    bestMatch = i;
                }
                else if (coverage == bestCoverage && confidence > bestConfidence)
                {
                    bestConfidence = confidence;
                    bestCoverage = coverage;
                    bestMatch = i;
                }
            }
            return bestMatch;
        }

        internal static List<List<TermMatch>> GroupedMatches(IEnumerable<TermMatch> matches)
        {
            var groups = new List<List<TermMatch>>();
            var sorted = from match in matches orderby match.Start ascending, match.End descending select match;
            var current = sorted.FirstOrDefault();
            var currentGroup = new List<TermMatch>();
            foreach (var match in sorted)
            {
                if (match != current)
                {
                    if (current.Same(match))
                    {
                        // No preference loses to everything
                        if (current.Value == null)
                        {
                            current = match;
                        }
                        else if (match != null)
                        {
                            // Ambiguous match
                            currentGroup.Add(match);
                        }
                    }
                    else if (!current.Overlaps(match))
                    // TODO: We are not really handling partial overlap.  To do so we need a lattice.
                    {
                        // New group
                        currentGroup.Add(current);
                        groups.Add(currentGroup);
                        current = match;
                        currentGroup = new List<TermMatch>();
                    }
                }
            }
            if (current != null)
            {
                currentGroup.Add(current);
                groups.Add(currentGroup);
            }
            return groups;
        }
    }
}
