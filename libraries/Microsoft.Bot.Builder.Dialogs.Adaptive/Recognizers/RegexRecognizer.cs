﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class RegexRecognizer : AdaptiveRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RegexRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public RegexRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets intent patterns for recognizing intents using regular expressions.
        /// </summary>
        /// <value>
        /// Dictionary of patterns -> Intent names.
        /// </value>
        [JsonProperty("intents")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<IntentPattern> Intents { get; set; } = new List<IntentPattern>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the entity recognizers.
        /// </summary>
        /// <value>
        /// The entity recognizers.
        /// </value>
        [JsonProperty("entities")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<EntityRecognizer> Entities { get; set; } = new List<EntityRecognizer>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Runs current DialogContext.TurnContext.Activity through a recognizer and returns a <see cref="RecognizerResult"/>.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="activity"><see cref="Activity"/> to recognize.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <param name="telemetryProperties">Optional, additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Optional, additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>Analysis of utterance.</returns>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            // Identify matched intents
            var text = activity.Text ?? string.Empty;
            var locale = activity.Locale ?? "en-us";

            var recognizerResult = new RecognizerResult()
            {
                Text = text,
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                // nothing to recognize, return empty recognizerResult
                return recognizerResult;
            }

            // add entities from regexrecgonizer to the entities pool
            var entityPool = new List<Entity>();

            var textEntity = new TextEntity(text);
            textEntity.Properties["start"] = 0;
            textEntity.Properties["end"] = text.Length;
            textEntity.Properties["score"] = 1.0;

            entityPool.Add(textEntity);

            foreach (var intentPattern in Intents)
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
                }
            }

            if (Entities != null)
            {
                // process entities using EntityRecognizerSet
                var entitySet = new EntityRecognizerSet(Entities);
                var newEntities = await entitySet.RecognizeEntitiesAsync(dialogContext, text, locale, entityPool).ConfigureAwait(false);
                if (newEntities.Any())
                {
                    entityPool.AddRange(newEntities);
                }
            }

            // map entityPool of Entity objects => RecognizerResult entity format
            recognizerResult.Entities = new JObject();

            foreach (var entityResult in entityPool.Where(entity => entity != textEntity))
            {
                // add value
                if (!recognizerResult.Entities.TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out var values))
                {
                    values = new JArray();
                    recognizerResult.Entities[entityResult.Type] = values;
                }

                // The Entity type names are not consistent, map everything to camelcase so we can process them cleaner.
                dynamic entity = JObject.FromObject(entityResult);
                ((JArray)values).Add(entity.text);

                // get/create $instance
                if (!recognizerResult.Entities.TryGetValue("$instance", StringComparison.OrdinalIgnoreCase, out var instanceRoot))
                {
                    instanceRoot = new JObject();
                    recognizerResult.Entities["$instance"] = instanceRoot;
                }

                // add instanceData
                if (!((JObject)instanceRoot).TryGetValue(entityResult.Type, StringComparison.OrdinalIgnoreCase, out var instanceData))
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

            TrackRecognizerResult(dialogContext, TelemetryLoggerConstants.RegexRecognizerResultEvent, FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dialogContext), telemetryMetrics);

            return recognizerResult;
        }
    }
}
