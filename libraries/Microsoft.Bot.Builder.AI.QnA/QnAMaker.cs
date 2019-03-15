// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Provides access to a QnA Maker knowledge base.
    /// </summary>
    public class QnAMaker : ITelemetryQnAMaker
    {
        public const string QnAMakerName = nameof(QnAMaker);
        public const string QnAMakerTraceType = "https://www.qnamaker.ai/schemas/trace";
        public const string QnAMakerTraceLabel = "QnAMaker Trace";

        private static readonly HttpClient DefaultHttpClient = new HttpClient();
        private readonly HttpClient _httpClient;

        private readonly QnAMakerEndpoint _endpoint;
        private readonly QnAMakerOptions _options;

        private readonly bool _isLegacyProtocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        /// <param name="telemetryClient">The IBotTelemetryClient used for logging telemetry events.</param>
        /// <param name="logPersonalInformation">Set to true to include personally indentifiable information in telemetry events.</param>
        public QnAMaker(QnAMakerEndpoint endpoint, QnAMakerOptions options = null, HttpClient httpClient = null, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
        {
            _httpClient = httpClient ?? DefaultHttpClient;

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

            if (_endpoint.Host.EndsWith("v2.0"))
            {
                throw new NotSupportedException("v2.0 of QnA Maker service is no longer supported in the Bot Framework. Please upgrade your QnA Maker service at www.qnamaker.ai.");
            }

            _isLegacyProtocol = _endpoint.Host.EndsWith("v3.0");

            _options = options ?? new QnAMakerOptions();
            ValidateOptions(_options);

            TelemetryClient = telemetryClient ?? new NullBotTelemetryClient();
            LogPersonalInformation = logPersonalInformation;
        }

        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="service">QnA service details from configuration.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        public QnAMaker(QnAMakerService service, QnAMakerOptions options = null, HttpClient httpClient = null, IBotTelemetryClient telemetryClient = null, bool logPersonalInformation = false)
            : this(new QnAMakerEndpoint(service), options, httpClient, telemetryClient, logPersonalInformation)
        {
        }

        /// <summary>
        /// Gets a value indicating whether determines whether to log personal information that came from the user.
        /// </summary>
        /// <value>If true, will log personal information into the IBotTelemetryClient.TrackEvent method; otherwise the properties will be filtered.</value>
        public bool LogPersonalInformation { get; }

        /// <summary>
        /// Gets the currently configured <see cref="IBotTelemetryClient"/> that logs the QnaMessage event.
        /// </summary>
        /// <value>The <see cref=IBotTelemetryClient"/> being used to log events.</value>
        public IBotTelemetryClient TelemetryClient { get; }

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
                                        QnAMakerOptions options = null,
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

            var hydratedOptions = HydrateOptions(options);
            ValidateOptions(hydratedOptions);

            var result = await QueryQnaServiceAsync((Activity)messageActivity, hydratedOptions).ConfigureAwait(false);

            await EmitTraceInfoAsync(turnContext, (Activity)messageActivity, result, hydratedOptions).ConfigureAwait(false);

            await OnQnaResultsAsync(result, turnContext, telemetryProperties, telemetryMetrics, CancellationToken.None).ConfigureAwait(false);

            return result;
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
            TelemetryClient.TrackEvent(QnATelemetryConstants.QnaMsgEvent, eventData.Properties, eventData.Metrics);
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
            var userName = turnContext.Activity.From.Name;

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

            // Fill in Qna Results (found or not)
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
                properties.Add(QnATelemetryConstants.QuestionProperty, "No Qna Question matched");
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
        /// Combines QnAMakerOptions passed into the QnAMaker constructor with the options passed as arguments into GetAnswersAsync().
        /// </summary>
        /// <param name="queryOptions">The options for the QnA Maker knowledge base.</param>
        private QnAMakerOptions HydrateOptions(QnAMakerOptions queryOptions)
        {
            var hydratedOptions = _options;

            if (queryOptions != null)
            {
                if (queryOptions.ScoreThreshold != hydratedOptions.ScoreThreshold && queryOptions.ScoreThreshold != 0)
                {
                    hydratedOptions.ScoreThreshold = queryOptions.ScoreThreshold;
                }

                if (queryOptions.Top != hydratedOptions.Top && queryOptions.Top != 0)
                {
                    hydratedOptions.Top = queryOptions.Top;
                }

                if (queryOptions.StrictFilters?.Length > 0)
                {
                   hydratedOptions.StrictFilters = queryOptions.StrictFilters;
                }

                if (queryOptions.MetadataBoost?.Length > 0)
                {
                   hydratedOptions.MetadataBoost = queryOptions.MetadataBoost;
                }
            }

            return hydratedOptions;
        }

        private void ValidateOptions(QnAMakerOptions options)
        {
            if (options.ScoreThreshold == 0)
            {
                options.ScoreThreshold = 0.3F;
            }

            if (options.Top == 0)
            {
                options.Top = 1;
            }

            if (options.ScoreThreshold < 0 || options.ScoreThreshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options.ScoreThreshold), "Score threshold should be a value between 0 and 1");
            }

            if (options.Top < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Top), "Top should be an integer greater than 0");
            }

            if (options.StrictFilters == null)
            {
                options.StrictFilters = new Metadata[] { };
            }

            if (options.MetadataBoost == null)
            {
                options.MetadataBoost = new Metadata[] { };
            }
        }

        private async Task<QueryResult[]> QueryQnaServiceAsync(Activity messageActivity, QnAMakerOptions options)
        {
            var request = BuildRequest(messageActivity, options);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await FormatQnaResultAsync(response, options).ConfigureAwait(false);

            return result;
        }

        private async Task EmitTraceInfoAsync(ITurnContext turnContext, Activity messageActivity, QueryResult[] result, QnAMakerOptions options)
        {
            var traceInfo = new QnAMakerTraceInfo
            {
                Message = (Activity)messageActivity,
                QueryResults = result,
                KnowledgeBaseId = _endpoint.KnowledgeBaseId,
                ScoreThreshold = options.ScoreThreshold,
                Top = options.Top,
                StrictFilters = options.StrictFilters,
                MetadataBoost = options.MetadataBoost,
            };
            var traceActivity = Activity.CreateTraceActivity(QnAMakerName, QnAMakerTraceType, traceInfo, QnAMakerTraceLabel);
            await turnContext.SendActivityAsync(traceActivity).ConfigureAwait(false);
        }

        private HttpRequestMessage BuildRequest(Activity messageActivity, QnAMakerOptions options)
        {
            var requestUrl = $"{_endpoint.Host}/knowledgebases/{_endpoint.KnowledgeBaseId}/generateanswer";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            var jsonRequest = JsonConvert.SerializeObject(
                new
                {
                    question = messageActivity.Text,
                    top = options.Top,
                    strictFilters = options.StrictFilters,
                    metadataBoost = options.MetadataBoost,
                    scoreThreshold = options.ScoreThreshold,
                }, Formatting.None);

            request.Content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            SetHeaders(request);

            return request;
        }

        private async Task<QueryResult[]> FormatQnaResultAsync(HttpResponseMessage response, QnAMakerOptions options)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var results = _isLegacyProtocol ?
                ConvertLegacyResults(JsonConvert.DeserializeObject<InternalQueryResults>(jsonResponse))
                    :
                JsonConvert.DeserializeObject<QueryResults>(jsonResponse);

            foreach (var answer in results.Answers)
            {
                answer.Score = answer.Score / 100;
            }

            var result = results.Answers.Where(answer => answer.Score > options.ScoreThreshold).ToArray();

            return result;
        }

        private void SetHeaders(HttpRequestMessage request)
        {
            if (_isLegacyProtocol)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", _endpoint.EndpointKey);
            }
            else
            {
                request.Headers.Add("Authorization", $"EndpointKey {_endpoint.EndpointKey}");
            }

            AddUserAgent(request);
        }

        private void AddUserAgent(HttpRequestMessage request)
        {
            // Bot Builder Package name and version
            var assemblyName = this.GetType().Assembly.GetName();
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString()));

            // Platform information: OS and language runtime
            var framework = Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;
            var comment = $"({Environment.OSVersion.VersionString};{framework})";
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(comment));
        }

        // The old version of the protocol returns the id in a field called qnaId the
        // following classes and helper function translate this old structure
        // This method can consume results from v3.0 of QnA API, but not v2.0.
        private QueryResults ConvertLegacyResults(InternalQueryResults legacyResults) => new QueryResults
        {
            Answers = legacyResults.Answers
                    .Select(answer => new QueryResult
                    {
                        // The old version of the protocol returns the "id" in a field called "qnaId"
                        Id = answer.QnaId,
                        Answer = answer.Answer,
                        Metadata = answer.Metadata,
                        Score = answer.Score,
                        Source = answer.Source,
                        Questions = answer.Questions,
                    })
                    .ToArray(),
        };

        private class InternalQueryResult : QueryResult
        {
            [JsonProperty(PropertyName = "qnaId")]
            public int QnaId { get; set; }
        }

        private class InternalQueryResults
        {
            /// <summary>
            /// Gets or sets the answers for a user query,
            /// sorted in decreasing order of ranking score.
            /// </summary>
            [JsonProperty("answers")]
            public InternalQueryResult[] Answers { get; set; }
        }
    }
}
