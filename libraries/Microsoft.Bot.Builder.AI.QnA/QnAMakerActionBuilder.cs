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
    internal class QnAMakerActionBuilder
    {
        /// <summary>
        /// QnA Maker action builder
        /// </summary>
        internal const string QnAMakerDialogName = "qnamaker-dialog";

        // Dialog Options parameters
        internal const float DefaultThreshold = 0.3F;
        internal const int DefaultTopN = 3;
        internal const string DefaultNoAnswer = "No QnAMaker answers found.";

        // Card parameters
        internal const string DefaultCardTitle = "Did you mean:";
        internal const string DefaultCardNoMatchText = "None of the above.";
        internal const string DefaultCardNoMatchResponse = "Thanks for the feedback.";

        // Define value names for values tracked inside the dialogs.
        internal const string QnAOptions = "qnaOptions";
        internal const string QnADialogResponseOptions = "qnaDialogResponseOptions";
        private const string CurrentQuery = "currentQuery";
        private const string QnAData = "qnaData";
        private const string QnAContextData = "qnaContextData";
        private const string PreviousQnAId = "prevQnAId";

        private readonly QnAMaker _services;

        /// <summary>
        /// Gets QnA Maker Dialog.
        /// </summary>
        private readonly WaterfallDialog _qnaMakerDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerActionBuilder"/> class.
        /// Dialog helper to generate dialogs.
        /// </summary>
        /// <param name="services">Bot Services.</param>
        internal QnAMakerActionBuilder(QnAMaker services)
        {
            _qnaMakerDialog = new WaterfallDialog(QnAMakerDialogName)
                .AddStep(CallGenerateAnswerAsync)
                .AddStep(CallTrain)
                .AddStep(CheckForMultiTurnPrompt)
                .AddStep(DisplayQnAResult);
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Build dialog by adding QnAMaker dialog to dialog context.
        /// </summary>
        /// <param name="dc">DialogContext</param>
        /// <returns>Updated dialog context.</returns>
        internal DialogContext BuildDialog(DialogContext dc)
        {
            if (dc == null)
            {
                return dc;
            }

            dc.Dialogs.Add(_qnaMakerDialog);
            return dc;
        }

        private static Dictionary<string, object> GetDialogOptionsValue(DialogContext dialogContext)
        {
            var dialogOptions = new Dictionary<string, object>();

            if (dialogContext.ActiveDialog.State["options"] != null)
            {
                dialogOptions = dialogContext.ActiveDialog.State["options"] as Dictionary<string, object>;
            }

            return dialogOptions;
        }

        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var qnaMakerOptions = new QnAMakerOptions
            {
                ScoreThreshold = DefaultThreshold,
                Top = DefaultTopN
            };

            var dialogOptions = GetDialogOptionsValue(stepContext);

            // Getting options
            if (dialogOptions.ContainsKey(QnAOptions))
            {
                qnaMakerOptions = dialogOptions[QnAOptions] as QnAMakerOptions;
                qnaMakerOptions.ScoreThreshold = qnaMakerOptions?.ScoreThreshold ?? DefaultThreshold;
                qnaMakerOptions.Top = DefaultTopN;
            }

            // Storing the context info
            stepContext.Values[CurrentQuery] = stepContext.Context.Activity.Text;

            // -Check if previous context is present, if yes then put it with the query
            // -Check for id if query is present in reverse index.
            if (!dialogOptions.ContainsKey(QnAContextData))
            {
                dialogOptions[QnAContextData] = new Dictionary<string, int>();
            }
            else
            {
                var previousContextData = dialogOptions[QnAContextData] as Dictionary<string, int>;
                if (dialogOptions[PreviousQnAId] != null)
                {
                    var previousQnAId = Convert.ToInt32(dialogOptions[PreviousQnAId]);

                    if (previousQnAId > 0)
                    {
                        qnaMakerOptions.Context = new QnARequestContext
                        {
                            PreviousQnAId = previousQnAId
                        };

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
            dialogOptions[PreviousQnAId] = -1;
            stepContext.ActiveDialog.State["options"] = dialogOptions;

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
                    var qnaDialogResponseOptions = dialogOptions[QnADialogResponseOptions] as QnADialogResponseOptions;
                    var message = QnACardBuilder.GetSuggestionsCard(suggestedQuestions, qnaDialogResponseOptions.ActiveLearningCardTitle, qnaDialogResponseOptions.CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            var result = new List<QueryResult>();
            if (response.Any())
            {
                result.Add(response.First());
            }

            stepContext.Values[QnAData] = result;

            // If card is not shown, move to next step with top qna response.
            return await stepContext.NextAsync(result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CallTrain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var trainResponses = stepContext.Values[QnAData] as List<QueryResult>;
            var currentQuery = stepContext.Values[CurrentQuery] as string;

            var reply = stepContext.Context.Activity.Text;

            var dialogOptions = GetDialogOptionsValue(stepContext);
            var qnaDialogResponseOptions = dialogOptions[QnADialogResponseOptions] as QnADialogResponseOptions;

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
                else if (reply.Equals(qnaDialogResponseOptions.CardNoMatchText, StringComparison.OrdinalIgnoreCase))
                {
                    var activity = await qnaDialogResponseOptions.CardNoMatchResponse.BindToData(stepContext.Context, stepContext.State).ConfigureAwait(false);
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

                var answer = response.First();

                if (answer.Context != null && answer.Context.Prompts.Count() > 1)
                {
                    var dialogOptions = GetDialogOptionsValue(stepContext);
                    var qnaDialogResponseOptions = dialogOptions[QnADialogResponseOptions] as QnADialogResponseOptions;
                    var previousContextData = new Dictionary<string, int>();
                    if (dialogOptions.ContainsKey(QnAContextData))
                    {
                        previousContextData = dialogOptions[QnAContextData] as Dictionary<string, int>;
                    }

                    foreach (var prompt in answer.Context.Prompts)
                    {
                        previousContextData.Add(prompt.DisplayText, prompt.QnaId);
                    }

                    dialogOptions[QnAContextData] = previousContextData;
                    dialogOptions[PreviousQnAId] = answer.Id;
                    stepContext.ActiveDialog.State["options"] = dialogOptions;

                    // Get multi-turn prompts card activity.
                    var message = QnACardBuilder.GetQnAPromptsCard(answer, qnaDialogResponseOptions.CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> DisplayQnAResult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = GetDialogOptionsValue(stepContext);
            var qnaDialogResponseOptions = dialogOptions[QnADialogResponseOptions] as QnADialogResponseOptions;
            var reply = stepContext.Context.Activity.Text;

            if (reply.Equals(qnaDialogResponseOptions.CardNoMatchText, StringComparison.OrdinalIgnoreCase))
            {
                var activity = await qnaDialogResponseOptions.CardNoMatchResponse.BindToData(stepContext.Context, stepContext.State).ConfigureAwait(false);
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
            var previousQnAId = Convert.ToInt32(dialogOptions[PreviousQnAId]);
            if (previousQnAId > 0)
            {
                return await stepContext.ReplaceDialogAsync(QnAMakerDialogName, dialogOptions, cancellationToken).ConfigureAwait(false);
            }

            // If response is present then show that response, else default answer.
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(response.First().Answer, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var activity = await qnaDialogResponseOptions.NoAnswer.BindToData(stepContext.Context, stepContext.State).ConfigureAwait(false);
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
    }
}
