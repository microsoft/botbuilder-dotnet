// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Active learning helper class.
    /// </summary>
    public static class ActiveLearningUtils
    {       
        /// <summary>
        /// Returns list of qnaSearch results which have low score variation.
        /// </summary>
        /// <param name="qnaSearchResults">List of QnaSearch results.</param>
        /// <returns>List of filtered qnaSearch results.</returns>
        public static List<QueryResult> GetLowScoreVariation(List<QueryResult> qnaSearchResults)
        {
            return GetLowScoreVariation(qnaSearchResults, 95);
        }

        /// <summary>
        /// Returns list of qnaSearch results which have low score variation.
        /// </summary>
        /// <param name="qnaSearchResults">List of QnaSearch results.</param>
        /// <param name="maximumScoreForLowScoreVariation">maximumScoreForLowScoreVariation.</param>
        /// <param name="minimumScoreForLowScoreVariation">minimumScoreForLowScoreVariation.</param>
        /// <param name="previousLowScoreVariationMultiplier">previousLowScoreVariationMultiplier.</param>
        /// <param name="maxLowScoreVariationMultiplier">maxLowScoreVariationMultiplier.</param>
        /// <returns>List of filtered qnaSearch results.</returns>
        public static List<QueryResult> GetLowScoreVariation(
            List<QueryResult> qnaSearchResults,
            double maximumScoreForLowScoreVariation = 95.0,
            double minimumScoreForLowScoreVariation = 20.0,
            double previousLowScoreVariationMultiplier = 0.7,
            double maxLowScoreVariationMultiplier = 95.0)
        {
            if (qnaSearchResults is null)
            {
                throw new ArgumentNullException(nameof(qnaSearchResults));
            }

            var filteredQnaSearchResult = new List<QueryResult>();
            if (qnaSearchResults == null || qnaSearchResults.Count == 0)
            {
                return filteredQnaSearchResult;
            }

            if (qnaSearchResults.Count == 1)
            {
                return qnaSearchResults;
            }

            var topAnswerScore = qnaSearchResults[0].Score * 100;
            if (topAnswerScore > maximumScoreForLowScoreVariation)
            {
                filteredQnaSearchResult.Add(qnaSearchResults[0]);
                return filteredQnaSearchResult;
            }

            var prevScore = topAnswerScore;
            if (topAnswerScore > minimumScoreForLowScoreVariation)
            {
                filteredQnaSearchResult.Add(qnaSearchResults[0]);
                for (var i = 1; i < qnaSearchResults.Count; i++)
                {
                    if (IncludeForClustering(prevScore, qnaSearchResults[i].Score * 100, previousLowScoreVariationMultiplier) && IncludeForClustering(topAnswerScore, qnaSearchResults[i].Score * 100, maxLowScoreVariationMultiplier))
                    {
                        prevScore = qnaSearchResults[i].Score * 100;
                        filteredQnaSearchResult.Add(qnaSearchResults[i]);
                    }
                }
            }

            return filteredQnaSearchResult;
        }

        private static bool IncludeForClustering(double prevScore, double currentScore, double multiplier)
        {
            return (prevScore - currentScore) < (multiplier * Math.Sqrt(prevScore));
        }
    }
}
