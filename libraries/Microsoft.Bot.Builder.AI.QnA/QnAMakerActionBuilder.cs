// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAMaker action builder class
    /// </summary>
    public class QnAMakerActionBuilder
    {
        /// <summary>
        /// QnA Maker action auilder
        /// </summary>
        public const string QnAMakerDialogName = "active-learning-dialog";

        // Define value names for values tracked inside the dialogs.
        private const string CurrentQuery = "value-current-query";
        private const string QnAData = "value-qnaData";
        private const string QnAContextData = "value-qnaContextData";
        private const string PreviousQnAId = "value-prevQnAId";

        // Dialog Options parameters
        private const float DefaultThreshold = 0.3F;
        private const int DefaultTopN = 3;

        // Card parameters
        private const string CardTitle = "Did you mean:";
        private const string CardNoMatchText = "None of the above.";
        private const string CardNoMatchResponse = "Thanks for the feedback.";

        private readonly QnAMaker _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerActionBuilder"/> class.
        /// Dialog helper to generate dialogs.
        /// </summary>
        /// <param name="services">Bot Services.</param>
        public QnAMakerActionBuilder(QnAMaker services)
        {
            QnAMakerDialog = new WaterfallDialog(QnAMakerDialogName)
                .AddStep(CallGenerateAnswerAsync)
                .AddStep(CallTrain)
                .AddStep(CheckForMultiTurnPrompt)
                .AddStep(DisplayQnAResult);
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets QnA Maker Dialog.
        /// </summary>
        private WaterfallDialog QnAMakerDialog { get; }

        /// <summary>
        /// Build dialog by adding QnAMaker dialog to dialog context.
        /// </summary>
        /// <param name="dc">DialogContext</param>
        /// <returns>Updated dialog context.</returns>
        public DialogContext BuildDialog(DialogContext dc)
        {
            if (dc == null)
            {
                return dc;
            }

            dc.Dialogs.Add(QnAMakerDialog);
            return dc;
        }

        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var scoreThreshold = DefaultThreshold;
            var top = DefaultTopN;

            QnAMakerOptions qnaMakerOptions = null;

            // Getting options
            if (stepContext.ActiveDialog.State["options"] != null)
            {
                qnaMakerOptions = stepContext.ActiveDialog.State["options"] as QnAMakerOptions;
                scoreThreshold = qnaMakerOptions?.ScoreThreshold != null ? qnaMakerOptions.ScoreThreshold : DefaultThreshold;
                top = qnaMakerOptions?.Top != null ? qnaMakerOptions.Top : DefaultTopN;
            }
            
            // Storing the context info
            stepContext.Values[CurrentQuery] = stepContext.Context.Activity.Text;

            // -Check if previous context is present, if yes then put it with the query
            // -Check for id if query is present in reverse index.
            if (!stepContext.ActiveDialog.State.ContainsKey(QnAContextData))
            {
                stepContext.ActiveDialog.State[QnAContextData] = new Dictionary<string, int>();
            }
            else
            {
                var previousContextData = stepContext.ActiveDialog.State[QnAContextData] as Dictionary<string, int>;
                if (stepContext.ActiveDialog.State[PreviousQnAId] != null)
                {
                    var previousQnAId = Convert.ToInt32(stepContext.ActiveDialog.State[PreviousQnAId]);

                    if (previousQnAId > 0)
                    {
                        qnaMakerOptions.Context.PreviousQnAId = previousQnAId;

                        if (previousContextData.TryGetValue(stepContext.Context.Activity.Text, out var currentQnAId))
                        {
                            qnaMakerOptions.QnAId = currentQnAId;
                        }
                    }
                }
            }

            // Calling QnAMaker to get response.
            var response = await _services.GetAnswersAsync(stepContext.Context, qnaMakerOptions).ConfigureAwait(false);

            // Resetting previous query.
            stepContext.ActiveDialog.State[PreviousQnAId] = -1;

            // Take this value from GetAnswerResponse 
            var isActiveLearningEnabled = true;

            stepContext.Values[QnAData] = new List<QueryResult>(response);

            // Check if active learning is enabled.
            if (isActiveLearningEnabled)
            {
                // Get filtered list of the response that support low score variation criteria.
                response = _services.GetLowScoreVariation(response);

                if (response.Count() > 1)
                {
                    var suggestedQuestions = new List<string>();
                    foreach (var qna in response)
                    {
                        suggestedQuestions.Add(qna.Questions[0]);
                    }

                    // Get active learning suggestion card activity.
                    var message = QnACardBuilder.GetSuggestionsCard(suggestedQuestions, CardTitle, CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            var result = new List<QueryResult>();
            if (response.Count() > 0)
            {
                result.Add(response.FirstOrDefault());
            }

            // If card is not shown, move to next step with top qna response.
            return await stepContext.NextAsync(result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CallTrain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var trainResponses = stepContext.Values[QnAData] as List<QueryResult>;
            var currentQuery = stepContext.Values[CurrentQuery] as string;

            var reply = stepContext.Context.Activity.Text;

            if (trainResponses.Count > 1)
            {
                var qnaResult = trainResponses.FirstOrDefault(kvp => kvp.Questions[0] == reply);

                if (qnaResult != null)
                {
                    stepContext.Values[QnAData] = new List<QueryResult>() { qnaResult };

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
                    await _services.CallTrainAsync(feedbackRecords).ConfigureAwait(false);

                    return await stepContext.NextAsync(new List<QueryResult>() { qnaResult }, cancellationToken).ConfigureAwait(false);
                }
                else if (reply.Equals(CardNoMatchText))
                {
                    await stepContext.Context.SendActivityAsync(CardNoMatchResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return await stepContext.EndDialogAsync().ConfigureAwait(false);
                }
                else
                {
                    return await stepContext.ReplaceDialogAsync(QnAMakerDialogName, stepContext.ActiveDialog.State["options"], cancellationToken).ConfigureAwait(false);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CheckForMultiTurnPrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                // -Check if context is present and prompt exists 
                // -If yes: Add reverse index of prompt display name and its corresponding qna id
                // -Set PreviousQnAId as answer.Id
                // -Display card for the prompt
                // -Wait for the reply
                // -If no: Skip to next step

                var answer = response.FirstOrDefault();

                if (answer.Context != null && answer.Context.Prompts.Count() > 1)
                {
                    var previousContextData = new Dictionary<string, int>();
                    if (stepContext.ActiveDialog.State.ContainsKey(QnAContextData))
                    {
                        previousContextData = stepContext.ActiveDialog.State[QnAContextData] as Dictionary<string, int>;
                    }

                    foreach (var prompt in answer.Context.Prompts)
                    {
                        previousContextData.Add(prompt.DisplayText, prompt.QnaId);
                    }

                    stepContext.ActiveDialog.State[QnAContextData] = previousContextData;
                    stepContext.ActiveDialog.State[PreviousQnAId] = answer.Id;

                    // Get multi-turn prompts card activity.
                    var message = QnACardBuilder.GetQnAPromptsCard(answer, CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> DisplayQnAResult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If previous QnAId is present, replace the dialog
            var previousQnAId = Convert.ToInt32(stepContext.ActiveDialog.State[PreviousQnAId]);
            if (previousQnAId > 0)
            {
                return await stepContext.ReplaceDialogAsync(QnAMakerDialogName, stepContext.ActiveDialog.State["options"], cancellationToken).ConfigureAwait(false);
            }

            var reply = stepContext.Context.Activity.Text;
            if (reply.Equals(CardNoMatchText))
            {
                await stepContext.Context.SendActivityAsync(CardNoMatchResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
                return await stepContext.EndDialogAsync().ConfigureAwait(false);
            }

            // If response is present then show that response, else default answer.
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(response[0].Answer, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var msg = "No QnAMaker answers found.";
                if (stepContext.ActiveDialog.State["options"] != null)
                {
                    var qnaMakerOptions = stepContext.ActiveDialog.State["options"] as QnAMakerOptions;
                    msg = qnaMakerOptions.NoAnswer ?? msg;
                }

                await stepContext.Context.SendActivityAsync(msg, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
