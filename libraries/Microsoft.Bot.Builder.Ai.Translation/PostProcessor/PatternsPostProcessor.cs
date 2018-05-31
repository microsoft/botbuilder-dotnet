// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    /// <summary>
    /// PatternsPostProcessor  is used to handle translation errors while translating numbers
    /// and to handle the words that need to be kept same as source language from provided template each line having a regex
    /// having first group matching the words that needs to be kept.
    /// </summary>
    public class PatternsPostProcessor : IPostProcessor
    {
        private readonly Dictionary<string, HashSet<string>> _processedPatterns;

        /// <summary>
        /// Constructor that indexes input template for the source language.
        /// </summary>
        /// <param name="patterns">No translate patterns for different languages</param> 
        public PatternsPostProcessor(Dictionary<string, List<string>> patterns)
        {
            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            if (patterns.Count == 0)
            {
                throw new ArgumentException(MessagesProvider.EmptyPatternsErrorMessage);
            }

            _processedPatterns = new Dictionary<string, HashSet<string>>();
            foreach (KeyValuePair<string, List<string>> item in patterns)
            {
                _processedPatterns.Add(item.Key, new HashSet<string>());
                foreach (string pattern in item.Value)
                {
                    string processedLine = pattern.Trim();
                    if (!processedLine.Contains('('))
                    {
                        processedLine = '(' + processedLine + ')';
                    }
                    _processedPatterns[item.Key].Add(processedLine);
                }
            }
        }

        ///// <summary>
        ///// Process the logic for patterns post processor used to handle numbers and no translate list.
        ///// </summary>
        ///// <param name="translatedDocument">Translated document</param>
        ///// <param name="currentLanguage">Current source language</param>
        ///// <returns>A <see cref="PostProcessedDocument"/> stores the original translated document state and the newly post processed message</returns>
        //public PostProcessedDocument Process(TranslatedDocument translatedDocument, string currentLanguage)
        //{
        //    ValidateParameters(translatedDocument);
        //    bool containsNum = Regex.IsMatch(translatedDocument.SourceMessage, @"\d");
        //    string processedResult;
        //    HashSet<string> temporaryPatterns = _processedPatterns[currentLanguage];

        //    if (translatedDocument.LiteranlNoTranslatePhrases != null && translatedDocument.LiteranlNoTranslatePhrases.Count > 0)
        //    {
        //        temporaryPatterns.UnionWith((translatedDocument.LiteranlNoTranslatePhrases));
        //    }

        //    if (temporaryPatterns.Count == 0 && !containsNum)
        //        processedResult = translatedDocument.TargetMessage;

        //    if (string.IsNullOrWhiteSpace(translatedDocument.RawAlignment))
        //        processedResult = translatedDocument.TargetMessage;

        //    var toBeReplaced = from result in temporaryPatterns
        //                       where Regex.IsMatch(translatedDocument.SourceMessage, result, RegexOptions.Singleline | RegexOptions.IgnoreCase)
        //                       select result;

        //    if (toBeReplaced.Any())
        //    {
        //        foreach (string pattern in toBeReplaced)
        //        {
        //            Match matchNoTranslate = Regex.Match(translatedDocument.SourceMessage, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        //            int noTranslateStartChrIndex = matchNoTranslate.Groups[1].Index;
        //            int noTranslateMatchLength = matchNoTranslate.Groups[1].Length;
        //            int wrdIndx = 0;
        //            int chrIndx = 0;
        //            int newChrLengthFromMatch = 0;
        //            int srcIndex = -1;
        //            int newNoTranslateArrayLength = 1;
        //            foreach (string wrd in translatedDocument.SourceTokens)
        //            {

        //                chrIndx += wrd.Length + 1;
        //                wrdIndx++;
        //                if (chrIndx == noTranslateStartChrIndex)
        //                {
        //                    srcIndex = wrdIndx;
        //                }
        //                if (srcIndex != -1)
        //                {
        //                    if (newChrLengthFromMatch + translatedDocument.SourceTokens[wrdIndx].Length >= noTranslateMatchLength)
        //                        break;
        //                    newNoTranslateArrayLength += 1;
        //                    newChrLengthFromMatch += translatedDocument.SourceTokens[wrdIndx].Length + 1;
        //                }

        //            }
        //            if (srcIndex == -1)
        //                continue;
        //            string[] wrdNoTranslate = new string[newNoTranslateArrayLength];
        //            Array.Copy(translatedDocument.SourceTokens, srcIndex, wrdNoTranslate, 0, newNoTranslateArrayLength);
        //            foreach (string srcWrd in wrdNoTranslate)
        //            {
        //                translatedDocument.TranslatedTokens = PostProcessingUtilities.KeepSrcWrdInTranslation(translatedDocument.IndexedAlignment, translatedDocument.SourceTokens, translatedDocument.TranslatedTokens, srcIndex);
        //                srcIndex++;
        //            }
        //        }
        //    }

        //    MatchCollection numericMatches = Regex.Matches(translatedDocument.SourceMessage, @"\d+", RegexOptions.Singleline);
        //    foreach (Match numericMatch in numericMatches)
        //    {
        //        int srcIndex = Array.FindIndex(translatedDocument.SourceTokens, row => row == numericMatch.Groups[0].Value);
        //        translatedDocument.TranslatedTokens = PostProcessingUtilities.KeepSrcWrdInTranslation(translatedDocument.IndexedAlignment, translatedDocument.SourceTokens, translatedDocument.TranslatedTokens, srcIndex);
        //    }
        //    processedResult = PostProcessingUtilities.Join(" ", translatedDocument.TranslatedTokens);
        //    return new PostProcessedDocument(translatedDocument, processedResult);
        //}

        public PostProcessedDocument Process(TranslatedDocument translatedDocument, string currentLanguage)
        {
            ValidateParameters(translatedDocument);
            bool containsNum = Regex.IsMatch(translatedDocument.SourceMessage, @"\d");
            string processedResult;
            HashSet<string> temporaryPatterns = _processedPatterns[currentLanguage];

            if (translatedDocument.LiteranlNoTranslatePhrases != null && translatedDocument.LiteranlNoTranslatePhrases.Count > 0)
            {
                temporaryPatterns.UnionWith((translatedDocument.LiteranlNoTranslatePhrases));
            }

            if (temporaryPatterns.Count == 0 && !containsNum)
            {
                processedResult = translatedDocument.TargetMessage;
            }

            if (string.IsNullOrWhiteSpace(translatedDocument.RawAlignment))
            {
                processedResult = translatedDocument.TargetMessage;
            }

            foreach (string pattern in temporaryPatterns)
            {
                if (Regex.IsMatch(translatedDocument.SourceMessage, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                {
                    SubstitutePattern(translatedDocument, pattern);
                }
            }
            SubstitutePattern(translatedDocument);
            processedResult = PostProcessingUtilities.Join(" ", translatedDocument.TranslatedTokens);
            return new PostProcessedDocument(translatedDocument, processedResult);
        }
        private void SubstitutePattern(TranslatedDocument translatedDocument, string pattern)
        {
            Match matchNoTranslate = Regex.Match(translatedDocument.SourceMessage, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            int noTranslateStartChrIndex = matchNoTranslate.Groups[1].Index;
            int noTranslateMatchLength = matchNoTranslate.Groups[1].Length;
            int wrdIndx = 0;
            int chrIndx = 0;
            int newChrLengthFromMatch = 0;
            int srcIndex = -1;
            int newNoTranslateArrayLength = 1;
            foreach (string wrd in translatedDocument.SourceTokens)
            {

                chrIndx += wrd.Length + 1;
                wrdIndx++;
                if (chrIndx == noTranslateStartChrIndex)
                {
                    srcIndex = wrdIndx;
                }
                if (srcIndex != -1)
                {
                    if (newChrLengthFromMatch + translatedDocument.SourceTokens[wrdIndx].Length >= noTranslateMatchLength)
                        break;
                    newNoTranslateArrayLength += 1;
                    newChrLengthFromMatch += translatedDocument.SourceTokens[wrdIndx].Length + 1;
                }

            }
            if (srcIndex == -1)
                return;
            string[] wrdNoTranslate = new string[newNoTranslateArrayLength];
            Array.Copy(translatedDocument.SourceTokens, srcIndex, wrdNoTranslate, 0, newNoTranslateArrayLength);
            foreach (string srcWrd in wrdNoTranslate)
            {
                translatedDocument.TranslatedTokens = PostProcessingUtilities.KeepSourceWordInTranslation(translatedDocument.IndexedAlignment, translatedDocument.SourceTokens, translatedDocument.TranslatedTokens, srcIndex);
                srcIndex++;
            }
        }

        private void SubstitutePattern(TranslatedDocument translatedDocument)
        {
            MatchCollection numericMatches = Regex.Matches(translatedDocument.SourceMessage, @"\d+", RegexOptions.Singleline);
            foreach (Match numericMatch in numericMatches)
            {
                int srcIndex = Array.FindIndex(translatedDocument.SourceTokens, row => row == numericMatch.Groups[0].Value);
                translatedDocument.TranslatedTokens = PostProcessingUtilities.KeepSourceWordInTranslation(translatedDocument.IndexedAlignment, translatedDocument.SourceTokens, translatedDocument.TranslatedTokens, srcIndex);
            }
        }

        /// <summary>
        /// Validate <see cref="TranslatedDocument"/> object main parameters for null values.
        /// </summary>
        /// <param name="translatedDocument"></param>
        private void ValidateParameters(TranslatedDocument translatedDocument)
        {
            if (translatedDocument == null)
            {
                throw new ArgumentNullException(nameof(translatedDocument));
            }

            if (translatedDocument.SourceMessage == null)
            {
                throw new ArgumentNullException(nameof(translatedDocument.SourceMessage));
            }

            if (translatedDocument.TargetMessage == null)
            {
                throw new ArgumentNullException(nameof(translatedDocument.TargetMessage));
            }
        }
    }
}
