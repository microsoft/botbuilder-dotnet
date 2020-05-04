using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

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
        public Recognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            if (!string.IsNullOrEmpty(callerPath))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange()
                {
                    Path = callerPath,
                    StartPoint = new SourcePoint() { LineIndex = callerLine, CharIndex = 0 },
                    EndPoint = new SourcePoint() { LineIndex = callerLine + 1, CharIndex = 0 },
                });
            }
        }

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
        public virtual Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
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
        public virtual async Task<T> RecognizeAsync<T>(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await this.RecognizeAsync(dialogContext, activity, cancellationToken).ConfigureAwait(false));
            return result;
        }

        /// <summary>
        /// Uses the RecognizerResult to create a list of propeties to be included when tracking the result in telemetry.
        /// </summary>
        /// <param name="recognizerResult">Recognizer Result.</param>
        /// <param name="telemetryProperties">A list of properties to append or override the properties created using the RecognizerResult.</param>
        /// <param name="dialogContext">Dialog Context.</param>
        /// <returns>A dictionary that can be included when calling the TrackEvent method on the TelemetryClient.</returns>
        protected virtual Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, DialogContext dialogContext = null)
        {
            var properties = new Dictionary<string, string>()
            {
                { "Text", recognizerResult.Text },
                { "AlteredText", recognizerResult.AlteredText },
                { "TopIntent", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Key : null },
                { "TopIntentScore", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Value?.ToString() : null },
                { "Intents", recognizerResult.Intents.Any() ? JsonConvert.SerializeObject(recognizerResult.Intents) : null },
                { "Entities", recognizerResult.Entities != null ? recognizerResult.Entities.ToString() : null },
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
            if (this.TelemetryClient is NullBotTelemetryClient)
            {
                var turnStateTelemetryClient = dialogContext.Context.TurnState.Get<IBotTelemetryClient>();
                this.TelemetryClient = turnStateTelemetryClient ?? this.TelemetryClient;
            }

            this.TelemetryClient.TrackEvent(eventName, telemetryProperties, telemetryMetrics);
        }
    }
}
