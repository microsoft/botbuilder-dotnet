// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Client to access a QnA Maker knowledge base.
    /// </summary>
    public interface IQnAMakerClient
    {
        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The options for the QnA Maker knowledge base. If null, constructor option is used for this instance.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the QnaMessage event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the QnaMessage event.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        Task<QueryResult[]> GetAnswersAsync(
                                        ITurnContext turnContext,
                                        QnAMakerOptions options,
                                        Dictionary<string, string> telemetryProperties,
                                        Dictionary<string, double> telemetryMetrics = null);

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The options for the QnA Maker knowledge base. If null, constructor option is used for this instance.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the QnaMessage event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the QnaMessage event.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        Task<QueryResults> GetAnswersRawAsync(
                                        ITurnContext turnContext,
                                        QnAMakerOptions options,
                                        Dictionary<string, string> telemetryProperties = null,
                                        Dictionary<string, double> telemetryMetrics = null);

        /// <summary>
        /// Filters the ambiguous question for active learning.
        /// </summary>
        /// <param name="queryResult">User query output.</param>
        /// <returns>Filtered array of ambiguous question.</returns>
        QueryResult[] GetLowScoreVariation(QueryResult[] queryResult);

        /// <summary>
        /// Send feedback to the knowledge base.
        /// </summary>
        /// <param name="feedbackRecords">Feedback records.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CallTrainAsync(FeedbackRecords feedbackRecords);
    }
}
