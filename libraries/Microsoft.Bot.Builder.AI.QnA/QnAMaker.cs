// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Provides access to a QnA Maker knowledge base.
    /// </summary>
    public class QnAMaker : IQnAMakerClient, ITelemetryQnAMaker
    {
        public static readonly string QnAMakerName = nameof(QnAMaker);
        public static readonly string QnAMakerTraceType = "https://www.qnamaker.ai/schemas/trace";
        public static readonly string QnAMakerTraceLabel = "QnAMaker Trace";

        private readonly HttpClient _httpClient;

        private readonly QnAMakerEndpoint _endpoint;

        private GenerateAnswerUtils generateAnswerHelper;
        private TrainUtils activeLearningTrainHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used for logging telemetry events.</param>
        /// <param name="logPersonalInformation">Set to true to include personally identifiable information in telemetry events.</param>
        public QnAMaker(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
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

            if (string.IsNullOrEmpty(endpoint.EndpointKey))
            {
                throw new ArgumentException(nameof(endpoint.EndpointKey));
            }

            if (_endpoint.Host.EndsWith("v2.0") || _endpoint.Host.EndsWith("v3.0"))
            {
                throw new NotSupportedException("v2.0 and v3.0 of QnA Maker service is no longer supported in the QnA Maker.");
            }

            if (httpClient == null)
            {
                _httpClient = DefaultHttpClient;
            }
            else
            {
                _httpClient = httpClient;
            }

            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;

            this.generateAnswerHelper = new GenerateAnswerUtils(TelemetryClient, _endpoint, options, _httpClient);
            this.activeLearningTrainHelper = new TrainUtils(_endpoint, _httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        public QnAMaker(QnAMakerEndpoint endpoint, QnAMakerOptions options = null, HttpClient httpClient = null)
            : this(endpoint, options, httpClient, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="service">QnA service details from configuration.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used for logging telemetry events.</param>
        /// <param name="logPersonalInformation">Set to true to include personally identifiable information in telemetry events.</param>
        public QnAMaker(QnAMakerService service, QnAMakerOptions options, HttpClient httpClient, IBotTelemetryClient telemetryClient, bool logPersonalInformation = false)
            : this(new QnAMakerEndpoint(service), options, httpClient, telemetryClient, logPersonalInformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="service">QnA service details from configuration.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        public QnAMaker(QnAMakerService service, QnAMakerOptions options = null, HttpClient httpClient = null)
            : this(new QnAMakerEndpoint(service), options, httpClient, null)
        {
        }

        public static HttpClient DefaultHttpClient { get; } = new HttpClient();

        /// <summary>
        /// Gets a value indicating whether determines whether to log personal information that came from the user.
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
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The options for the QnA Maker knowledge base. If null, constructor option is used for this instance.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public Task<QueryResult[]> GetAnswersAsync(ITurnContext turnContext, QnAMakerOptions options = null)
        {
            return GetAnswersAsync(turnContext, options, null);
        }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The options for the QnA Maker knowledge base. If null, constructor option is used for this instance.</param>
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
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="options">The options for the QnA Maker knowledge base. If null, constructor option is used for this instance.</param>
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
                throw new ArgumentNullException(nameof(turnContext.Activity));
            }

            var messageActivity = turnContext.Activity.AsMessageActivity();
            if (messageActivity == null)
            {
                throw new ArgumentException("Activity type is not a message");
            }

            if (string.IsNullOrEmpty(turnContext.Activity.Text))
            {
                throw new ArgumentException("Null or empty text");
            }

            var result = await this.generateAnswerHelper.GetAnswersRawAsync(turnContext, messageActivity, options).ConfigureAwait(false);

            await OnQnaResultsAsync(result.Answers, turnContext, telemetryProperties, telemetryMetrics, CancellationToken.None).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Filters the ambiguous question for active learning.
        /// </summary>
        /// <param name="queryResult">User query output.</param>
        /// <returns>Filtered array of ambiguous question.</returns>
        public QueryResult[] GetLowScoreVariation(QueryResult[] queryResult)
        {
            return ActiveLearningUtils.GetLowScoreVariation(queryResult.ToList()).ToArray();
        }

        /// <summary>
        /// Send feedback to the knowledge base.
        /// </summary>
        /// <param name="feedbackRecords">Feedback records.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CallTrainAsync(FeedbackRecords feedbackRecords)
        {
            await this.activeLearningTrainHelper.CallTrainAsync(feedbackRecords).ConfigureAwait(false);
        }

        protected virtual async Task OnQnaResultsAsync(
                   QueryResult[] queryResults,
                   ITurnContext turnContext,
                   Dictionary<string, string> telemetryProperties = null,
                   Dictionary<string, double> telemetryMetrics = null,
                   CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventData = await FillQnAEventAsync(queryResults, turnContext, telemetryProperties, telemetryMetrics, cancellationToken).ConfigureAwait(false);

            // Track the event
            this.TelemetryClient.TrackEvent(QnATelemetryConstants.QnaMsgEvent, eventData.Properties, eventData.Metrics);
        }

        /// <summary>
        /// Fills the event properties and metrics for the QnaMessage event for telemetry.
        /// These properties are logged when the QnA GetAnswers method is called.
        /// </summary>
        /// <param name="queryResults">QnA service results.</param>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="telemetryProperties">Properties to add/override for the event.</param>
        /// <param name="telemetryMetrics">Metrics to add/override for the event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// additionalProperties
        /// <returns>A tuple of Properties and Metrics that will be sent to the IBotTelemetryClient.TrackEvent method for the QnAMessage event.  The properties and metrics returned the standard properties logged with any properties passed from the GetAnswersAsync method.</returns>
        protected Task<(Dictionary<string, string> Properties, Dictionary<string, double> Metrics)> FillQnAEventAsync(QueryResult[] queryResults, ITurnContext turnContext, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var properties = new Dictionary<string, string>();
            var metrics = new Dictionary<string, double>();

            properties.Add(QnATelemetryConstants.KnowledgeBaseIdProperty, _endpoint.KnowledgeBaseId);

            var text = turnContext.Activity.Text;
            var userName = turnContext.Activity.From?.Name;

            // Use the LogPersonalInformation flag to toggle logging PII data, text and user name are common examples
            if (this.LogPersonalInformation)
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
                properties.Add(QnATelemetryConstants.MatchedQuestionProperty, JsonConvert.SerializeObject(queryResult.Questions));
                properties.Add(QnATelemetryConstants.QuestionIdProperty, queryResult.Id.ToString());
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
    }
}
