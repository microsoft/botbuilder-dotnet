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

            var entities = new JObject();
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
                                JToken values;
                                if (!entities.TryGetValue(groupName, out values))
                                {
                                    values = new JArray();
                                    entities.Add(groupName, values);
                                }

                                ((JArray)values).Add(group.Value);

                                // get/create $instance
                                JToken instanceRoot;
                                if (!entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                                {
                                    instanceRoot = new JObject();
                                    entities["$instance"] = instanceRoot;
                                }

                                // add instanceData
                                JToken instanceData;
                                if (!((JObject)instanceRoot).TryGetValue(groupName, StringComparison.OrdinalIgnoreCase, out instanceData))
                                {
                                    instanceData = new JArray();
                                    instanceRoot[groupName] = instanceData;
                                }

                                dynamic instance = new JObject();
                                instance.startIndex = group.Index;
                                instance.endIndex = group.Index + group.Length;
                                instance.score = (double)1.0;
                                instance.text = (string)group.Value;
                                instance.type = groupName;
                                ((JArray)instanceData).Add(instance);
                            }
                        }
                    }

                    // found
                    break;
                }
            }

            if (this.Entities != null)
            {
                EntityRecognizerSet entitySet = new EntityRecognizerSet(this.Entities);
                IList<Entity> entities2 = new List<Entity>();
                entities2 = await entitySet.RecognizeEntities(dialogContext, text, locale, entities2).ConfigureAwait(false);
                foreach (var entity in entities2)
                {
                    // add value
                    JToken values;
                    if (!entities.TryGetValue(entity.Type, StringComparison.OrdinalIgnoreCase, out values))
                    {
                        values = new JArray();
                        entities[entity.Type] = values;
                    }

                    ((JArray)values).Add((string)entity.Properties["Text"]);

                    // get/create $instance
                    JToken instanceRoot;
                    if (!entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out instanceRoot))
                    {
                        instanceRoot = new JObject();
                        entities["$instance"] = instanceRoot;
                    }

                    // add instanceData
                    JToken instanceData;
                    if (!((JObject)instanceRoot).TryGetValue(entity.Type, StringComparison.OrdinalIgnoreCase, out instanceData))
                    {
                        instanceData = new JArray();
                        instanceRoot[entity.Type] = instanceData;
                    }

                    dynamic instance = new JObject();
                    instance.startIndex = entity.Properties["Start"];
                    instance.endIndex = entity.Properties["End"];
                    instance.score = (double)1.0;
                    instance.text = (string)entity.Properties["Text"];
                    instance.type = entity.Type;
                    instance.resolution = entity.Properties["Resolution"];
                    ((JArray)instanceData).Add(instance);
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
    }
}
