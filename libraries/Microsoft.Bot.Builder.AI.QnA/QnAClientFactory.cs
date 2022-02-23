// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using Microsoft.Bot.Builder.AI.QnA.Models;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAClient Factory for creating an object that implements <see cref="IQnAMakerClient"/>.
    /// </summary>
    public static class QnAClientFactory
    {
        /// <summary>
        /// Gets an <see cref="IQnAMakerClient"/> to use to access the QnA Maker knowledge base or Custom Question Answering.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used for logging telemetry events.</param>
        /// <param name="logPersonalInformation">Set to true to include personally identifiable information in telemetry events.</param>
        /// <returns>An <see cref="IQnAMakerClient"/>.</returns>
        public static IQnAMakerClient CreateQnAClient(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
        {
            if (endpoint.QnAServiceType == Constants.LanguageQnAServiceType)
            {
                return new CustomQuestionAnswering(endpoint, options, httpClient, telemetryClient, logPersonalInformation);
            }
            else
            {
                return new QnAMaker(endpoint, options, httpClient, telemetryClient, logPersonalInformation);
            }
        }
    }
}
