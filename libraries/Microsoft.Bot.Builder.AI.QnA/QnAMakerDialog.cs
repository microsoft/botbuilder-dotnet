// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAMaker dialog which uses QnAMaker to get an answer.
    /// </summary>
    public class QnAMakerDialog : WaterfallDialog
    {
        // Dialog Options parameters
        private const float DefaultThreshold = 0.3F;
        private const int DefaultTopN = 3;
        private const string DefaultNoAnswer = "No QnAMaker answers found.";

        // Card parameters
        private const string DefaultCardTitle = "Did you mean:";
        private const string DefaultCardNoMatchText = "None of the above.";
        private const string DefaultCardNoMatchResponse = "Thanks for the feedback.";

        private readonly HttpClient httpClient;
        private float maximumScoreForLowScoreVariation = 0.95F;
        private string knowledgeBaseId;
        private string hostName;
        private string endpointKey;
        private float threshold;
        private Activity noAnswer;
        private string activeLearningCardTitle;
        private string cardNoMatchText;
        private Activity cardNoMatchResponse;
        private Metadata[] strictFilters;

        public QnAMakerDialog(
            string knowledgeBaseId,
            string endpointKey,
            string hostName,
            Activity noAnswer = null,
            float threshold = DefaultThreshold,
            string activeLearningCardTitle = DefaultCardTitle,
            string cardNoMatchText = DefaultCardNoMatchText,
            Activity cardNoMatchResponse = null,
            Metadata[] strictFilters = null,
            HttpClient httpClient = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
            : base(nameof(QnAMakerDialog))
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            this.knowledgeBaseId = knowledgeBaseId ?? throw new ArgumentNullException(nameof(knowledgeBaseId));
            this.hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            this.endpointKey = endpointKey ?? throw new ArgumentNullException(nameof(endpointKey));
            this.threshold = threshold;
            this.activeLearningCardTitle = activeLearningCardTitle;
            this.cardNoMatchText = cardNoMatchText;
            this.strictFilters = strictFilters;
            this.httpClient = httpClient;
            this.noAnswer = noAnswer;
            this.cardNoMatchResponse = cardNoMatchResponse;

            // add waterfall steps
            this.AddStep(CallGenerateAnswerAsync);
            this.AddStep(CallTrainAsync);
            this.AddStep(CheckForMultiTurnPromptAsync);
            this.AddStep(DisplayQnAResultAsync);
        }

        [JsonConstructor]
        public QnAMakerDialog([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(nameof(QnAMakerDialog))
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);

            // add waterfall steps
            this.AddStep(CallGenerateAnswerAsync);
            this.AddStep(CallTrainAsync);
            this.AddStep(CheckForMultiTurnPromptAsync);
            this.AddStep(DisplayQnAResultAsync);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (dc.Context?.Activity?.Type != ActivityTypes.Message)
            {
                return EndOfTurn;
            }

            var dialogOptions = new QnAMakerDialogOptions()
            {
                Options = GetQnAMakerOptions(dc),
                ResponseOptions = GetQnAResponseOptions(dc)
            };

            if (options != null)
            {
                dialogOptions = ObjectPath.Assign<QnAMakerDialogOptions>(dialogOptions, options);
            }

            dc.State.SetValue(ThisPath.OPTIONS, dialogOptions);

            return await base.BeginDialogAsync(dc, dialogOptions, cancellationToken).ConfigureAwait(false);
        }

        protected virtual QnAMaker GetQnAMakerClient(DialogContext dc)
        {
            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = this.endpointKey,
                Host = this.hostName,
                KnowledgeBaseId = this.knowledgeBaseId
            };
            return new QnAMaker(endpoint, GetQnAMakerOptions(dc), httpClient);
        }

        protected virtual QnAMakerOptions GetQnAMakerOptions(DialogContext dc)
        {
            return new QnAMakerOptions
            {
                ScoreThreshold = this.threshold,
                StrictFilters = this.strictFilters
            };
        }

        protected virtual QnADialogResponseOptions GetQnAResponseOptions(DialogContext dc)
        {
            return new QnADialogResponseOptions
            {
                NoAnswer = noAnswer,
                ActiveLearningCardTitle = activeLearningCardTitle ?? DefaultCardTitle,
                CardNoMatchText = cardNoMatchText ?? DefaultCardNoMatchText,
                CardNoMatchResponse = cardNoMatchResponse
            };
        }

        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = stepContext.State.GetValue<QnAMakerDialogOptions>(ThisPath.OPTIONS);

            // Storing the context info
            stepContext.Values[ValueProperty.CurrentQuery] = stepContext.Context.Activity.Text;

            // -Check if previous context is present, if yes then put it with the query
            // -Check for id if query is present in reverse index.
            var previousContextData = stepContext.State.GetValue(QnAPath.QnAContextData, () => new Dictionary<string, int>());
            var previousQnAId = stepContext.State.GetIntValue(QnAPath.PreviousQnAId, 0);

            if (previousQnAId > 0)
            {
                dialogOptions.Options.Context = new QnARequestContext
                {
                    PreviousQnAId = previousQnAId
                };

                if (previousContextData.TryGetValue(stepContext.Context.Activity.Text, out var currentQnAId))
                {
                    dialogOptions.Options.QnAId = currentQnAId;
                }
            }

            // Calling QnAMaker to get response.
            var qnaClient = GetQnAMakerClient(stepContext);
            var response = await qnaClient.GetAnswersRawAsync(stepContext.Context, dialogOptions.Options).ConfigureAwait(false);

            // Resetting previous query.
            previousQnAId = -1;
            stepContext.State.SetValue(QnAPath.PreviousQnAId, previousQnAId);

            // Take this value from GetAnswerResponse 
            var isActiveLearningEnabled = response.ActiveLearningEnabled;

            stepContext.Values[ValueProperty.QnAData] = new List<QueryResult>(response.Answers);

            // Check if active learning is enabled.
            // maximumScoreForLowScoreVariation is the score above which no need to check for feedback.
            if (isActiveLearningEnabled && response.Answers.Any() && response.Answers.First().Score <= maximumScoreForLowScoreVariation)
            {
                // Get filtered list of the response that support low score variation criteria.
                response.Answers = qnaClient.GetLowScoreVariation(response.Answers);

                if (response.Answers.Count() > 1)
                {
                    var suggestedQuestions = new List<string>();
                    foreach (var qna in response.Answers)
                    {
                        suggestedQuestions.Add(qna.Questions[0]);
                    }

                    // Get active learning suggestion card activity.
                    var message = QnACardBuilder.GetSuggestionsCard(suggestedQuestions, dialogOptions.ResponseOptions.ActiveLearningCardTitle, dialogOptions.ResponseOptions.CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    stepContext.State.SetValue(ThisPath.OPTIONS, dialogOptions);
                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            var result = new List<QueryResult>();
            if (response.Answers.Any())
            {
                result.Add(response.Answers.First());
            }

            stepContext.Values[ValueProperty.QnAData] = result;
            stepContext.State.SetValue(ThisPath.OPTIONS, dialogOptions);

            // If card is not shown, move to next step with top qna response.
            return await stepContext.NextAsync(result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CallTrainAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = stepContext.State.GetValue<QnAMakerDialogOptions>(ThisPath.OPTIONS);
            var trainResponses = stepContext.Values[ValueProperty.QnAData] as List<QueryResult>;
            var currentQuery = stepContext.Values[ValueProperty.CurrentQuery] as string;

            var reply = stepContext.Context.Activity.Text;

            if (trainResponses.Count > 1)
            {
                var qnaResult = trainResponses.FirstOrDefault(kvp => kvp.Questions[0] == reply);

                if (qnaResult != null)
                {
                    stepContext.Values[ValueProperty.QnAData] = new List<QueryResult>() { qnaResult };

                    var records = new FeedbackRecord[]
                    {
                        new FeedbackRecord
                        {
                            UserId = stepContext.Context.Activity.Id,
                            UserQuestion = currentQuery,
                            QnaId = qnaResult.Id,
                        }
                    };

                    var feedbackRecords = new FeedbackRecords { Records = records };

                    // Call Active Learning Train API
                    var qnaClient = GetQnAMakerClient(stepContext);
                    await qnaClient.CallTrainAsync(feedbackRecords).ConfigureAwait(false);

                    return await stepContext.NextAsync(new List<QueryResult>() { qnaResult }, cancellationToken).ConfigureAwait(false);
                }
                else if (reply.Equals(dialogOptions.ResponseOptions.CardNoMatchText, StringComparison.OrdinalIgnoreCase))
                {
                    var activity = dialogOptions.ResponseOptions.CardNoMatchResponse;
                    if (activity == null)
                    {
                        await stepContext.Context.SendActivityAsync(DefaultCardNoMatchResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }

                    return await stepContext.EndDialogAsync().ConfigureAwait(false);
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(this.Id, stepContext.ActiveDialog.State["options"], cancellationToken).ConfigureAwait(false);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CheckForMultiTurnPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = stepContext.State.GetValue<QnAMakerDialogOptions>(ThisPath.OPTIONS);
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                // -Check if context is present and prompt exists 
                // -If yes: Add reverse index of prompt display name and its corresponding qna id
                // -Set PreviousQnAId as answer.Id
                // -Display card for the prompt
                // -Wait for the reply
                // -If no: Skip to next step

                var answer = response.First();

                if (answer.Context != null && answer.Context.Prompts.Count() > 0)
                {
                    var previousContextData = stepContext.State.GetValue(QnAPath.QnAContextData, () => new Dictionary<string, int>());
                    var previousQnAId = stepContext.State.GetIntValue(QnAPath.PreviousQnAId, 0);

                    foreach (var prompt in answer.Context.Prompts)
                    {
                        previousContextData.Add(prompt.DisplayText, prompt.QnaId);
                    }

                    stepContext.State.SetValue(QnAPath.QnAContextData, previousContextData);
                    stepContext.State.SetValue(QnAPath.PreviousQnAId, answer.Id);
                    stepContext.State.SetValue(ThisPath.OPTIONS, dialogOptions);

                    // Get multi-turn prompts card activity.
                    var message = QnACardBuilder.GetQnAPromptsCard(answer, dialogOptions.ResponseOptions.CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> DisplayQnAResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = stepContext.State.GetValue<QnAMakerDialogOptions>(ThisPath.OPTIONS);
            var reply = stepContext.Context.Activity.Text;

            if (reply.Equals(dialogOptions.ResponseOptions.CardNoMatchText, StringComparison.OrdinalIgnoreCase))
            {
                var activity = dialogOptions.ResponseOptions.CardNoMatchResponse;
                if (activity == null)
                {
                    await stepContext.Context.SendActivityAsync(DefaultCardNoMatchResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                return await stepContext.EndDialogAsync().ConfigureAwait(false);
            }

            // If previous QnAId is present, replace the dialog
            var previousQnAId = stepContext.State.GetIntValue(QnAPath.PreviousQnAId, 0);
            if (previousQnAId > 0)
            {
                return await stepContext.ReplaceDialogAsync(this.Id, dialogOptions, cancellationToken).ConfigureAwait(false);
            }

            // If response is present then show that response, else default answer.
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(response.First().Answer, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var activity = dialogOptions.ResponseOptions.NoAnswer;
                if (activity == null)
                {
                    await stepContext.Context.SendActivityAsync(DefaultNoAnswer, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(activity, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }

        internal class QnAPath
        {
            internal const string QnAContextData = "this.qnaContextData";
            internal const string PreviousQnAId = "this.prevQnAId";
        }

        internal class ValueProperty
        {
            internal const string CurrentQuery = "currentQuery";
            internal const string QnAData = "qnaData";
        }
    }
}
