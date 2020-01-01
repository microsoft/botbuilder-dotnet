// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.NumberWithUnit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// RecognizerSet - Recognizer which is the union of of multiple recognizers into one RecognizerResult.
    /// </summary>
    /// <remarks>
    /// Intents will be merged by picking the intent with the MaxScore.
    /// Entities are merged as a simple union of all of the Entities.
    /// </remarks>
    public class RecognizerSet : Recognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.RecognizerSet";

        [JsonConstructor]
        public RecognizerSet()
        {
        }

        /// <summary>
        /// Gets or sets the input recognizers.
        /// </summary>
        /// <value>
        /// The input recognizers.
        /// </value>
        [JsonProperty("recognizers")]
        public List<Recognizer> Recognizers { get; set; } = new List<Recognizer>();

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, CancellationToken cancellationToken = default)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            // run all of the recognizers in parallel
            var results = await Task.WhenAll(Recognizers.Select(r => r.RecognizeAsync(dialogContext, cancellationToken)));

            // merge intents
            return MergeResults(results);
        }

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            // run all of the recognizers in parallel
            var results = await Task.WhenAll(Recognizers.Select(r => r.RecognizeAsync(dialogContext, activity, cancellationToken)));

            // merge intents
            return MergeResults(results);
        }

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, string text, string locale = null, CancellationToken cancellationToken = default)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (locale == null)
            {
                locale = string.Empty;
            }

            // run all of the recognizers in parallel
            var results = await Task.WhenAll(Recognizers.Select(r => r.RecognizeAsync(dialogContext, text, locale, cancellationToken)));

            // merge intents
            return MergeResults(results);
        }

        private RecognizerResult MergeResults(RecognizerResult[] results)
        {
            var recognizerResult = new RecognizerResult();
            JObject instanceData = new JObject();
            recognizerResult.Entities["$instance"] = instanceData;

            foreach (var result in results)
            {
                var (intent, score) = result.GetTopScoringIntent();
                if (intent != "None")
                {
                    // merge text
                    if (recognizerResult.Text == null)
                    {
                        recognizerResult.Text = result.Text;
                    }
                    else if (result.Text != recognizerResult.Text)
                    {
                        recognizerResult.AlteredText = result.Text;
                    }

                    // merge intents
                    foreach (var intentPair in result.Intents)
                    {
                        if (recognizerResult.Intents.TryGetValue(intentPair.Key, out var prevScore))
                        {
                            if (intentPair.Value.Score < prevScore.Score)
                            {
                                continue; // we already have a higher score for this intent
                            }
                        }

                        recognizerResult.Intents[intentPair.Key] = intentPair.Value;
                    }
                }

                // merge entities
                // entities shape is:
                //   { 
                //      "name": ["value1","value2","value3"], 
                //      "$instance": {
                //          "name": [ { "startIndex" : 15, ... }, ... ] 
                //      }
                //   }
                foreach (var entityProperty in result.Entities.Properties())
                    {
                        if (entityProperty.Name == "$instance")
                        {
                            // property is "$instance" so get the instance data
                            JObject resultInstanceData = (JObject)entityProperty.Value;
                            foreach (var name in resultInstanceData.Properties())
                            {
                                // merge sourceInstanceData[name] => instanceData[name]
                                MergeArrayProperty(resultInstanceData, name, instanceData);
                            }
                        }
                        else
                        {
                            // property is a "name" with values, 
                            // merge result.Entities["name"] => recognizerResult.Entities["name"]
                            MergeArrayProperty(result.Entities, entityProperty, recognizerResult.Entities);
                        }
                    }
            }

            if (!recognizerResult.Intents.Any())
            {
                recognizerResult.Intents.Add("None", new IntentScore() { Score = 1.0d });
            }

            return recognizerResult;
        }

        private void MergeArrayProperty(JObject sourceObject, JProperty property, JObject targetObject)
        {
            // get elements from source object
            var elements = (JArray)property.Value;
            foreach (var element in elements)
            {
                if (!targetObject.TryGetValue(property.Name, out JToken target))
                {
                    target = new JArray();
                    targetObject[property.Name] = target;
                }

                ((JArray)target).Add(element);
            }
        }
    }
}
