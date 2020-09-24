// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Interface for adding telemetry logging capabilities to <see cref="QnAMaker"/>.
    /// </summary>
    public interface ITelemetryQnAMaker
    {
        /// <summary>
        /// Gets a value indicating whether determines whether to log personal information that came from the user.
        /// </summary>
        /// <value>If true, will log personal information into the IBotTelemetryClient.TrackEvent method; otherwise the properties will be filtered.</value>
        bool LogPersonalInformation { get; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the QnaMessage event.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> being used to log events.</value>
        IBotTelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The options for the QnA Maker knowledge base. If null, constructor option is used for this instance.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the QnaMessage event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the QnaMessage event.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        Task<QueryResult[]> GetAnswersAsync(ITurnContext turnContext, QnAMakerOptions options, Dictionary<string, string> telemetryProperties, Dictionary<string, double> telemetryMetrics = null);
    }
}
