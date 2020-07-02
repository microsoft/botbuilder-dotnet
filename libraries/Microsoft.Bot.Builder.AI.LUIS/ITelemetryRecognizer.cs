// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Recognizer with Telemetry support.
    /// </summary>
    public interface ITelemetryRecognizer : IRecognizer
    {
        /// <summary>
        /// Gets a value indicating whether determines whether to log personal information that came from the user.
        /// </summary>
        /// <value>If true, will log personal information into the IBotTelemetryClient.TrackEvent method; otherwise the properties will be filtered.</value>
        bool LogPersonalInformation { get; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the LuisResult event.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> being used to log events.</value>
        IBotTelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Return results of the analysis (suggested intents and entities) using the turn context.
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The LUIS results of the analysis of the current message text in the current turn's context activity.</returns>
        Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="telemetryProperties">Dictionary containing additional properties to be attached to the outgoing telemetry item.</param>
        /// <param name="telemetryMetrics">Dictionary containing additional metrics to be attached to the outgoing telemetry item.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        Task<T> RecognizeAsync<T>(ITurnContext turnContext, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics, CancellationToken cancellationToken = default)
            where T : IRecognizerConvert, new();

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analysis of utterance.</returns>
        new Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken = default)
            where T : IRecognizerConvert, new();
    }
}
