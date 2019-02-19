using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CCI.Content.Entities;

namespace Microsoft.Bot.Builder.TestBot.Json.CCI
{
    internal class ExactMatchRecognizer : IRecognizer
    {
        private IList<Intent> intents;

        public ExactMatchRecognizer(IList<Intent> intents)
        {
            this.intents = intents;
        }

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var intent = GetExactMatchIntent(turnContext.Activity.Text) ?? intents[0];
            return Task.FromResult(new RecognizerResult()
            {
                Intents = new Dictionary<string, IntentScore>()
                {
                    {intent.Id, new IntentScore(){Score = 1.0} }
                }
            });
        }
    

        private Intent GetExactMatchIntent(string text)
        {
            foreach (var intent in intents)
            {
                if (string.Equals(text, intent.DisplayName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return intent;
                }

                // CCI System intents don't have trigger queries
                if (intent.TriggerQueries != null)
                {
                    foreach (var triggerQuery in intent.TriggerQueries)
                    {
                        if (string.Equals(text, triggerQuery, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return intent;
                        }
                    }
                }
            }

            return null;
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            throw new System.NotImplementedException();
        }
    }
}
