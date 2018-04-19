using System;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Contains methods for working with intent recognizer results.
    /// </summary>
    public static class RecognizerResultExtensions
    {
        /// <summary>
        /// Gets the top-scoring intent from recognition results.
        /// </summary>
        /// <param name="result">The recognition results.</param>
        /// <returns>A tuple of the key the of the top scoring intent and the score associated with that intent.</returns>
        public static (string key, double score) GetTopScoringIntent(this RecognizerResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.Intents == null)
                throw new ArgumentNullException(nameof(result.Intents));

            var topIntent = (string.Empty, 0.0d);
            foreach(var intent in result.Intents)
            {
                var score = (double) intent.Value;
                if (score > topIntent.Item2)
                {
                    topIntent = (intent.Key, score);
                }
            }
            return topIntent;
        }
    }
}
