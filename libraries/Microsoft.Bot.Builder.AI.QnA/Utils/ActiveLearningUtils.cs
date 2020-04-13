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
        /// Minimum Score For Low Score Variation.
        /// </summary>
        private const double MinimumScoreForLowScoreVariation = 20.0;

        /// <summary>
        /// Previous Low Score Variation Multiplier.
        /// </summary>
        private const double PreviousLowScoreVariationMultiplier = 0.7;

        /// <summary>
        /// Max Low Score Variation Multiplier.
        /// </summary>
        private const double MaxLowScoreVariationMultiplier = 1.0;

        /// <summary>
        /// Maximum Score For Low Score Variation.
        /// </summary>
        private const double MaximumScoreForLowScoreVariation = 95.0;

        /// <summary>
        /// Returns list of qnaSearch results which have low score variation.
        /// </summary>
        /// <param name="qnaSearchResults">List of QnaSearch results.</param>
        /// <returns>List of filtered qnaSearch results.</returns>
        public static List<QueryResult> GetLowScoreVariation(List<QueryResult> qnaSearchResults)
        {
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
            if (topAnswerScore > MaximumScoreForLowScoreVariation)
            {
                filteredQnaSearchResult.Add(qnaSearchResults[0]);
                return filteredQnaSearchResult;
            }
            
            var prevScore = topAnswerScore;

            if (topAnswerScore > MinimumScoreForLowScoreVariation) 
            {
                filteredQnaSearchResult.Add(qnaSearchResults[0]);

                for (var i = 1; i < qnaSearchResults.Count; i++)
                {
                    if (IncludeForClustering(prevScore, qnaSearchResults[i].Score * 100, PreviousLowScoreVariationMultiplier) && IncludeForClustering(topAnswerScore, qnaSearchResults[i].Score * 100, MaxLowScoreVariationMultiplier))
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
