// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// CrossTrainedRecognizerSet - Recognizer for selecting between cross trained recognizers.
    /// </summary>
    /// <remarks>
    /// Recognizer implementation which calls multiple recognizers that are cross trained with intents
    /// that model deferring to another recognizer. Each recognizer should have intents
    /// with special intent name pattern $"DefersToRecognizer_{Id}" to represent a cross-trained
    /// intent for another recognizer.
    ///
    /// If there is consensus among the cross trained recognizers, the recognizerResult structure from
    /// the consensus recognizer is returned.
    ///
    /// In the case that there is conflicting or ambiguous signals from the recognizers then an
    /// intent of "ChooseIntent" will be returned with the results of all of the recognizers.
    /// </remarks>
    public class CrossTrainedRecognizerSet : AdaptiveRecognizer
    {
        /// <summary>
        /// Class idenfifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.CrossTrainedRecognizerSet";

        /// <summary>
        /// Standard cross trained intent name prefix.
        /// </summary>
        public const string DeferPrefix = "DeferToRecognizer_";

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossTrainedRecognizerSet"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public CrossTrainedRecognizerSet([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the input recognizers.
        /// </summary>
        /// <value>
        /// The input recognizers.
        /// </value>
        [JsonProperty("recognizers")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Recognizer> Recognizers { get; set; } = new List<Recognizer>();
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
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (Recognizers.Any() == false)
            {
                return new RecognizerResult()
                {
                    Intents = new Dictionary<string, IntentScore>() { { NoneIntent, new IntentScore() { Score = 1.0 } } }
                };
            }

            EnsureRecognizerIds();

            // run all of the recognizers in parallel
            var results = await Task.WhenAll(Recognizers.Select(async recognizer =>
            {
                var result = await recognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                result.Properties["id"] = recognizer.Id;
                return result;
            })).ConfigureAwait(false);

            var result = ProcessResults(results);

            TrackRecognizerResult(dialogContext, "CrossTrainedRecognizerSetResult", FillRecognizerResultTelemetryProperties(result, telemetryProperties, dialogContext), telemetryMetrics);

            return result;
        }

        private RecognizerResult ProcessResults(IEnumerable<RecognizerResult> results)
        {
            // put results into a dictionary for easier lookup while processing.
            var recognizerResults = results.ToDictionary(result => (string)result.Properties["id"], System.StringComparer.OrdinalIgnoreCase);
            var intents = results.ToDictionary(result => (string)result.Properties["id"], result => result.GetTopScoringIntent().intent, System.StringComparer.OrdinalIgnoreCase);

            // this is the consensusRecognizer to use
            string consensusRecognizerId = null;
            foreach (var recognizer in Recognizers)
            {
                var recognizerId = recognizer.Id;
                var intent = intents[recognizer.Id];

                if (IsRedirect(intent))
                {
                    // follow redirect and see where it takes us
                    recognizerId = GetRedirectId(intent);
                    intent = intents[recognizerId];
                    while (recognizerId != recognizer.Id && IsRedirect(intent))
                    {
                        recognizerId = GetRedirectId(intent);
                        intent = intents[recognizerId];
                    }

                    // if we ended up back at the recognizer.id and we have no consensensus then it's a none intent
                    if (recognizerId == recognizer.Id && consensusRecognizerId == null)
                    {
                        // circular redirects, just return a none intent
                        return new RecognizerResult()
                        {
                            Text = recognizerResults[recognizer.Id].Text,
                            Intents = new Dictionary<string, IntentScore>() { { NoneIntent, new IntentScore() { Score = 1.0 } } }
                        };
                    }
                }

                // we have a real intent and it's the first one we found.
                if (consensusRecognizerId == null)
                {
                    if (intent != NoneIntent && !string.IsNullOrEmpty(intent))
                    {
                        consensusRecognizerId = recognizerId;
                    }
                }
                else
                {
                    // we have a second recognizer result which is either none or real

                    // if one of them is None intent, then go with the other one.
                    if (intent == NoneIntent || string.IsNullOrEmpty(intent))
                    {
                        // then we are fine with the one we have, just ignore this one
                        continue;
                    }
                    else if (recognizerId == consensusRecognizerId)
                    {
                        // this is more consensus for this recgonizer
                        continue;
                    }
                    else
                    {
                        // ambiguous because we have 2 or more real intents, so return ChooseIntent, filter out redirect results and return ChooseIntent
                        var recognizersWithRealIntents = recognizerResults
                            .Where(kv => !IsRedirect(kv.Value.GetTopScoringIntent().intent))
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                        return CreateChooseIntentResult(recognizersWithRealIntents);
                    }
                }
            }

            // we have consensus for consensusRecognizer, return the results of that recognizer as the result.
            if (consensusRecognizerId != null)
            {
                return recognizerResults[consensusRecognizerId];
            }

            //find if there is missing entities matched
            var mergedEntities = new JObject();
            foreach (var rocogResult in results)
            {
                if (rocogResult.Entities.Count > 0)
                {
                    mergedEntities.Merge(rocogResult.Entities);
                }
            }

            // return none.
            return new RecognizerResult()
            {
                Text = recognizerResults.Values.First().Text,
                Intents = new Dictionary<string, IntentScore>() { { NoneIntent, new IntentScore() { Score = 1.0 } } },
                Entities = mergedEntities
            };
        }

        private bool IsRedirect(string intent)
        {
            return intent.StartsWith(DeferPrefix, StringComparison.Ordinal);
        }

        private string GetRedirectId(string intent)
        {
            return intent.Substring(DeferPrefix.Length);
        }

        private void EnsureRecognizerIds()
        {
            if (Recognizers.Any(recognizer => string.IsNullOrEmpty(recognizer.Id)))
            {
                throw new InvalidOperationException("This recognizer requires that each recognizer in the set have an .Id value.");
            }
        }
    }
}
