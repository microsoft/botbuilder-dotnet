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
    /// CrossTrainedRecognizerSet - InputRecognizer for selecting between cross trained recognizers.
    /// </summary>
    /// <remarks>
    /// InputRecognizer implementation which calls multiple recognizers that are cross trained and uses
    /// Intents called $"DefersToRecognizer_{Id}" to represent a cross-trained intent for another recognizer.
    /// 
    /// If there is consensus among the cross trained recognizers, the recognizerResult structure from
    /// the consensus recognizer is returned.
    /// 
    /// In the case that there is conflicting or ambigious signals from the recognizers then an 
    /// intent of "AmbigiousIntent" will be returned with the results of all of the recognizers.
    /// </remarks>
    public class CrossTrainedRecognizerSet : InputRecognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.CrossTrainedRecognizerSet";

        public const string DeferPrefix = "DeferToRecognizer_";
        public const string AmbigiousIntent = "AmbigiousIntent";
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
        public List<InputRecognizer> Recognizers { get; set; } = new List<InputRecognizer>();

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

            if (this.Recognizers.Any(recognizer => string.IsNullOrEmpty(recognizer.Id)))
            {
                throw new ArgumentNullException("RecognizerSet requires that Recognizers in the set have an .Id set");
            }

            // run all of the recognizers in parallel
            var results = await Task.WhenAll(Recognizers.Select(r => r.RecognizeAsync(dialogContext, text, locale, cancellationToken)));

            // put results into a dictionary for easier lookup while processing.
            Dictionary<string, RecognizerResult> recognizerResults = new Dictionary<string, RecognizerResult>(System.StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> intents = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            for (int iRecognizer = 0; iRecognizer < Recognizers.Count; iRecognizer++)
            {
                var recognizer = Recognizers[iRecognizer];
                var result = results[iRecognizer];
                recognizerResults[recognizer.Id] = result;
                var (topIntent, score) = result.GetTopScoringIntent();
                intents[recognizer.Id] = topIntent;
            }

            // this is the consensusRecognizer to use
            string consensusRecognizerId = null;
            foreach (var recognizer in Recognizers)
            {
                var intent = intents[recognizer.Id];

                if (!IsRedirect(intent))
                {
                    // we have a real intent and it's the first one we found.
                    if (consensusRecognizerId == null)
                    {
                        consensusRecognizerId = recognizer.Id;
                    }
                    else
                    {
                        // we have a second recognizer with an intent

                        // if one of them is None intent, then go with the other one.
                        if (intent == NoneIntent)
                        {
                            // then we are fine with the one we have, just ignore this one
                            continue;
                        }
                        else if (intents[consensusRecognizerId] == "None")
                        {
                            // then we can drop the old one and go with the new one instead
                            consensusRecognizerId = recognizer.Id;
                        }
                        else
                        {
                            // ambigious because of 2 real intents, and neither are None so return AmbigiousIntent
                            return CreateAmbigiousIntentResult(text, recognizerResults);
                        }
                    }
                }
                else
                {
                    // get the redirectId and redirectIntent 
                    var redirectId = GetRedirectId(intent);
                    var redirectIntent = intents[redirectId];

                    // if the redirectIntent is itself a redirect, then we have double redirect which means disagreement.
                    if (IsRedirect(redirectIntent))
                    {
                        // we have ambiguity, return AmbigiousIntent
                        return CreateAmbigiousIntentResult(text, recognizerResults);
                    }
                }
            }

            // we have consensus for consensusRecognizer, return the results of that recognizer as the result.
            return recognizerResults[consensusRecognizerId];
        }

        private RecognizerResult CreateAmbigiousIntentResult(string text, Dictionary<string, RecognizerResult> recognizerResults)
        {
            // create IntentScore with { "recognizerId" : { ...RecognizerResult.. } }
            var ambigiousScore = new IntentScore()
            {
                Score = 0.5F
            };

            foreach (var result in recognizerResults)
            {
                ambigiousScore.Properties[result.Key] = result.Value;
            }

            return new RecognizerResult()
            {
                Text = text,
                Intents = new Dictionary<string, IntentScore>()
                {
                    { AmbigiousIntent, (IntentScore)ambigiousScore }
                }
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
    }
}
