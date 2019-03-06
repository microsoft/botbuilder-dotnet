using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Planning.Recognizers
{
    public class RegexRecognizer : IRecognizer
    {
        public Dictionary<string, string> Rules = new Dictionary<string, string>();
        
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Process only messages
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return new RecognizerResult() { Text = turnContext.Activity.Text };
            }

            // Identify matched intents
            var utterance = turnContext.Activity.Text ?? string.Empty;

            var result = new RecognizerResult()
            {
                Text = utterance,
                Intents = new Dictionary<string, IntentScore>(),
            };

            foreach (var kv in Rules)
            {
                var intent = kv.Key;
                var regex = new Regex(kv.Value);

                var match = regex.Match(utterance);

                if (match.Success)
                {
                    // TODO length weighted match and multiple intents
                    result.Intents.Add(intent, new IntentScore() { Score = 1.0 });

                    // Check for named capture groups
                    // TODO: Implement
                    //if (match.Groups?.Count > 0)
                    //{
                    //    result.Entities = JObject.FromObject(match.Groups.Cast<Group>().SelectMany(g => g.Captures.Cast<Capture>().Select(c => new KeyValuePair<int, string>(c.Index, c.Value))));
                    //}
                    //else
                    //{
                    //    result.Entities = JObject.FromObject(match.Groups.Cast<Match>().Select(m => new KeyValuePair<int, string>(m.Index, m.Value)));
                    //}
                }
            }

            return result;
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }
    }
}
