using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected virtual Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, ITurnContext turnContext = null)
        {
            if (telemetryProperties == null)
            {
                telemetryProperties = new Dictionary<string, string>();
            }

            telemetryProperties.Add("Text", recognizerResult.Text);
            telemetryProperties.Add("AlteredText", recognizerResult.AlteredText);
            telemetryProperties.Add("TopIntent", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Key : null);
            telemetryProperties.Add("TopIntentScore", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Value?.ToString() : null);
            telemetryProperties.Add("Intents", recognizerResult.Intents.Any() ? JsonConvert.SerializeObject(recognizerResult.Intents) : null);
            telemetryProperties.Add("Entities", recognizerResult.Entities != null ? recognizerResult.Entities.ToString() : null);
            telemetryProperties.Add("AdditionalProperties", recognizerResult.Properties.Any() ? JsonConvert.SerializeObject(recognizerResult.Properties) : null);

            return telemetryProperties;
        }
    }
}
