// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public static class RecognizerResultExtensions
    {
        /// <summary>
        /// Return the top scoring intent and its score.
        /// </summary>
        /// <param name="result">Recognizer result.</param>
        /// <returns>Intent and score.</returns>
        public static (string intent, double score) GetTopScoringIntent(this RecognizerResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.Intents == null)
                throw new ArgumentNullException(nameof(result.Intents));

            var topIntent = (string.Empty, 0.0d);
            foreach(var intent in result.Intents)
            {
                var score = (double) intent.Value["score"];
                if (score > topIntent.Item2)
                {
                    topIntent = (intent.Key, score);
                }
            }
            return topIntent;
        }
    }
}
