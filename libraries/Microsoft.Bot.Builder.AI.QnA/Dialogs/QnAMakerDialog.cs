// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.QnA.Utils;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Dialogs
{
    /// <summary>
    /// A dialog that supports multi-step and adaptive-learning QnA Maker services.
    /// </summary>
    /// <remarks>An instance of this class targets a specific QnA Maker knowledge base.
    /// It supports knowledge bases that include follow-up prompt and active learning features.</remarks>
    public class QnAMakerDialog : WaterfallDialog
    {
        /// <summary>
        /// The declarative name for this type.
        /// </summary>
        /// <remarks>Used by the framework to serialize and deserialize an instance of this type to JSON.</remarks>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.QnAMakerDialog";

        /// <summary>
        /// The path for storing and retrieving QnA Maker context data.
        /// </summary>
        /// <remarks>This represents context about the current or previous call to QnA Maker.
        /// It is stored within the current step's <see cref="WaterfallStepContext"/>.
        /// It supports QnA Maker's follow-up prompt and active learning features.</remarks>
        protected const string QnAContextData = "qnaContextData";

        /// <summary>
        /// The path for storing and retrieving the previous question ID.
        /// </summary>
        /// <remarks>This represents the QnA question ID from the previous turn.
        /// It is stored within the current step's <see cref="WaterfallStepContext"/>.
        /// It supports QnA Maker's follow-up prompt and active learning features.</remarks>
        protected const string PreviousQnAId = "prevQnAId";

        /// <summary>
        /// The path for storing and retrieving the options for this instance of the dialog.
        /// </summary>
        /// <remarks>This includes the options with which the dialog was started and options
        /// expected by the QnA Maker service.
        /// It is stored within the current step's <see cref="WaterfallStepContext"/>.
        /// It supports QnA Maker and the dialog system.</remarks>
        protected const string Options = "options";

        // Dialog Options parameters

        /// <summary>
        /// The default threshold for answers returned, based on score.
        /// </summary>
        protected const float DefaultThreshold = 0.3F;

        /// <summary>
        /// The default maximum number of answers to be returned for the question.
        /// </summary>
        protected const int DefaultTopN = 3;

        private const string DefaultNoAnswer = "No QnAMaker answers found.";

        // Card parameters
        private const string DefaultCardTitle = "Did you mean:";
        private const string DefaultCardNoMatchText = "None of the above.";
        private const string DefaultCardNoMatchResponse = "Thanks for the feedback.";

        private float maximumScoreForLowScoreVariation = 0.95F;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerDialog"/> class.
        /// </summary>
        /// <param name="knowledgeBaseId">The ID of the QnA Maker knowledge base to query.</param>
        /// <param name="endpointKey">The QnA Maker endpoint key to use to query the knowledge base.</param>
        /// <param name="hostName">The QnA Maker host URL for the knowledge base, starting with "https://" and
        /// ending with "/qnamaker".</param>
        /// <param name="noAnswer">The activity to send the user when QnA Maker does not find an answer.</param>
        /// <param name="threshold">The threshold for answers returned, based on score.</param>
        /// <param name="activeLearningCardTitle">The card title to use when showing active learning options
        /// to the user, if active learning is enabled.</param>
        /// <param name="cardNoMatchText">The button text to use with active learning options,
        /// allowing a user to indicate none of the options are applicable.</param>
        /// <param name="top">The maximum number of answers to return from the knowledge base.</param>
        /// <param name="cardNoMatchResponse">The activity to send the user if they select the no match option
        /// on an active learning card.</param>
        /// <param name="strictFilters">QnA Maker metadata with which to filter or boost queries to the
        /// knowledge base; or null to apply none.</param>
        /// <param name="httpClient">An HTTP client to use for requests to the QnA Maker Service;
        /// or `null` to use a default client.</param>
        /// <param name="sourceFilePath">The source file path, for debugging. Defaults to the full path
        /// of the source file that contains the caller.</param>
        /// <param name="sourceLineNumber">The line number, for debugging. Defaults to the line number
        /// in the source file at which the method is called.</param>
        public QnAMakerDialog(
            string knowledgeBaseId,
            string endpointKey,
            string hostName,
            Activity noAnswer = null,
            float threshold = DefaultThreshold,
            string activeLearningCardTitle = DefaultCardTitle,
            string cardNoMatchText = DefaultCardNoMatchText,
            int top = DefaultTopN,
            Activity cardNoMatchResponse = null,
            Metadata[] strictFilters = null,
            HttpClient httpClient = null,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
            : base(nameof(QnAMakerDialog))
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            this.KnowledgeBaseId = knowledgeBaseId ?? throw new ArgumentNullException(nameof(knowledgeBaseId));
            this.HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            this.EndpointKey = endpointKey ?? throw new ArgumentNullException(nameof(endpointKey));
            this.Threshold = threshold;
            this.Top = top;
            this.ActiveLearningCardTitle = activeLearningCardTitle;
            this.CardNoMatchText = cardNoMatchText;
            this.StrictFilters = strictFilters;
            this.NoAnswer = new BindToActivity(noAnswer ?? MessageFactory.Text(DefaultNoAnswer));
            this.CardNoMatchResponse = new BindToActivity(cardNoMatchResponse ?? MessageFactory.Text(DefaultCardNoMatchResponse));
            this.HttpClient = httpClient;

            // add waterfall steps
            this.AddStep(CallGenerateAnswerAsync);
            this.AddStep(CallTrainAsync);
            this.AddStep(CheckForMultiTurnPromptAsync);
            this.AddStep(DisplayQnAResultAsync);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerDialog"/> class.
        /// The JSON serializer uses this constructor to deserialize objects of this class.
        /// </summary>
        /// <param name="sourceFilePath">The source file path, for debugging. Defaults to the full path
        /// of the source file that contains the caller.</param>
        /// <param name="sourceLineNumber">The line number, for debugging. Defaults to the line number
        /// in the source file at which the method is called.</param>
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

        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> instance to use for requests to the QnA Maker service.
        /// </summary>
        /// <value>The HTTP client.</value>
        [JsonIgnore]
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker knowledge base ID to query.
        /// </summary>
        /// <value>
        /// The knowledge base ID or an expression which evaluates to the knowledge base ID.
        /// </value>
        [JsonProperty("knowledgeBaseId")]
        public StringExpression KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker host URL for the knowledge base.
        /// </summary>
        /// <value>
        /// The QnA Maker host URL or an expression which evaluates to the host URL.
        /// </value>
        [JsonProperty("hostname")]
        public StringExpression HostName { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker endpoint key to use to query the knowledge base.
        /// </summary>
        /// <value>
        /// The QnA Maker endpoint key to use or an expression which evaluates to the endpoint key.
        /// </value>
        [JsonProperty("endpointKey")]
        public StringExpression EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the threshold for answers returned, based on score.
        /// </summary>
        /// <value>
        /// The threshold for answers returned or an expression which evaluates to the threshold.
        /// </value>
        [JsonProperty("threshold")]
        public NumberExpression Threshold { get; set; } = DefaultThreshold;

        /// <summary>
        /// Gets or sets the maximum number of answers to return from the knowledge base.
        /// </summary>
        /// <value>
        /// The maximum number of answers to return from the knowledge base or an expression which
        /// evaluates to the maximum number to return.
        /// </value>
        [JsonProperty("top")]
        public IntExpression Top { get; set; } = DefaultTopN;

        /// <summary>
        /// Gets or sets the template to send the user when QnA Maker does not find an answer.
        /// </summary>
        /// <value>
        /// The template to send the user when QnA Maker does not find an answer.
        /// </value>
        [JsonProperty("noAnswer")]
        public ITemplate<Activity> NoAnswer { get; set; } = new BindToActivity(MessageFactory.Text(DefaultNoAnswer));

        /// <summary>
        /// Gets or sets the card title to use when showing active learning options to the user,
        /// if active learning is enabled.
        /// </summary>
        /// <value>
        /// The path card title to use when showing active learning options to the user or an
        /// expression which evaluates to the card title.
        /// </value>
        [JsonProperty("activeLearningCardTitle")]
        public StringExpression ActiveLearningCardTitle { get; set; }

        /// <summary>
        /// Gets or sets the button text to use with active learning options, allowing a user to
        /// indicate none of the options are applicable.
        /// </summary>
        /// <value>
        /// The button text to use with active learning options or an expression which evaluates to
        /// the button text.
        /// </value>
        [JsonProperty("cardNoMatchText")]
        public StringExpression CardNoMatchText { get; set; }

        /// <summary>
        /// Gets or sets the template to send the user if they select the no match option on an
        /// active learning card.
        /// </summary>
        /// <value>
        /// The template to send the user if they select the no match option on an active learning card.
        /// </value>
        [JsonProperty("cardNoMatchResponse")]
        public ITemplate<Activity> CardNoMatchResponse { get; set; } = new BindToActivity(MessageFactory.Text(DefaultCardNoMatchResponse));

        /// <summary>
        /// Gets or sets the QnA Maker metadata with which to filter or boost queries to the knowledge base;
        /// or null to apply none.
        /// </summary>
        /// <value>
        /// The QnA Maker metadata with which to filter or boost queries to the knowledge base
        /// or an expression which evaluates to the QnA Maker metadata.
        /// </value>
        [JsonProperty("strictFilters")]
        public ArrayExpression<Metadata> StrictFilters { get; set; }

        /// <summary>
        /// Gets or sets the flag to determine if personal information should be logged in telemetry.
        /// </summary>
        /// <value>
        /// The flag to indicate in personal information should be logged in telemetry.
        /// </value>
        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; } = "=settings.telemetry.logPersonalInformation";

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets environment of knowledgebase to be called. 
        /// </summary>
        /// <value>
        /// A value indicating whether to call test or prod environment of knowledge base. 
        /// </value>
        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker ranker type to use.
        /// </summary>
        /// <value>
        /// The QnA Maker ranker type to use or an expression which evaluates to the ranker type.
        /// </value>
        /// <seealso cref="RankerTypes"/>
        [JsonProperty("rankerType")]
        public StringExpression RankerType { get; set; } = new StringExpression(RankerTypes.DefaultRankerType);

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.
        /// 
        /// You can use the <paramref name="options"/> parameter to include the QnA Maker context data,
        /// which represents context from the previous query. To do so, the value should include a
        /// `context` property of type <see cref="QnAResponseContext"/>.</remarks>
        /// <seealso cref="DialogContext.BeginDialogAsync(string, object, CancellationToken)"/>
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
                QnAMakerOptions = await GetQnAMakerOptionsAsync(dc).ConfigureAwait(false),
                ResponseOptions = await GetQnAResponseOptionsAsync(dc).ConfigureAwait(false)
            };

            if (options != null)
            {
                dialogOptions = ObjectPath.Assign<QnAMakerDialogOptions>(dialogOptions, options);
            }

            ObjectPath.SetPathValue(dc.ActiveDialog.State, Options, dialogOptions);

            return await base.BeginDialogAsync(dc, dialogOptions, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an <see cref="IQnAMakerClient"/> to use to access the QnA Maker knowledge base.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the QnA Maker client to use.</remarks>
        protected async virtual Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
        {
            var qnaClient = dc.Context.TurnState.Get<IQnAMakerClient>();
            if (qnaClient != null)
            {
                // return mock client
                return qnaClient;
            }

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = this.EndpointKey.GetValue(dc.State),
                Host = this.HostName.GetValue(dc.State),
                KnowledgeBaseId = this.KnowledgeBaseId.GetValue(dc.State)
            };
            var options = await GetQnAMakerOptionsAsync(dc).ConfigureAwait(false);
            return new QnAMaker(endpoint, options, HttpClient, this.TelemetryClient, this.LogPersonalInformation.GetValue(dc.State));
        }

        /// <summary>
        /// Gets the options for the QnA Maker client that the dialog will use to query the knowledge base.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the QnA Maker options to use.</remarks>
        protected virtual Task<QnAMakerOptions> GetQnAMakerOptionsAsync(DialogContext dc)
        {
            return Task.FromResult(new QnAMakerOptions
            {
                ScoreThreshold = this.Threshold.GetValue(dc.State),
                StrictFilters = this.StrictFilters?.GetValue(dc.State)?.ToArray(),
                Top = this.Top.GetValue(dc.State),
                Context = new QnARequestContext(),
                QnAId = 0,
                RankerType = this.RankerType?.GetValue(dc.State),
                IsTest = this.IsTest
            });
        }

        /// <summary>
        /// Gets the options the dialog will use to display query results to the user.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the response options to use.</remarks>
        protected async virtual Task<QnADialogResponseOptions> GetQnAResponseOptionsAsync(DialogContext dc)
        {
            return new QnADialogResponseOptions
            {
                NoAnswer = await this.NoAnswer.BindAsync(dc, dc.State).ConfigureAwait(false),
                ActiveLearningCardTitle = this.ActiveLearningCardTitle?.GetValue(dc.State) ?? DefaultCardTitle,
                CardNoMatchText = this.CardNoMatchText?.GetValue(dc.State) ?? DefaultCardNoMatchText,
                CardNoMatchResponse = await this.CardNoMatchResponse.BindAsync(dc).ConfigureAwait(false)
            };
        }

        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = ObjectPath.GetPathValue<QnAMakerDialogOptions>(stepContext.ActiveDialog.State, Options);

            // Resetting context and QnAId
            dialogOptions.QnAMakerOptions.QnAId = 0;
            dialogOptions.QnAMakerOptions.Context = new QnARequestContext();

            // Storing the context info
            stepContext.Values[ValueProperty.CurrentQuery] = stepContext.Context.Activity.Text;

            // -Check if previous context is present, if yes then put it with the query
            // -Check for id if query is present in reverse index.
            var previousContextData = ObjectPath.GetPathValue<Dictionary<string, int>>(stepContext.ActiveDialog.State, QnAContextData, new Dictionary<string, int>());
            var previousQnAId = ObjectPath.GetPathValue<int>(stepContext.ActiveDialog.State, PreviousQnAId, 0);

            if (previousQnAId > 0)
            {
                dialogOptions.QnAMakerOptions.Context = new QnARequestContext
                {
                    PreviousQnAId = previousQnAId
                };

                if (previousContextData.TryGetValue(stepContext.Context.Activity.Text, out var currentQnAId))
                {
                    dialogOptions.QnAMakerOptions.QnAId = currentQnAId;
                }
            }

            // Calling QnAMaker to get response.
            var qnaClient = await GetQnAMakerClientAsync(stepContext).ConfigureAwait(false);
            var response = await qnaClient.GetAnswersRawAsync(stepContext.Context, dialogOptions.QnAMakerOptions).ConfigureAwait(false);

            // Resetting previous query.
            previousQnAId = -1;
            ObjectPath.SetPathValue(stepContext.ActiveDialog.State, PreviousQnAId, previousQnAId);

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

                    ObjectPath.SetPathValue(stepContext.ActiveDialog.State, Options, dialogOptions);
                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            var result = new List<QueryResult>();
            if (response.Answers.Any())
            {
                result.Add(response.Answers.First());
            }

            stepContext.Values[ValueProperty.QnAData] = result;
            ObjectPath.SetPathValue(stepContext.ActiveDialog.State, Options, dialogOptions);

            // If card is not shown, move to next step with top QnA response.
            return await stepContext.NextAsync(result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CallTrainAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = ObjectPath.GetPathValue<QnAMakerDialogOptions>(stepContext.ActiveDialog.State, Options);
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
                    var qnaClient = await GetQnAMakerClientAsync(stepContext).ConfigureAwait(false);
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
                    // restart the waterfall to step 0
                    return await RunStepAsync(stepContext, index: 0, reason: DialogReason.BeginCalled, result: null, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> CheckForMultiTurnPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dialogOptions = ObjectPath.GetPathValue<QnAMakerDialogOptions>(stepContext.ActiveDialog.State, Options);
            if (stepContext.Result is List<QueryResult> response && response.Count > 0)
            {
                // -Check if context is present and prompt exists 
                // -If yes: Add reverse index of prompt display name and its corresponding QnA ID
                // -Set PreviousQnAId as answer.Id
                // -Display card for the prompt
                // -Wait for the reply
                // -If no: Skip to next step

                var answer = response.First();

                if (answer.Context != null && answer.Context.Prompts.Count() > 0)
                {
                    var previousContextData = ObjectPath.GetPathValue(stepContext.ActiveDialog.State, QnAContextData, new Dictionary<string, int>());
                    var previousQnAId = ObjectPath.GetPathValue<int>(stepContext.ActiveDialog.State, PreviousQnAId, 0);

                    foreach (var prompt in answer.Context.Prompts)
                    {
                        previousContextData[prompt.DisplayText] = prompt.QnaId;
                    }

                    ObjectPath.SetPathValue(stepContext.ActiveDialog.State, QnAContextData, previousContextData);
                    ObjectPath.SetPathValue(stepContext.ActiveDialog.State, PreviousQnAId, answer.Id);
                    ObjectPath.SetPathValue(stepContext.ActiveDialog.State, Options, dialogOptions);

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
            var dialogOptions = ObjectPath.GetPathValue<QnAMakerDialogOptions>(stepContext.ActiveDialog.State, Options);
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
            var previousQnAId = ObjectPath.GetPathValue<int>(stepContext.ActiveDialog.State, PreviousQnAId, 0);
            if (previousQnAId > 0)
            {
                // restart the waterfall to step 0
                return await RunStepAsync(stepContext, index: 0, reason: DialogReason.BeginCalled, result: null, cancellationToken: cancellationToken).ConfigureAwait(false);
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

        internal class ValueProperty
        {
            internal const string CurrentQuery = "currentQuery";
            internal const string QnAData = "qnaData";
        }
    }
}
