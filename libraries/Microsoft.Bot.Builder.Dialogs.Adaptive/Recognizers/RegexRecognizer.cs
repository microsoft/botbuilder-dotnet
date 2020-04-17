// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizer implementation which uses regex expressions to identify intents.
    /// </summary>
    public class RegexRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RegexRecognizer";

        [JsonConstructor]
        public RegexRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets intent patterns for recognizing intents using regular expressions.
        /// </summary>
        /// <value>
        /// Dictionary of patterns -> Intent names.
        /// </value>
        [JsonProperty("intents")]
        public List<IntentPattern> Intents { get; set; } = new List<IntentPattern>();

        /// <summary>
        /// Gets or sets the entity recognizers.
        /// </summary>
        /// <value>
        /// The entity recognizers.
        /// </value>
        [JsonProperty("entities")]
        public List<EntityRecognizer> Entities { get; set; } = new List<EntityRecognizer>();

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            // Identify matched intents
            var text = activity.Text ?? string.Empty;
            var locale = activity.Locale ?? "en-us";

            var recognizerResult = new RecognizerResult()
            {
                Text = text,
                Intents = new Dictionary<string, IntentScore>(),
            };

            // add entities from regexrecgonizer to the entities pool
            var entityPool = new List<Entity>();

            var textEntity = new TextEntity(text);
            textEntity.Properties["start"] = 0;
            textEntity.Properties["end"] = text.Length;
            textEntity.Properties["score"] = 1.0;

            entityPool.Add(textEntity);

            foreach (var intentPattern in this.Intents)
            {
                var matches = intentPattern.Regex.Matches(text);

                if (matches.Count > 0)
                {
                    // TODO length weighted match and multiple intents
                    var intentKey = intentPattern.Intent.Replace(" ", "_");
                    if (!recognizerResult.Intents.ContainsKey(intentKey))
                    {
                        recognizerResult.Intents.Add(intentKey, new IntentScore()
                        {
                            Score = 1.0,
                            Properties = new Dictionary<string, object>() { { "pattern", intentPattern.Pattern } }
                        });
                    }

                    // Check for named capture groups
                    // only if we have a value and the name is not a number "0"
                    foreach (var groupName in intentPattern.Regex.GetGroupNames().Where(name => name.Length > 1))
                    {
                        foreach (var match in matches.Cast<Match>())
                        {
                            var group = (Group)match.Groups[groupName];
                            if (group.Success)
                            {
                                // add as entity to entity pool
                                Entity entity = new Entity(groupName);
                                entity.Properties["text"] = group.Value;
                                entity.Properties["start"] = group.Index;
                                entity.Properties["end"] = group.Index + group.Length;
                                entityPool.Add(entity);
                            }
                        }
                    }

                    // found
                    break;
                }
            }

            if (this.Entities != null)
            {
                // process entities using EntityRecognizerSet
                var entitySet = new EntityRecognizerSet(this.Entities);
                var newEntities = await entitySet.RecognizeEntities(dialogContext, text, locale, entityPool).ConfigureAwait(false);
                if (newEntities.Any())
                {
                    entityPool.AddRange(newEntities);
                }
            }

            // map entityPool of Entity objects => RecognizerResult entity format
            recognizerResult.Entities = new JObject();

            foreach (var entityResult in entityPool)
            {
                // add value
                JToken values;
                if (!recognizerResult.Entities.TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out values))
                {
                    values = new JArray();
                    recognizerResult.Entities[entityResult.Type] = values;
                }

                // The Entity type names are not consistent, map everything to camelcase so we can process them cleaner.
                dynamic entity = JObject.FromObject(entityResult);
                ((JArray)values).Add(entity.text);

                // get/create $instance
                JToken instanceRoot;
                if (!recognizerResult.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                {
                    instanceRoot = new JObject();
                    recognizerResult.Entities["$instance"] = instanceRoot;
                }

                // add instanceData
                JToken instanceData;
                if (!((JObject)instanceRoot).TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out instanceData))
                {
                    instanceData = new JArray();
                    instanceRoot[entityResult.Type] = instanceData;
                }

                dynamic instance = new JObject();
                instance.startIndex = entity.start;
                instance.endIndex = entity.end;
                instance.score = (double)1.0;
                instance.text = entity.text;
                instance.type = entity.type;
                instance.resolution = entity.resolution;
                ((JArray)instanceData).Add(instance);
            }

            // if no match return None intent
            if (!recognizerResult.Intents.Keys.Any())
            {
                recognizerResult.Intents.Add("None", new IntentScore() { Score = 1.0 });
            }

            await dialogContext.Context.TraceActivityAsync(nameof(RegexRecognizer), JObject.FromObject(recognizerResult), "RecognizerResult", "Regex RecognizerResult", cancellationToken).ConfigureAwait(false);

            this.TrackRecognizerResult(dialogContext, "RegexRecognizerResult", this.FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);

            return recognizerResult;
        }
    }
}
