// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Utils
{
    /// <summary>
    /// Helper class for Language service query knowledgebase and update suggestions api.
    /// </summary>
    internal class LanguageServiceUtils
    {
        private const string ApiVersionQueryParam = "api-version=2021-10-01";

        private readonly IBotTelemetryClient _telemetryClient;
        private readonly HttpClient _httpClient;
        private readonly QnAMakerOptions _options;
        private readonly QnAMakerEndpoint _endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageServiceUtils"/> class.
        /// </summary>
        /// <param name="telemetryClient">Telemetry client.</param>
        /// <param name="endpoint">Language Service endpoint details.</param>
        /// <param name="options">QnA Maker options.</param>
        /// <param name="httpClient">Http client.</param>
        public LanguageServiceUtils(IBotTelemetryClient telemetryClient, HttpClient httpClient, QnAMakerEndpoint endpoint, QnAMakerOptions options)
        {
            _telemetryClient = telemetryClient;
            _endpoint = endpoint;
            _httpClient = httpClient;
            _options = options ?? new QnAMakerOptions();
            ValidateOptions(_options);
            _httpClient = httpClient;
        }

        /// <summary>
        /// Converts array of metadata, array of sources and corresponding join operations to LanguageService input format - an object of <see cref="Filters"/>.
        /// </summary>
        /// <param name="metadata">Array of metadata.</param>
        /// <param name="metadataFiltersJoinOperation">Metadata Compound Operation.</param>
        /// <remarks>Required for bot codes working with legacy metadata filters.</remarks>
        /// <returns>An object of <see cref="Filters"/>.</returns>
        public static Filters GetFilters(Metadata[] metadata, string metadataFiltersJoinOperation = "AND")
        {
            if (metadata == null)
            {
                return null;
            }

            var metadataFilter = new MetadataFilter()
            {
                LogicalOperation = metadataFiltersJoinOperation
            };
            metadataFilter.Metadata.AddRange(metadata.Select(m => { return new KeyValuePair<string, string>(key: m.Name, value: m.Value); }).ToList());

            return new Filters()
            {
                MetadataFilter = metadataFilter,
                LogicalOperation = JoinOperator.OR.ToString()
            };
        }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="turnContext">The Turn Context that contains the user question to be queried against your knowledge base.</param>
        /// <param name="messageActivity">Message activity of the turn context.</param>
        /// <param name="options">The options for the Language Service knowledge base. If null, constructor option is used for this instance.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public async Task<QueryResults> QueryKnowledgeBaseAsync(ITurnContext turnContext, IMessageActivity messageActivity, QnAMakerOptions options)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new ArgumentException($"The {nameof(turnContext.Activity)} property for {nameof(turnContext)} can't be null.", nameof(turnContext));
            }

            if (messageActivity == null)
            {
                throw new ArgumentException("Activity type is not a message");
            }

            var hydratedOptions = HydrateOptions(options);
            ValidateOptions(hydratedOptions);

            var result = await QueryKbAsync((Activity)messageActivity, hydratedOptions).ConfigureAwait(false);

            await EmitTraceInfoAsync(turnContext, (Activity)messageActivity, result.Answers, hydratedOptions).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Call to Update Active Learning Feedback.
        /// </summary>
        /// <param name="feedbackRecords">Feedback record list.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateActiveLearningFeedbackAsync(FeedbackRecords feedbackRecords)
        {
            if (feedbackRecords == null)
            {
                throw new ArgumentNullException(nameof(feedbackRecords), "Feedback records cannot be null.");
            }

            if (feedbackRecords.Records == null || feedbackRecords.Records.Length == 0)
            {
                return;
            }

            // Call update active learning feedback 
            await UpdateActiveLearningFeedbackRecordsAsync(feedbackRecords).ConfigureAwait(false);
        }

        /// <summary>
        /// ValidateOptions - duplaicated and modified code from GenerateAnswerUtils class.
        /// </summary>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        private static void ValidateOptions(QnAMakerOptions options)
        {
            if (options.Top == 0)
            {
                options.Top = 1;
            }

            if (options.ScoreThreshold < 0 || options.ScoreThreshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options), $"The {nameof(options.ScoreThreshold)} property should be a value between 0 and 1");
            }

            if (options.Timeout == 0.0D)
            {
                options.Timeout = 100000;
            }

            if (options.Top < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options), $"The {nameof(options.Top)} property should be an integer greater than 0");
            }

            if (options.Filters == null)
            {
                options.Filters = new Filters
                {
                    MetadataFilter = new MetadataFilter()
                    {
                        LogicalOperation = JoinOperator.AND.ToString()
                    }
                };
            }

            if (options.RankerType == null)
            {
                options.RankerType = RankerTypes.DefaultRankerType;
            }
        }

        private async Task<QueryResults> FormatQnaResultAsync(HttpResponseMessage response)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var results = JsonConvert.DeserializeObject<KnowledgeBaseAnswers>(jsonResponse);

            return new QueryResults
            {
                Answers = results.Answers.Select(answer => GetQueryResultFromKBAnswer(answer))
                                        .ToArray(),
                ActiveLearningEnabled = true
            };
        }

        private async Task<QueryResults> QueryKbAsync(Activity messageActivity, QnAMakerOptions options)
        {
            var requestUrl = $"{_endpoint.Host}/language/:query-knowledgebases?projectName={_endpoint.KnowledgeBaseId}&deploymentName=production&{ApiVersionQueryParam}";

            var jsonRequest = JsonConvert.SerializeObject(
                new
                {
                    question = messageActivity.Text,
                    top = options.Top,
                    filters = options.Filters,
                    confidenceScoreThreshold = options.ScoreThreshold,
                    context = options.Context,
                    qnaId = options.QnAId,
                    rankerType = options.RankerType,
                    answerSpanRequest = new { enable = options.EnablePreciseAnswer },
                    includeUnstructuredSources = options.IncludeUnstructuredSources
                }, Formatting.None);
            var httpRequestHelper = new HttpRequestUtils(_httpClient);
            var response = await httpRequestHelper.ExecuteHttpRequestAsync(requestUrl, jsonRequest, _endpoint).ConfigureAwait(false);

            var result = await FormatQnaResultAsync(response).ConfigureAwait(false);
            return result;
        }

        private async Task UpdateActiveLearningFeedbackRecordsAsync(FeedbackRecords feedbackRecords)
        {
            var requestUrl = $"{_endpoint.Host}/language/query-knowledgebases/projects/{_endpoint.KnowledgeBaseId}/feedback?{ApiVersionQueryParam}";

            var feedbackRecordsRequest = new FeedbackRecordsRequest();
            feedbackRecordsRequest.Records.AddRange(feedbackRecords.Records.ToList());

            var jsonRequest = JsonConvert.SerializeObject(feedbackRecordsRequest);

            var httpRequestHelper = new HttpRequestUtils(_httpClient);
            await httpRequestHelper.ExecuteHttpRequestAsync(requestUrl, jsonRequest, _endpoint).ConfigureAwait(false);
        }

        /// <summary>
        /// Combines QnAMakerOptions passed into the QnAMaker constructor with the options passed as arguments into GetAnswersAsync().
        /// </summary>
        /// <param name="queryOptions">The options for the QnA Maker knowledge base.</param>
        /// <returns>Return modified options for the QnA Maker knowledge base.</returns>
        private QnAMakerOptions HydrateOptions(QnAMakerOptions queryOptions)
        {
            var hydratedOptions = JsonConvert.DeserializeObject<QnAMakerOptions>(JsonConvert.SerializeObject(_options));

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

                if (queryOptions.Filters != null)
                {
                    hydratedOptions.Filters = queryOptions.Filters;
                }
                else
                {
                    // For Legacy and V2 preview bots to work as-is with language service
                    hydratedOptions.Filters = GetFilters(queryOptions.StrictFilters, queryOptions.StrictFiltersJoinOperator.ToString());
                }

                hydratedOptions.Context = queryOptions.Context;
                hydratedOptions.QnAId = queryOptions.QnAId;
                hydratedOptions.IsTest = queryOptions.IsTest;
                hydratedOptions.RankerType = queryOptions.RankerType != null ? queryOptions.RankerType : RankerTypes.DefaultRankerType;

                hydratedOptions.EnablePreciseAnswer = queryOptions.EnablePreciseAnswer;
                hydratedOptions.IncludeUnstructuredSources = queryOptions.IncludeUnstructuredSources;
            }

            return hydratedOptions;
        }

        private QueryResult GetQueryResultFromKBAnswer(KnowledgeBaseAnswer kbAnswer)
        {
            return new QueryResult
            {
                Answer = kbAnswer.Answer,
                AnswerSpan = kbAnswer.AnswerSpan != null ? new AnswerSpanResponse
                {
                    Score = (float)kbAnswer.AnswerSpan.ConfidenceScore,
                    Text = kbAnswer.AnswerSpan?.Text,
                    StartIndex = kbAnswer.AnswerSpan?.Offset ?? 0,
                    EndIndex = kbAnswer.AnswerSpan?.Length != null ? (kbAnswer.AnswerSpan?.Offset + kbAnswer.AnswerSpan?.Length - 1).Value : 0
                }
                : null,
                Context = kbAnswer.Dialog != null ? new QnAResponseContext
                {
                    Prompts = kbAnswer.Dialog?.Prompts?.Select(p =>
                                new QnaMakerPrompt { DisplayOrder = p.DisplayOrder, DisplayText = p.DisplayText, Qna = null, QnaId = p.QnaId }).ToArray()
                }
                : null,
                Id = kbAnswer.Id,
                Metadata = kbAnswer.Metadata?.ToList().Select(nv => new Metadata { Name = nv.Key, Value = nv.Value }).ToArray(),
                Questions = kbAnswer.Questions?.ToArray(),
                Score = (float)kbAnswer.ConfidenceScore,
                Source = kbAnswer.Source
            };
        }

        private async Task EmitTraceInfoAsync(ITurnContext turnContext, Activity messageActivity, QueryResult[] result, QnAMakerOptions options)
        {
            var traceInfo = new QnAMakerTraceInfo
            {
                Message = messageActivity,
                QueryResults = result,
                KnowledgeBaseId = _endpoint.KnowledgeBaseId,
                ScoreThreshold = options.ScoreThreshold,
                Top = options.Top,
                StrictFilters = options.StrictFilters,
                Context = options.Context,
                QnAId = options.QnAId,
                IsTest = options.IsTest,
                RankerType = options.RankerType,
                Filters = options.Filters,
                EnablePreciseAnswer = options.EnablePreciseAnswer,
                IncludeUnstructuredSources = options.IncludeUnstructuredSources
            };
            var traceActivity = Activity.CreateTraceActivity(QnAMaker.QnAMakerName, QnAMaker.QnAMakerTraceType, traceInfo, QnAMaker.QnAMakerTraceLabel);
            await turnContext.SendActivityAsync(traceActivity).ConfigureAwait(false);
        }
    }
}
