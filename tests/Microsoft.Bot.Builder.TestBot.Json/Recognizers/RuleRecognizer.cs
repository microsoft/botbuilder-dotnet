using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TestBot.Json.Recognizers
{
    public class RuleRecognizer : IRecognizer
    {
        public Dictionary<string, string> Rules { get; set; } = new Dictionary<string, string>();

        private const string defaultIntent = "None";

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string intent = defaultIntent;

            if (Rules.TryGetValue(turnContext.Activity.Text, out string value))
            {
                intent = value;
            }

            return Task.FromResult(new RecognizerResult()
            {
                Text = turnContext.Activity.Text,
                AlteredText = turnContext.Activity.Text,
                Intents = new Dictionary<string, IntentScore>()
                {
                    { intent, new IntentScore() { Score = 1.0} }
                }
            });
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }
    }
}
