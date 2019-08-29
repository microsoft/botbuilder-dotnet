// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition;
using Microsoft.Bot.Builder.Dialogs.Composition.Recognizers;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schema.NET;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// IRecognizer implementation which uses regex expressions to identify intents.
    /// </summary>
    public class RegexRecognizer : IRecognizer
    {
        private Dictionary<string, Regex> patterns = new Dictionary<string, Regex>();

        public RegexRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets dictionary of patterns -> Intent names.
        /// </summary>
        /// <value>
        /// Dictionary of patterns -> Intent names.
        /// </value>
        [JsonProperty("intents")]
        public Dictionary<string, string> Intents { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the entity recognizers.
        /// </summary>
        [JsonProperty("entities")]
        public EntityRecognizerSet EntityRecognizer { get; set; } = new EntityRecognizerSet();

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Process only messages
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return await Task.FromResult(new RecognizerResult() { Text = turnContext.Activity.Text });
            }

            // Identify matched intents
            var utterance = turnContext.Activity.Text ?? string.Empty;

            var result = new RecognizerResult()
            {
                Text = utterance,
                Intents = new Dictionary<string, IntentScore>(),
            };

            lock (patterns)
            {
                foreach (var kv in Intents)
                {
                    var intent = kv.Key;
                    if (!patterns.TryGetValue(intent, out Regex regex))
                    {
                        regex = CommonRegex.CreateRegex(kv.Value);
                        patterns.Add(intent, regex);
                    }
                }

                patterns = patterns.OrderByDescending(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            }

            var entities = new Dictionary<string, List<string>>();
            foreach (var pattern in patterns)
            {
                var intent = pattern.Key;

                foreach (var regex in patterns.Values)
                {
                    var matches = pattern.Value.Matches(utterance);

                    if (matches.Count > 0)
                    {
                        // TODO length weighted match and multiple intents
                        var intentKey = intent.Replace(" ", "_");
                        if (!result.Intents.ContainsKey(intentKey))
                        {
                            result.Intents.Add(intentKey, new IntentScore() { Score = 1.0 });
                        }

                        // Check for named capture groups
                        // only if we have a value and the name is not a number "0"
                        foreach (var groupName in regex.GetGroupNames().Where(name => name.Length > 1))
                        {
                            foreach (var match in matches.Cast<Match>())
                            {
                                var group = (Group)match.Groups[groupName];
                                if (group.Success)
                                {
                                    List<string> values;
                                    if (!entities.TryGetValue(groupName, out values))
                                    {
                                        values = new List<string>();
                                        entities.Add(groupName, values);
                                    }

                                    values.Add(group.Value);
                                }
                            }
                        }
                    }
                }
            }

            if (this.EntityRecognizer != null)
            {
                IList<Entity> entities2 = new List<Entity>();
                entities2 = await this.EntityRecognizer.RecognizeEntities(turnContext, entities2).ConfigureAwait(false);
                foreach (var entity in entities2)
                {
                    if (!entities.TryGetValue(entity.Type, out List<string> values))
                    {
                        values = new List<string>();
                        entities[entity.Type] = values;
                    }

                    values.Add((string)entity.Properties["Text"]);
                }
            }

            result.Entities = JObject.FromObject(entities);

            // if no match return None intent
            if (!result.Intents.Keys.Any())
            {
                result.Intents.Add("None", new IntentScore() { Score = 1.0 });
            }

            return result;
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = await this.RecognizeAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return JObject.FromObject(result).ToObject<T>();
        }
    }
}
