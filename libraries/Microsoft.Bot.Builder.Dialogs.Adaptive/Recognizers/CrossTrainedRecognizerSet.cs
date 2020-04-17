// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming.Payloads;
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
    /// In the case that there is conflicting or ambigious signals from the recognizers then an 
    /// intent of "ChooseIntent" will be returned with the results of all of the recognizers.
    /// </remarks>
    public class CrossTrainedRecognizerSet : Recognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.CrossTrainedRecognizerSet";

        /// <summary>
        /// Standard cross trained intent name prefix.
        /// </summary>
        public const string DeferPrefix = "DeferToRecognizer_";

        /// <summary>
        /// Intent name that will be produced by this recognizer if the child recognizers do not have consensus for intents.
        /// </summary>
        public const string ChooseIntent = "ChooseIntent";

        /// <summary>
        /// Standard none intent that means none of the recognizers recognize the intent.
        /// </summary>
        /// <remarks>
        /// If each recognizer returns no intents or None intents, then this recognizer will return None intent.
        /// </remarks>
        public const string NoneIntent = "None";

        [JsonConstructor]
        public CrossTrainedRecognizerSet()
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

            EnsureRecognizerIds();

            // run all of the recognizers in parallel
            var results = await Task.WhenAll(Recognizers.Select(async recognizer =>
            {
                var result = await recognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
                result.Properties["id"] = recognizer.Id;
                return result;
            }));

            var result = ProcessResults(results);

            this.TrackRecognizerResult(dialogContext, "CrossTrainedRecognizerSetResult", this.FillRecognizerResultTelemetryProperties(result, telemetryProperties), telemetryMetrics);

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
                    if (intent != NoneIntent)
                    {
                        consensusRecognizerId = recognizerId;
                    }
                }
                else
                {
                    // we have a second recognizer result which is either none or real

                    // if one of them is None intent, then go with the other one.
                    if (intent == NoneIntent)
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
                        // ambigious because we have 2 or more real intents, so return ChooseIntent
                        return CreateChooseIntentResult(recognizerResults);
                    }
                }
            }

            // we have consensus for consensusRecognizer, return the results of that recognizer as the result.
            if (consensusRecognizerId != null)
            {
                return recognizerResults[consensusRecognizerId];
            }

            // return none.
            return new RecognizerResult()
            {
                Text = recognizerResults.Values.First().Text,
                Intents = new Dictionary<string, IntentScore>() { { NoneIntent, new IntentScore() { Score = 1.0 } } }
            };
        }

        private RecognizerResult CreateChooseIntentResult(Dictionary<string, RecognizerResult> recognizerResults)
        {
            string text = null;
            List<JObject> candidates = new List<JObject>();

            foreach (var recognizerResult in recognizerResults)
            {
                text = recognizerResult.Value.Text;
                var (intent, score) = recognizerResult.Value.GetTopScoringIntent();
                if (!IsRedirect(intent) && intent != NoneIntent)
                {
                    dynamic candidate = new JObject();
                    candidate.id = recognizerResult.Key;
                    candidate.intent = intent;
                    candidate.score = score;
                    candidate.result = JObject.FromObject(recognizerResult.Value);
                    candidates.Add(candidate);
                }
            }

            if (candidates.Any())
            {
                // return ChooseIntent with Candidtes array
                return new RecognizerResult()
                {
                    Text = text,
                    Intents = new Dictionary<string, IntentScore>() { { ChooseIntent, new IntentScore() { Score = 1.0 } } },
                    Properties = new Dictionary<string, object>() { { "candidates", candidates } },
                };
            }

            // just return a none intent
            return new RecognizerResult()
            {
                Text = text,
                Intents = new Dictionary<string, IntentScore>() { { NoneIntent, new IntentScore() { Score = 1.0 } } }
            };
        }

        private bool IsRedirect(string intent)
        {
            return intent.StartsWith(DeferPrefix);
        }

        private string GetRedirectId(string intent)
        {
            return intent.Substring(DeferPrefix.Length);
        }

        private void EnsureRecognizerIds()
        {
            if (this.Recognizers.Any(recognizer => string.IsNullOrEmpty(recognizer.Id)))
            {
                throw new ArgumentNullException("This recognizer requires that each recognizer in the set have an .Id value.");
            }
        }
    }
}
