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
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizer implementation which uses regex expressions to identify intents.
    /// </summary>
    public class RegexRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.RegexRecognizer";

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

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, string text, string locale, CancellationToken cancellationToken)
        {
            // Identify matched intents
            text = text ?? string.Empty;

            var result = new RecognizerResult()
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
                    if (!result.Intents.ContainsKey(intentKey))
                    {
                        result.Intents.Add(intentKey, new IntentScore() { Score = 1.0 });
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
            result.Entities = new JObject();
            
            foreach (var entityResult in entityPool)
            {
                // add value
                JToken values;
                if (!result.Entities.TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out values))
                {
                    values = new JArray();
                    result.Entities[entityResult.Type] = values;
                }

                // The Entity type names are not consistent, map everything to camelcase so we can process them cleaner.
                dynamic entity = JObject.FromObject(entityResult);
                ((JArray)values).Add(entity.text);

                // get/create $instance
                JToken instanceRoot;
                if (!result.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                {
                    instanceRoot = new JObject();
                    result.Entities["$instance"] = instanceRoot;
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
            if (!result.Intents.Keys.Any())
            {
                result.Intents.Add("None", new IntentScore() { Score = 1.0 });
            }

            return result;
        }
    }
}
