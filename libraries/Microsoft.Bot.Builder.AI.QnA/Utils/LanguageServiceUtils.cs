// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.QuestionAnswering.Authoring;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Utils
{
    /// <summary>
    /// Helper class for Language service query knowledgebase and update suggestions API.
    /// </summary>
    internal class LanguageServiceUtils
    {
        private readonly QnAMakerOptions _options;
        private readonly QnAMakerEndpoint _endpoint;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings { MaxDepth = null };
        private readonly CustomQuestionAnsweringClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageServiceUtils"/> class.
        /// </summary>
        /// <param name="managedIdentityClientId">The ClientId of the Managed Identity resource.</param>
        /// <param name="endpoint">Language Service endpoint details.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">A client with which to talk to Language Service.</param>
        public LanguageServiceUtils(string managedIdentityClientId, QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(managedIdentityClientId))
            {
                throw new ArgumentNullException(nameof(managedIdentityClientId));
            }

            var credential = new ManagedIdentityCredential(managedIdentityClientId);
            AddHeaders(httpClient);
            var clientOptions = httpClient != null ? new QuestionAnsweringClientOptions { Transport = new HttpClientTransport(httpClient) } : null;
            _client = new CustomQuestionAnsweringClient(
                new QuestionAnsweringClient(new Uri(endpoint.Host), credential, clientOptions),
                new QuestionAnsweringAuthoringClient(new Uri(endpoint.Host), credential, clientOptions));

            _endpoint = endpoint;
            _options = options ?? new QnAMakerOptions();
            ValidateOptions(_options);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageServiceUtils"/> class.
        /// </summary>
        /// <param name="endpoint">Language Service endpoint details.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="httpClient">A client with which to talk to Language Service.</param>
        public LanguageServiceUtils(QnAMakerEndpoint endpoint, QnAMakerOptions options, HttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(endpoint.EndpointKey))
            {
                throw new ArgumentNullException(nameof(endpoint.EndpointKey));
            }

            var credential = new AzureKeyCredential(endpoint.EndpointKey);
            AddHeaders(httpClient);
            var clientOptions = httpClient != null ? new QuestionAnsweringClientOptions { Transport = new HttpClientTransport(httpClient) } : null;
            _client = new CustomQuestionAnsweringClient(
                new QuestionAnsweringClient(new Uri(endpoint.Host), credential, clientOptions),
                new QuestionAnsweringAuthoringClient(new Uri(endpoint.Host), credential, clientOptions));

            _endpoint = endpoint;
            _options = options ?? new QnAMakerOptions();
            ValidateOptions(_options);
        }

        internal LanguageServiceUtils(CustomQuestionAnsweringClient client, QnAMakerEndpoint endpoint, QnAMakerOptions options)
        {
            _client = client;
            _endpoint = endpoint;
            _options = options ?? new QnAMakerOptions();
            ValidateOptions(_options);
        }

        /// <summary>
        /// Converts array of metadata, array of sources and corresponding join operations to Language Service input format - an object of <see cref="Filters"/>.
        /// </summary>
        /// <param name="metadata">Array of <see cref="Metadata"/>.</param>
        /// <param name="metadataFiltersJoinOperation">Metadata Compound Operation.</param>
        /// <remarks>Required for bot codes working with legacy metadata filters.</remarks>
        /// <returns>An object of <see cref="Filters"/>.</returns>
        public static Filters GetFilters(Metadata[] metadata, string metadataFiltersJoinOperation = "AND")
        {
            if (metadata == null)
            {
                return null;
            }

            var metadataFilter = new Models.MetadataFilter()
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
        /// <param name="turnContext">The <see cref="TurnContext"/> that contains the user question to be queried against your knowledge base.</param>
        /// <param name="messageActivity">Message activity of the turn context.</param>
        /// <param name="options">The <see cref="QnAMakerOptions"/> for the Custom Question Answering Knowledge Base. If null, constructor option is used for this instance.</param>
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
        /// <param name="feedbackRecords">An object containing an array of <see cref="FeedbackRecord"/>.</param>
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
        /// ValidateOptions - duplicated and modified code from GenerateAnswerUtils class.
        /// </summary>
        /// <param name="options">The options for the Custom Question Answering Knowledge Base.</param>
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
                    MetadataFilter = new Models.MetadataFilter()
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

        /// <summary>
        /// Add headers to the incoming HttpClient although there is an issue preventing to do so: https://github.com/Azure/azure-sdk-for-net/issues/4086.
        /// </summary>
        /// <param name="httpClient">The httpClient <see cref="HttpClient"/>.</param>
        private void AddHeaders(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                return;
            }

            // Bot Builder Package name and version
            var assemblyName = GetType().Assembly.GetName();
            var botBuilderInfo = new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString());

            // Platform information: OS and language runtime
            var framework = Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName ?? RuntimeInformation.FrameworkDescription;
            var comment = $"({Environment.OSVersion.VersionString};{framework})";
            var platformInfo = new ProductInfoHeaderValue(comment);

            httpClient.DefaultRequestHeaders.UserAgent.Add(botBuilderInfo);
            httpClient.DefaultRequestHeaders.UserAgent.Add(platformInfo);
        }

        private async Task<QueryResults> QueryKbAsync(Activity messageActivity, QnAMakerOptions options)
        {
            var result = await _client.GetAnswersAsync(messageActivity, _endpoint, options).ConfigureAwait(false);

            return new QueryResults
            {
                Answers = result.Answers
                    .Select(answer => GetQueryResultFromKBAnswer(answer))
                    .ToArray(),
                ActiveLearningEnabled = true
            };
        }

        private async Task UpdateActiveLearningFeedbackRecordsAsync(FeedbackRecords feedbackRecords)
        {
            var records = RequestContent.Create(feedbackRecords);
            await _client.AddFeedbackAsync(_endpoint.KnowledgeBaseId, records).ConfigureAwait(false);
        }

        /// <summary>
        /// Combines <see cref="QnAMakerOptions"/> passed into the <see cref="CustomQuestionAnswering"/> constructor with the options passed as arguments into GetAnswersAsync().
        /// </summary>
        /// <param name="queryOptions">The <see cref="QnAMakerOptions"/> for the QnA Maker knowledge base.</param>
        /// <returns>Return modified options for the Custom Question Answering Knowledge Base.</returns>
        private QnAMakerOptions HydrateOptions(QnAMakerOptions queryOptions)
        {
            var hydratedOptions = JsonConvert.DeserializeObject<QnAMakerOptions>(JsonConvert.SerializeObject(_options, _settings), _settings);

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
                    // For Legacy and V2 preview bots to work as-is with Language Service
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

        private QueryResult GetQueryResultFromKBAnswer(Models.KnowledgeBaseAnswer kbAnswer)
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
                                new QnaMakerPrompt { DisplayOrder = (int)p.DisplayOrder, DisplayText = p.DisplayText, Qna = null, QnaId = (int)p.QnaId }).ToArray()
                }
                : null,
                Id = (int)kbAnswer.Id,
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
