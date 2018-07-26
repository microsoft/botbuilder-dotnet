// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// Provides access to a QnA Maker knowledge base.
    /// </summary>
    public class QnAMaker
    {
        public const string QnAMakerMiddlewareName = "QnAMakerMiddleware";
        public const string QnAMakerTraceType = "https://www.qnamaker.ai/schemas/trace";
        public const string QnAMakerTraceLabel = "QnAMaker Trace";

        private static readonly HttpClient DefaultHttpClient = new HttpClient();
        private readonly HttpClient _httpClient;

        private readonly QnAMakerEndpoint _endpoint;
        private readonly QnAMakerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMaker"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        public QnAMaker(QnAMakerEndpoint endpoint, QnAMakerOptions options = null, HttpClient httpClient = null)
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

            _options = options ?? new QnAMakerOptions();

            if (_options.ScoreThreshold == 0)
            {
                _options.ScoreThreshold = 0.3F;
            }

            if (_options.Top == 0)
            {
                _options.Top = 1;
            }

            if (_options.ScoreThreshold < 0 || _options.ScoreThreshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.ScoreThreshold), "Score threshold should be a value between 0 and 1");
            }

            if (_options.Top < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.Top), "Top should be an integer greater than 0");
            }

            if (_options.StrictFilters == null)
            {
                _options.StrictFilters = new Metadata[] { };
            }

            if (_options.MetadataBoost == null)
            {
                _options.MetadataBoost = new Metadata[] { };
            }
        }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="context">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public async Task<QueryResult[]> GetAnswersAsync(ITurnContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Activity == null)
            {
                throw new ArgumentNullException(nameof(context.Activity));
            }

            var messageActivity = context.Activity.AsMessageActivity();
            if (messageActivity == null)
            {
                throw new ArgumentException("Activity type is not a message");
            }

            if (string.IsNullOrEmpty(context.Activity.Text))
            {
                throw new ArgumentException("Null or empty text");
            }

            var requestUrl = $"{_endpoint.Host}/knowledgebases/{_endpoint.KnowledgeBaseId}/generateanswer";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            var jsonRequest = JsonConvert.SerializeObject(
                new
                {
                    messageActivity.Text,
                    top = _options.Top,
                    strictFilters = _options.StrictFilters,
                    metadataBoost = _options.MetadataBoost,
                }, Formatting.None);

            request.Content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            var isLegacyProtocol = _endpoint.Host.EndsWith("v2.0") || _endpoint.Host.EndsWith("v3.0");

            if (isLegacyProtocol)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", _endpoint.EndpointKey);
            }
            else
            {
                request.Headers.Add("Authorization", $"EndpointKey {_endpoint.EndpointKey}");
            }

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var results = isLegacyProtocol ?
                ConvertLegacyResults(JsonConvert.DeserializeObject<InternalQueryResults>(jsonResponse))
                    :
                JsonConvert.DeserializeObject<QueryResults>(jsonResponse);

            foreach (var answer in results.Answers)
            {
                answer.Score = answer.Score / 100;
            }

            var result = results.Answers.Where(answer => answer.Score > _options.ScoreThreshold).ToArray();

            var traceInfo = new QnAMakerTraceInfo
            {
                Message = (Activity)messageActivity,
                QueryResults = result,
                KnowledgeBaseId = _endpoint.KnowledgeBaseId,
                ScoreThreshold = _options.ScoreThreshold,
                Top = _options.Top,
                StrictFilters = _options.StrictFilters,
                MetadataBoost = _options.MetadataBoost,
            };
            var traceActivity = Activity.CreateTraceActivity(QnAMakerMiddlewareName, QnAMakerTraceType, traceInfo, QnAMakerTraceLabel);
            await context.SendActivityAsync(traceActivity).ConfigureAwait(false);

            return result;
        }

        // The old version of the protocol returns the id in a field called qnaId the
        // following classes and helper function translate this old structure
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
