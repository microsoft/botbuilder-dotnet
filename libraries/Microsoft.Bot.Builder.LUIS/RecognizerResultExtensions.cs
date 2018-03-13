using System;

namespace Microsoft.Bot.Builder.LUIS
{
    public static class RecognizerResultExtensions
    {
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
