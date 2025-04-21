// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.QuestionAnswering.Authoring;
using Azure.Core;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA.Utils
{
    /// <summary>
    /// A custom class to manage the <see cref="QuestionAnsweringClient"/> related operations.
    /// </summary>
    internal class CustomQuestionAnsweringClient
    {
        private readonly QuestionAnsweringClient _client;
        private readonly QuestionAnsweringAuthoringClient _authoring;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQuestionAnsweringClient"/> class.
        /// </summary>
        /// <param name="client">The <see cref="QuestionAnsweringClient"/> instance.</param>
        /// <param name="authoring">The <see cref="QuestionAnsweringAuthoringClient"/> instance.</param>
        internal CustomQuestionAnsweringClient(QuestionAnsweringClient client, QuestionAnsweringAuthoringClient authoring)
        {
            _client = client;
            _authoring = authoring;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomQuestionAnsweringClient"/> class.
        /// </summary>
        internal CustomQuestionAnsweringClient()
        {
            // This constructor is used for mocking purposes.
        }

        /// <summary>
        /// See <see cref="QuestionAnsweringClient.GetAnswersAsync(string, QuestionAnsweringProject, AnswersOptions, CancellationToken)"/>.
        /// </summary>
        /// <param name="activity">Activity.</param>
        /// <param name="endpoint">QnAMakerEndpoint.</param>
        /// <param name="options">QnAMakerOptions.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<KnowledgeBaseAnswers> GetAnswersAsync(Activity activity, QnAMakerEndpoint endpoint, QnAMakerOptions options = null, CancellationToken cancellationToken = default)
        {
            var deploymentName = options.IsTest ? "test" : "production";
            var project = new QuestionAnsweringProject(endpoint.KnowledgeBaseId, deploymentName);

            var newOptions = new AnswersOptions
            {
                ConfidenceThreshold = options.ScoreThreshold,
                IncludeUnstructuredSources = options.IncludeUnstructuredSources.Value,
                RankerKind = options.RankerType,
                Size = options.Top,
                UserId = activity.From?.Id,
                Filters = new QueryFilters
                {
                    MetadataFilter = new Azure.AI.Language.QuestionAnswering.MetadataFilter(),
                },
            };

            if (options.Context?.PreviousQnAId != null && options.Context?.PreviousUserQuery != null)
            {
                newOptions.AnswerContext = new KnowledgeBaseAnswerContext(options.Context.PreviousQnAId)
                {
                    PreviousQuestion = options.Context.PreviousUserQuery,
                };
            }

            if (!string.IsNullOrWhiteSpace(options.Filters?.LogicalOperation))
            {
                newOptions.Filters.LogicalOperation = options.Filters.LogicalOperation;
            }

            if (!string.IsNullOrWhiteSpace(options.Filters?.MetadataFilter?.LogicalOperation))
            {
                newOptions.Filters.MetadataFilter.LogicalOperation = options.Filters.MetadataFilter.LogicalOperation;
            }

            if (options.EnablePreciseAnswer.Value)
            {
                newOptions.ShortAnswerOptions = new ShortAnswerOptions
                {
                    ConfidenceThreshold = 1,
                };
            }

            var result = await _client.GetAnswersAsync(activity.Text, project, newOptions, cancellationToken).ConfigureAwait(false);

            var kbAnswers = new KnowledgeBaseAnswers();
            foreach (var answer in result.Value.Answers)
            {
                var kbAnswer = new Models.KnowledgeBaseAnswer
                {
                    Id = (int)answer.QnaId,
                    Answer = answer.Answer,
                    ConfidenceScore = (float)answer.Confidence,
                    Source = answer.Source
                };

                foreach (var questionItem in answer.Questions)
                {
                    kbAnswer.Questions.Add(questionItem);
                }

                if (answer.ShortAnswer != null)
                {
                    kbAnswer.AnswerSpan = new KnowledgeBaseAnswerSpan
                    {
                        ConfidenceScore = (float)answer.Confidence,
                        Length = (int)answer.ShortAnswer?.Length,
                        Offset = (int)answer.ShortAnswer?.Offset,
                    };
                }

                if (answer.Dialog != null)
                {
                    kbAnswer.Dialog = new QnAResponseContext
                    {
                        Prompts = answer.Dialog.Prompts.Select(e => new QnaMakerPrompt
                        {
                            DisplayOrder = (int)e.DisplayOrder,
                            DisplayText = e.DisplayText,
                            Qna = null,
                            QnaId = (int)e.QnaId
                        }).ToArray()
                    };
                }

                kbAnswers.Answers.Add(kbAnswer);
            }

            return kbAnswers;
        }

        /// <summary>
        /// See <see cref="QuestionAnsweringAuthoringClient.AddFeedbackAsync(string, RequestContent, RequestContext)"/>.
        /// </summary>
        /// <param name="projectName">Project name.</param>
        /// <param name="content">RequestContent.</param>
        /// <param name="context">RequestContext.</param>
        /// <returns>A task response.</returns>
        public virtual Task<Response> AddFeedbackAsync(string projectName, RequestContent content, RequestContext context = null)
        {
            return _authoring.AddFeedbackAsync(projectName, content, context);
        }
    }
}
