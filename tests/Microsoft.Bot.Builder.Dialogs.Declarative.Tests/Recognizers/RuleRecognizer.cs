using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests.Recognizers
{
    public class RuleRecognizer : IRecognizer
    {
        private const string DefaultIntent = "None";

        public Dictionary<string, string> Rules { get; set; } = new Dictionary<string, string>();

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string intent = DefaultIntent;

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
                    { intent, new IntentScore() { Score = 1.0 } }
                }
            });
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) 
            where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }
    }
}
