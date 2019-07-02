// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// IRecognizer implementation which uses regex expressions to identify intents
    /// </summary>
    public class RegexRecognizer : IRecognizer
    {
        private Dictionary<string, Regex> patterns = new Dictionary<string, Regex>();

        /// <summary>
        /// Dictionary of patterns -> Intent names
        /// </summary>
        [JsonProperty("intents")]
        public Dictionary<string, string> Intents = new Dictionary<string, string>();

        public RegexRecognizer()
        {

        }

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

            foreach (var kv in Intents)
            {
                var intent = kv.Key;
                Regex regex;
                lock (patterns)
                {
                    if (!patterns.TryGetValue(intent, out regex))
                    {
                        regex = CommonRegex.CreateRegex(kv.Value);
                        patterns.Add(intent, regex);
                    }
                }

                var match = regex.Match(utterance);

                if (match.Success)
                {
                    // TODO length weighted match and multiple intents
                    result.Intents.Add(intent.Replace(" ", "_"), new IntentScore() { Score = 1.0 });

                    // Check for named capture groups
                    var entities = new Dictionary<string, string>();
                    foreach (var name in regex.GetGroupNames().Where(name => name.Length > 1))
                    {
                        // only if we have a value and the name is not a number "0"
                        if (!string.IsNullOrEmpty(match.Groups[name].Value))
                        {
                            entities.Add(name, match.Groups[name].Value);
                        }
                    }
                    result.Entities = JObject.FromObject(entities);
                }
            }

            // if no match return None intent
            if (!result.Intents.Keys.Any())
            {
                result.Intents.Add("None", new IntentScore() { Score = 1.0 });
                result.Entities = new JObject();
            }

            return result;
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            throw new NotImplementedException();
        }
    }
}
