// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA.Utils;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Provides access to a Custom Question Answering Knowledge Base.
    /// </summary>
    public class CustomQuestionAnswering : IQnAMakerClient, ITelemetryQnAMaker
    {
        private readonly HttpClient _httpClient;

        private readonly QnAMakerEndpoint _endpoint;

        private readonly LanguageServiceUtils _languageServiceHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQuestionAnswering"/> class.
        /// </summary>
        /// <param name="endpoint">The <see cref="QnAMakerEndpoint"/> of the knowledge base to query.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base.</param>
        /// <param name="httpClient">An alternate client with which to talk to Language Service.
        /// If null, a default client is used for this instance.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used for logging telemetry events.</param>
        /// <param name="logPersonalInformation">Set to true to include personally identifiable information in telemetry events.</param>
        public CustomQuestionAnswering(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
            : this(endpoint, telemetryClient, logPersonalInformation, httpClient)
        {
            if (string.IsNullOrEmpty(endpoint.EndpointKey))
            {
                throw new ArgumentException(nameof(endpoint.EndpointKey));
            }

            _languageServiceHelper = new LanguageServiceUtils(endpoint, options, _httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQuestionAnswering"/> class.
        /// </summary>
        /// <param name="endpoint">The <see cref="QnAMakerEndpoint"/> of the knowledge base to query.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base.</param>
        /// <param name="httpClient">An alternate client with which to talk to Language Service.
        /// If null, a default client is used for this instance.</param>
        public CustomQuestionAnswering(QnAMakerEndpoint endpoint, QnAMakerOptions options = null, HttpClient httpClient = null)
            : this(endpoint, options, httpClient, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQuestionAnswering"/> class.
        /// </summary>
        /// <param name="managedIdentityClientId">The ClientId of the Managed Identity resource.</param>
        /// <param name="endpoint">The <see cref="QnAMakerEndpoint"/> of the knowledge base to query.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base.</param>
        /// <param name="httpClient">An alternate client with which to talk to Language Service.
        /// If null, a default client is used for this instance.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used for logging telemetry events.</param>
        /// <param name="logPersonalInformation">Set to true to include personally identifiable information in telemetry events.</param>
        public CustomQuestionAnswering(string managedIdentityClientId, QnAMakerEndpoint endpoint, QnAMakerOptions options = null, HttpClient httpClient = null, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
            : this(endpoint, telemetryClient, logPersonalInformation, httpClient)
        {
            _languageServiceHelper = new LanguageServiceUtils(managedIdentityClientId, endpoint, options, _httpClient);
        }

        internal CustomQuestionAnswering(QnAMakerEndpoint endpoint, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false, HttpClient httpClient = null)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

            if (string.IsNullOrEmpty(endpoint.KnowledgeBaseId))
            {
                throw new ArgumentException(nameof(endpoint.KnowledgeBaseId));
            }

            if (string.IsNullOrEmpty(endpoint.Host))
            {
                throw new ArgumentException(nameof(endpoint.Host));
            }

            if (_endpoint.Host.EndsWith("v2.0", StringComparison.Ordinal) || _endpoint.Host.EndsWith("v3.0", StringComparison.Ordinal))
            {
                throw new NotSupportedException("v2.0 and v3.0 of QnA Maker service is no longer supported in the QnA Maker.");
            }

            _httpClient = httpClient ?? DefaultHttpClient;

            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;
        }

        internal CustomQuestionAnswering(CustomQuestionAnsweringClient client, QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient = null, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _httpClient = httpClient ?? DefaultHttpClient;
            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;
            _languageServiceHelper = new LanguageServiceUtils(client, endpoint, options);
        }

        /// <summary>
        /// Gets the <see cref="HttpClient"/> to be used when calling the Custom Question Answering API.
        /// </summary>
        /// <value>
        /// A instance of <see cref="HttpClient"/>.
        /// </value>
        public static HttpClient DefaultHttpClient { get; } = new HttpClient();

        /// <summary>
        /// Gets a value indicating whether to log personal information that came from the user.
        /// </summary>
        /// <value>If true, will log personal information into the IBotTelemetryClient.TrackEvent method; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the QnaMessage event.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> being used to log events.</value>
        [JsonIgnore]
        public IBotTelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The <see cref="TurnContext"/> that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base. If null, constructor option is used for this instance.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public Task<QueryResult[]> GetAnswersAsync(ITurnContext turnContext, QnAMakerOptions options = null)
        {
            return GetAnswersAsync(turnContext, options, null);
        }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The <see cref="TurnContext"/> that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base. If null, constructor option is used for this instance.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the QnaMessage event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the QnaMessage event.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public async Task<QueryResult[]> GetAnswersAsync(
                                        ITurnContext turnContext,
                                        QnAMakerOptions options,
                                        Dictionary<string, string> telemetryProperties,
                                        Dictionary<string, double> telemetryMetrics = null)
        {
            var result = await GetAnswersRawAsync(turnContext, options, telemetryProperties, telemetryMetrics).ConfigureAwait(false);

            return result.Answers;
        }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The <see cref="TurnContext"/> that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base. If null, constructor option is used for this instance.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the QnaMessage event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the QnaMessage event.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public async Task<QueryResults> GetAnswersRawAsync(
                                        ITurnContext turnContext,
                                        QnAMakerOptions options,
                                        Dictionary<string, string> telemetryProperties = null,
                                        Dictionary<string, double> telemetryMetrics = null)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new ArgumentException($"The {nameof(turnContext.Activity)} property for {nameof(turnContext)} can't be null.", nameof(turnContext));
            }

            var messageActivity = turnContext.Activity.AsMessageActivity();
            if (messageActivity == null)
            {
                throw new ArgumentException("Activity type is not a message");
            }

            if (string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                throw new ArgumentException("Question cannot be null or empty text");
            }

            var results = await _languageServiceHelper.QueryKnowledgeBaseAsync(turnContext, messageActivity, options).ConfigureAwait(false);

            await OnQnaResultsAsync(results.Answers, turnContext, telemetryProperties, telemetryMetrics, CancellationToken.None).ConfigureAwait(false);

            return results;
        }

        /// <summary>
        /// Filters the ambiguous question for active learning.
        /// </summary>
        /// <param name="queryResult">An array of <see cref="QueryResult"/> which is the user query output.</param>
        /// <returns>Filtered array of ambiguous question.</returns>
        public QueryResult[] GetLowScoreVariation(QueryResult[] queryResult)
        {
            return ActiveLearningUtils.GetLowScoreVariation(queryResult.ToList()).ToArray();
        }

        /// <summary>
        /// Send feedback to the knowledge base.
        /// </summary>
        /// <param name="feedbackRecords">An object containing an array of <see cref="FeedbackRecord"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CallTrainAsync(FeedbackRecords feedbackRecords)
        {
            await _languageServiceHelper.UpdateActiveLearningFeedbackAsync(feedbackRecords).ConfigureAwait(false);
        }

        /// <summary>
        /// Fills the event properties and metrics for the QnaMessage event for telemetry.
        /// These properties are logged when the QnA GetAnswers method is called.
        /// </summary>
        /// <param name="queryResults">An array of <see cref="QueryResult"/> which is the user query output.</param>
        /// <param name="turnContext"><see cref="TurnContext"/> object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Properties to add/override for the event.</param>
        /// <param name="telemetryMetrics">Metrics to add/override for the event.</param>
        /// additionalProperties
        /// <returns>A tuple of Properties and Metrics that will be sent to the IBotTelemetryClient.TrackEvent method for the QnAMessage event.  The properties and metrics returned the standard properties logged with any properties passed from the GetAnswersAsync method.</returns>
        internal Task<(Dictionary<string, string> Properties, Dictionary<string, double> Metrics)> FillQnAEventAsync(QueryResult[] queryResults, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var properties = new Dictionary<string, string>();
            var metrics = new Dictionary<string, double>();

            properties.Add(QnATelemetryConstants.KnowledgeBaseIdProperty, _endpoint.KnowledgeBaseId);

            var text = turnContext.Activity.Text;
            var userName = turnContext.Activity.From?.Name;

            // Use the LogPersonalInformation flag to toggle logging PII data, text and user name are common examples
            if (LogPersonalInformation)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    properties.Add(QnATelemetryConstants.QuestionProperty, text);
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    properties.Add(QnATelemetryConstants.UsernameProperty, userName);
                }
            }

            // Fill in QnA Results (found or not)
            if (queryResults.Length > 0)
            {
                var queryResult = queryResults[0];
                properties.Add(QnATelemetryConstants.MatchedQuestionProperty, JsonConvert.SerializeObject(queryResult.Questions, new JsonSerializerSettings { MaxDepth = null }));
                properties.Add(QnATelemetryConstants.QuestionIdProperty, queryResult.Id.ToString(CultureInfo.InvariantCulture));
                properties.Add(QnATelemetryConstants.AnswerProperty, queryResult.Answer);
                metrics.Add(QnATelemetryConstants.ScoreProperty, queryResult.Score);
                properties.Add(QnATelemetryConstants.ArticleFoundProperty, "true");
            }
            else
            {
                properties.Add(QnATelemetryConstants.MatchedQuestionProperty, "No Qna Question matched");
                properties.Add(QnATelemetryConstants.QuestionIdProperty, "No QnA Question Id matched");
                properties.Add(QnATelemetryConstants.AnswerProperty, "No Qna Answer matched");
                properties.Add(QnATelemetryConstants.ArticleFoundProperty, "false");
            }

            // Additional Properties can override "stock" properties.
            if (telemetryProperties != null)
            {
                telemetryProperties = telemetryProperties.Concat(properties)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value);
            }

            // Additional Metrics can override "stock" metrics.
            if (telemetryMetrics != null)
            {
                telemetryMetrics = telemetryMetrics.Concat(metrics)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return Task.FromResult((Properties: telemetryProperties ?? properties, Metrics: telemetryMetrics ?? metrics));
        }

        /// <summary>
        /// Executed when a result is returned from Custom Question Answering.
        /// </summary>
        /// <param name="queryResults">An array of <see cref="QueryResult"/>.</param>
        /// <param name="turnContext">The <see cref="TurnContext"/> that contains the user question to be queried against your knowledge base.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A Task representing the work to be executed.</returns>
        protected virtual async Task OnQnaResultsAsync(
                   QueryResult[] queryResults,
                   ITurnContext turnContext,
                   Dictionary<string, string> telemetryProperties = null,
                   Dictionary<string, double> telemetryMetrics = null,
                   CancellationToken cancellationToken = default)
        {
            var eventData = await FillQnAEventAsync(queryResults, turnContext, telemetryProperties, telemetryMetrics).ConfigureAwait(false);

            // Track the event
            TelemetryClient.TrackEvent(QnATelemetryConstants.QnaMsgEvent, eventData.Properties, eventData.Metrics);
        }
    }
}
