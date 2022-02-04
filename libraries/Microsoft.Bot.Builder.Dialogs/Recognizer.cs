// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Recognizer base class.
    /// </summary>
    /// <remarks>
    /// Recognizers operate in a DialogContext environment to recognize user input into Intents and Entities. 
    /// This class models 3 virtual methods around
    /// * Pure DialogContext (where the recognition happens against current state dialogcontext
    /// * Activity (where the recognition is from an Activity)
    /// * Text/Locale (where the recognition is from text/locale)
    /// The default implementation of DialogContext method is to use Context.Activity and call the activity method.
    /// The default implementation of Activity method is to filter to Message activities and pull out text/locale and call the text/locale method.
    /// </remarks>
    public class Recognizer
    {
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

        /// <summary>
        /// Gets or sets id of the recognizer.
        /// </summary>
        /// <value>Id.</value>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the currently configured <see cref="IBotTelemetryClient"/> that logs the RecognizerResult event.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> being used to log events.</value>
        [JsonIgnore]
        public IBotTelemetryClient TelemetryClient { get; set; } = new NullBotTelemetryClient();

        /// <summary>
        /// Runs current DialogContext.TurnContext.Activity through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="dialogContext">Dialog context.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>Analysis of utterance.</returns>
#pragma warning disable CA1068 // CancellationToken parameters must come last (we can't change this without breaking binary compat)
        public virtual Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
#pragma warning restore CA1068 // CancellationToken parameters must come last
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Runs current DialogContext.TurnContext.Activity through a recognizer and returns a strongly-typed recognizer result using IRecognizerConvert.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="dialogContext">Dialog context.</param>
        /// <param name="activity">activity to recognize.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>Analysis of utterance.</returns>
#pragma warning disable CA1068 // CancellationToken parameters must come last (we can't change this without breaking binary compat)
        public virtual async Task<T> RecognizeAsync<T>(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
#pragma warning restore CA1068 // CancellationToken parameters must come last
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeAsync(dialogContext, activity, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// CreateChooseIntentResult - returns ChooseIntent between multiple recognizer results.
        /// </summary>
        /// <param name="recognizerResults">recognizer Id => recognizer results map.</param>
        /// <returns>recognizerResult which is ChooseIntent.</returns>
        protected static RecognizerResult CreateChooseIntentResult(Dictionary<string, RecognizerResult> recognizerResults)
        {
            string text = null;
            var candidates = new List<JObject>();

            foreach (var recognizerResult in recognizerResults)
            {
                text = recognizerResult.Value.Text;
                var (intent, score) = recognizerResult.Value.GetTopScoringIntent();
                if (intent != NoneIntent)
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
                // return ChooseIntent with candidates array
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

        /// <summary>
        /// Uses the RecognizerResult to create a list of properties to be included when tracking the result in telemetry.
        /// </summary>
        /// <param name="recognizerResult">Recognizer Result.</param>
        /// <param name="telemetryProperties">A list of properties to append or override the properties created using the RecognizerResult.</param>
        /// <param name="dialogContext">Dialog Context.</param>
        /// <returns>A dictionary that can be included when calling the TrackEvent method on the TelemetryClient.</returns>
        protected virtual Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, DialogContext dialogContext = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "Text", recognizerResult.Text },
                { "AlteredText", recognizerResult.AlteredText },
                { "TopIntent", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Key : null },
                { "TopIntentScore", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Value?.Score?.ToString("N1", CultureInfo.InvariantCulture) : null },
                { "Intents", recognizerResult.Intents.Any() ? JsonConvert.SerializeObject(recognizerResult.Intents) : null },
                { "Entities", recognizerResult.Entities?.ToString() },
                { "AdditionalProperties", recognizerResult.Properties.Any() ? JsonConvert.SerializeObject(recognizerResult.Properties) : null },
            };

            // Additional Properties can override "stock" properties.
            if (telemetryProperties != null)
            {
                return telemetryProperties.Concat(properties)
                    .GroupBy(kv => kv.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return properties;
        }

        /// <summary>
        /// Tracks an event with the event name provided using the TelemetryClient attaching the properties / metrics.
        /// </summary>
        /// <param name="dialogContext">Dialog Context.</param>
        /// <param name="eventName">The name of the event to track.</param>
        /// <param name="telemetryProperties">The properties to be included as part of the event tracking.</param>
        /// <param name="telemetryMetrics">The metrics to be included as part of the event tracking.</param>
        protected void TrackRecognizerResult(DialogContext dialogContext, string eventName, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics)
        {
            if (TelemetryClient is NullBotTelemetryClient)
            {
                var turnStateTelemetryClient = dialogContext.Context.TurnState.Get<IBotTelemetryClient>();
                TelemetryClient = turnStateTelemetryClient ?? TelemetryClient;
            }

            TelemetryClient.TrackEvent(eventName, telemetryProperties, telemetryMetrics);
        }
    }
}
