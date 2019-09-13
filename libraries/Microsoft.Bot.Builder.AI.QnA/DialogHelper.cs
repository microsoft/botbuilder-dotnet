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
    /// QnAMaker Active Learning Dialog helper class
    /// </summary>
    public class DialogHelper
    {
        /// <summary>
        /// QnA Maker Active Learning dialog name
        /// </summary>
        public const string ActiveLearningDialogName = "active-learning-dialog";

        /// <summary>
        /// Gets QnA Maker Active Learning Dialog.
        /// </summary>
        public WaterfallDialog QnAMakerActiveLearningDialog { get; }
        
        // Define value names for values tracked inside the dialogs.
        private const string CurrentQuery = "value-current-query";
        private const string QnAData = "value-qnaData";

        // Dialog Options parameters
        private const float DefaultThreshold = 0.03F;
        private const int DefaultTopN = 3;

        // Card parameters
        private const string CardTitle = "Did you mean:";
        private const string CardNoMatchText = "None of the above.";
        private const string CardNoMatchResponse = "Thanks for the feedback.";

        private readonly QnAMaker _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogHelper"/> class.
        /// Dialog helper to generate dialogs.
        /// </summary>
        /// <param name="services">Bot Services.</param>
        public DialogHelper(QnAMaker services)
        {
            QnAMakerActiveLearningDialog = new WaterfallDialog(ActiveLearningDialogName)
                .AddStep(CallGenerateAnswer)
                .AddStep(CallTrain)
                .AddStep(DisplayQnAResult);
            _services = services;
        }

        private async Task<DialogTurnResult> CallGenerateAnswer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
            // -Check for id if query is present in reverse index

            var response = await _services.GetAnswersAsync(stepContext.Context, qnaMakerOptions).ConfigureAwait(false);
            
            // TODO: Take this value from GetAnswerResponse 
            var isActiveLearningEnabled = true;
            var filteredResponse = response;
            stepContext.Values[QnAData] = new List<QueryResult>(filteredResponse);

            if (isActiveLearningEnabled)
            {
                filteredResponse = _services.GetLowScoreVariation(response.ToArray());

                if (filteredResponse.ToList().Count > 1)
                {
                    var suggestedQuestions = new List<string>();
                    foreach (var qna in filteredResponse)
                    {
                        suggestedQuestions.Add(qna.Questions[0]);
                    }

                    // Get hero card activity
                    var message = CardHelper.GetSuggestionsCard(suggestedQuestions, CardTitle, CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }
            else
            {
                filteredResponse = new QueryResult[] { response.FirstOrDefault() };
            }

            return await stepContext.NextAsync(new List<QueryResult>(filteredResponse), cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CallTrain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var trainResponses = stepContext.Values[QnAData] as List<QueryResult>;
            var currentQuery = stepContext.Values[CurrentQuery] as string;

            var reply = stepContext.Context.Activity.Text;

            if (trainResponses.Count > 1)
            {
                var qnaResult = trainResponses.Where(kvp => kvp.Questions[0] == reply).FirstOrDefault();

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
                    return await stepContext.ReplaceDialogAsync(ActiveLearningDialogName, stepContext.ActiveDialog.State["options"], cancellationToken).ConfigureAwait(false);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CheckForMultiTurnPrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                // -Check if prompt exists 
                // -If yes: Add reverse index of prompt display name and its corresponding qna id
                // -Set PreviousQnAId as response[0].Id
                // -Set PreviousQuery as stepContext.Values[CurrentQuery]
                // -Display card for the prompt
                // -Wait for the reply
                // -If no: Skip to next step
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> DisplayQnAResult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(response[0].Answer, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var msg = "No QnAMaker answers found.";
                await stepContext.Context.SendActivityAsync(msg, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // -Clean previous id, query and reverse index
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
