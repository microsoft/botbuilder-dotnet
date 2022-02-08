﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

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
        /// The path for storing and retrieving QnA Maker suggested questions data.
        /// </summary>
        /// <remarks>This represents suggestions returned from QnA Maker when ActiveLearningEnabled is enabled.
        /// It is stored within the active dialog's state.
        /// It supports QnA Maker's follow-up prompt and active learning features.</remarks>
        protected const string SuggestedQuestionsData = "suggestedQuestions";

        /// <summary>
        /// The path for storing and retrieving QnA Maker context data.
        /// </summary>
        /// <remarks>This represents context about the current or previous call to QnA Maker.
        /// It is stored within the active dialog's state.
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
        protected const double DefaultThreshold = 0.3F;

        /// <summary>
        /// The default maximum number of answers to be returned for the question.
        /// </summary>
        protected const int DefaultTopN = 3;

        private const string DefaultNoAnswer = "No QnAMaker answers found.";

        // Card parameters
        private const string DefaultCardTitle = "Did you mean:";
        private const string DefaultCardNoMatchText = "None of the above.";
        private const string DefaultCardNoMatchResponse = "Thanks for the feedback.";

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerDialog"/> class.
        /// </summary>
        /// <param name="dialogId">The ID of the <see cref="Dialog"/>.</param>
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
        public QnAMakerDialog(
            string dialogId,
            string knowledgeBaseId,
            string endpointKey,
            string hostName,
            Activity noAnswer = null,
            double threshold = DefaultThreshold,
            string activeLearningCardTitle = DefaultCardTitle,
            string cardNoMatchText = DefaultCardNoMatchText,
            int top = DefaultTopN,
            Activity cardNoMatchResponse = null,
            List<Metadata> strictFilters = null,
            HttpClient httpClient = null)
            : base(dialogId)
        {
            this.KnowledgeBaseId = knowledgeBaseId ?? throw new ArgumentNullException(nameof(knowledgeBaseId));
            this.HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            this.EndpointKey = endpointKey ?? throw new ArgumentNullException(nameof(endpointKey));
            this.Threshold = threshold;
            this.Top = top;
            this.ActiveLearningCardTitle = activeLearningCardTitle;
            this.CardNoMatchText = cardNoMatchText;
            this.StrictFilters = strictFilters;
            this.NoAnswer = noAnswer ?? MessageFactory.Text(DefaultNoAnswer);
            this.CardNoMatchResponse = cardNoMatchResponse ?? MessageFactory.Text(DefaultCardNoMatchResponse);
            this.HttpClient = httpClient;

            // add waterfall steps
            this.AddStep(CallGenerateAnswerAsync);
            this.AddStep(CallTrainAsync);
            this.AddStep(CheckForMultiTurnPromptAsync);
            this.AddStep(DisplayQnAResultAsync);
        }

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
        public QnAMakerDialog(
            string knowledgeBaseId,
            string endpointKey,
            string hostName,
            Activity noAnswer = null,
            double threshold = DefaultThreshold,
            string activeLearningCardTitle = DefaultCardTitle,
            string cardNoMatchText = DefaultCardNoMatchText,
            int top = DefaultTopN,
            Activity cardNoMatchResponse = null,
            List<Metadata> strictFilters = null,
            HttpClient httpClient = null)
            : this(
                nameof(QnAMakerDialog),
                knowledgeBaseId,
                endpointKey,
                hostName,
                noAnswer,
                threshold,
                activeLearningCardTitle,
                cardNoMatchText,
                top,
                cardNoMatchResponse,
                strictFilters,
                httpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerDialog"/> class.
        /// The JSON serializer uses this constructor to deserialize objects of this class.
        /// </summary>
        public QnAMakerDialog()
            : base(nameof(QnAMakerDialog))
        {
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
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker knowledge base ID to query.
        /// </summary>
        /// <value>
        /// The knowledge base ID or an expression which evaluates to the knowledge base ID.
        /// </value>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker host URL for the knowledge base.
        /// </summary>
        /// <value>
        /// The QnA Maker host URL or an expression which evaluates to the host URL.
        /// </value>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker endpoint key to use to query the knowledge base.
        /// </summary>
        /// <value>
        /// The QnA Maker endpoint key to use or an expression which evaluates to the endpoint key.
        /// </value>
        public string EndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the threshold for answers returned, based on score.
        /// </summary>
        /// <value>
        /// The threshold for answers returned or an expression which evaluates to the threshold.
        /// </value>
        public double Threshold { get; set; } = DefaultThreshold;

        /// <summary>
        /// Gets or sets the maximum number of answers to return from the knowledge base.
        /// </summary>
        /// <value>
        /// The maximum number of answers to return from the knowledge base or an expression which
        /// evaluates to the maximum number to return.
        /// </value>
        public int Top { get; set; } = DefaultTopN;

        /// <summary>
        /// Gets or sets the template to send the user when QnA Maker does not find an answer.
        /// </summary>
        /// <value>
        /// The template to send the user when QnA Maker does not find an answer.
        /// </value>
        public Activity NoAnswer { get; set; } = MessageFactory.Text(DefaultNoAnswer);

        /// <summary>
        /// Gets or sets the card title to use when showing active learning options to the user,
        /// if active learning is enabled.
        /// </summary>
        /// <value>
        /// The path card title to use when showing active learning options to the user or an
        /// expression which evaluates to the card title.
        /// </value>
        public string ActiveLearningCardTitle { get; set; }

        /// <summary>
        /// Gets or sets the button text to use with active learning options, allowing a user to
        /// indicate none of the options are applicable.
        /// </summary>
        /// <value>
        /// The button text to use with active learning options or an expression which evaluates to
        /// the button text.
        /// </value>
        public string CardNoMatchText { get; set; }

        /// <summary>
        /// Gets or sets the template to send the user if they select the no match option on an
        /// active learning card.
        /// </summary>
        /// <value>
        /// The template to send the user if they select the no match option on an active learning card.
        /// </value>
        public Activity CardNoMatchResponse { get; set; } = MessageFactory.Text(DefaultCardNoMatchResponse);

        /// <summary>
        /// Gets the QnA Maker metadata with which to filter or boost queries to the knowledge base;
        /// or null to apply none.
        /// </summary>
        /// <value>
        /// The QnA Maker metadata with which to filter or boost queries to the knowledge base
        /// or an expression which evaluates to the QnA Maker metadata.
        /// </value>
        public List<Metadata> StrictFilters { get; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the flag to determine if personal information should be logged in telemetry.
        /// </summary>
        /// <value>
        /// The flag to indicate in personal information should be logged in telemetry.
        /// </value>
        public bool LogPersonalInformation { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets environment of knowledgebase to be called. 
        /// </summary>
        /// <value>
        /// A value indicating whether to call test or prod environment of knowledge base. 
        /// </value>
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets the QnA Maker ranker type to use.
        /// </summary>
        /// <value>
        /// The QnA Maker ranker type to use or an expression which evaluates to the ranker type.
        /// </value>
        /// <seealso cref="RankerTypes"/>
        public string RankerType { get; set; } = RankerTypes.DefaultRankerType;

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

        /// <inheritdoc/>
        protected override async Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                // decide whether we want to allow interruption or not.
                // if we don't get a response from QnA which signifies we expected it,
                // then we allow interruption.

                var reply = dc.Context.Activity.Text;
                var dialogOptions = ObjectPath.GetPathValue<QnAMakerDialogOptions>(dc.ActiveDialog.State, Options);

                if (reply.Equals(dialogOptions.ResponseOptions.CardNoMatchText, StringComparison.OrdinalIgnoreCase))
                {
                    // it matches nomatch text, we like that.
                    return true;
                }

                if (dc.ActiveDialog.State.TryGetValue(SuggestedQuestionsData, out object value))
                {
                    var suggestedQuestions = value as List<string>; 
                    if (suggestedQuestions != null && suggestedQuestions.Any(question => string.Compare(question, reply.Trim(), StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        // it matches one of the suggested actions, we like that.
                        return true;
                    }
                }

                // Calling QnAMaker to get response.
                var qnaClient = await GetQnAMakerClientAsync(dc).ConfigureAwait(false);
                ResetOptions(dc, dialogOptions);

                var response = await qnaClient.GetAnswersRawAsync(dc.Context, dialogOptions.QnAMakerOptions).ConfigureAwait(false);

                // cache result so step doesn't have to do it again, this is a turn cache and we use hashcode so we don't conflict with any other qnamakerdialogs out there.
                dc.ActiveDialog.State[$"qnaresult{this.GetHashCode()}"] = response;

                // disable interruption if we have answers.
                return response.Answers.Any();
            }

            // call base for default behavior.
            return await OnPostBubbleEventAsync(dc, e, cancellationToken).ConfigureAwait(false);
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

            var httpClient = dc.Context.TurnState.Get<HttpClient>();
            if (httpClient == null)
            {
                httpClient = HttpClient;
            }

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = this.EndpointKey,
                Host = this.HostName,
                KnowledgeBaseId = this.KnowledgeBaseId
            };
            var options = await GetQnAMakerOptionsAsync(dc).ConfigureAwait(false);
            return new QnAMaker(endpoint, options, httpClient, this.TelemetryClient, this.LogPersonalInformation);
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
                ScoreThreshold = Threshold,
                StrictFilters = StrictFilters?.ToArray(),
                Top = Top,
                Context = new QnARequestContext(),
                QnAId = 0,
                RankerType = RankerType,
                IsTest = IsTest
            });
        }

        /// <summary>
        /// Gets the options the dialog will use to display query results to the user.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the response options to use.</remarks>
        protected virtual Task<QnADialogResponseOptions> GetQnAResponseOptionsAsync(DialogContext dc)
        {
            return Task.FromResult(new QnADialogResponseOptions
            {
                NoAnswer = this.NoAnswer,
                ActiveLearningCardTitle = this.ActiveLearningCardTitle ?? DefaultCardTitle,
                CardNoMatchText = this.CardNoMatchText ?? DefaultCardNoMatchText,
                CardNoMatchResponse = this.CardNoMatchResponse
            });
        }

        /// <summary>
        /// Displays QnA Result from stepContext through Activity - with first answer from QnA Maker response.
        /// </summary>
        /// <param name="stepContext">stepContext.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>An object of Task of type <see cref="DialogTurnResult"></see>.</returns>
        protected virtual async Task<DialogTurnResult> DisplayQnAResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

        private static void ResetOptions(DialogContext dc, QnAMakerDialogOptions dialogOptions)
        {
            // Resetting context and QnAId
            dialogOptions.QnAMakerOptions.QnAId = 0;
            dialogOptions.QnAMakerOptions.Context = new QnARequestContext();

            // -Check if previous context is present, if yes then put it with the query
            // -Check for id if query is present in reverse index.
            var previousContextData = ObjectPath.GetPathValue<Dictionary<string, int>>(dc.ActiveDialog.State, QnAContextData, new Dictionary<string, int>());
            var previousQnAId = ObjectPath.GetPathValue<int>(dc.ActiveDialog.State, PreviousQnAId, 0);

            if (previousQnAId > 0)
            {
                dialogOptions.QnAMakerOptions.Context = new QnARequestContext
                {
                    PreviousQnAId = previousQnAId
                };

                if (previousContextData.TryGetValue(dc.Context.Activity.Text, out var currentQnAId))
                {
                    dialogOptions.QnAMakerOptions.QnAId = currentQnAId;
                }
            }
        }

        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // clear suggestedQuestions between turns.
            stepContext.ActiveDialog.State.Remove(SuggestedQuestionsData);

            var dialogOptions = ObjectPath.GetPathValue<QnAMakerDialogOptions>(stepContext.ActiveDialog.State, Options);
            ResetOptions(stepContext, dialogOptions);

            // Storing the context info
            stepContext.Values[ValueProperty.CurrentQuery] = stepContext.Context.Activity.Text;

            // Calling QnAMaker to get response.
            QueryResults response;
            var qnaClient = await GetQnAMakerClientAsync(stepContext).ConfigureAwait(false);
            if (stepContext.ActiveDialog.State.TryGetValue($"qnaresult{GetHashCode()}", out var value))
            {
                response = (QueryResults)value;
            }
            else
            {
                response = await qnaClient.GetAnswersRawAsync(stepContext.Context, dialogOptions.QnAMakerOptions).ConfigureAwait(false);
            }

            // Resetting previous query.
            var previousQnAId = -1;
            ObjectPath.SetPathValue(stepContext.ActiveDialog.State, PreviousQnAId, previousQnAId);

            // Take this value from GetAnswerResponse 
            var isActiveLearningEnabled = response.ActiveLearningEnabled;

            stepContext.Values[ValueProperty.QnAData] = new List<QueryResult>(response.Answers);

            // Check if active learning is enabled.
            // MaximumScoreForLowScoreVariation is the score above which no need to check for feedback.
            if (response.Answers.Any() && response.Answers.First().Score <= (ActiveLearningUtils.MaximumScoreForLowScoreVariation / 100))
            {
                // Get filtered list of the response that support low score variation criteria.
                response.Answers = qnaClient.GetLowScoreVariation(response.Answers);

                if (response.Answers.Length > 1 && isActiveLearningEnabled)
                {
                    var suggestedQuestions = new List<string>();
                    foreach (var qna in response.Answers)
                    {
                        suggestedQuestions.Add(qna.Questions[0]);
                    }

                    // Get active learning suggestion card activity.
                    var message = QnACardBuilder.GetSuggestionsCard(suggestedQuestions, dialogOptions.ResponseOptions.ActiveLearningCardTitle, dialogOptions.ResponseOptions.CardNoMatchText);
                    await stepContext.Context.SendActivityAsync(message).ConfigureAwait(false);

                    stepContext.ActiveDialog.State[Options] = dialogOptions;
                    stepContext.ActiveDialog.State[SuggestedQuestionsData] = suggestedQuestions;
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

                    var records = new Collection<FeedbackRecord>
                    {
                        new FeedbackRecord
                        {
                            UserId = stepContext.Context.Activity.Id,
                            UserQuestion = currentQuery,
                            QnaId = qnaResult.Id,
                        }
                    };

                    var feedbackRecords = new FeedbackRecords(records);

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

                if (answer.Context != null && answer.Context.Prompts.Length > 0)
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

        internal class ValueProperty
        {
            internal const string CurrentQuery = "currentQuery";
            internal const string QnAData = "qnaData";
        }
    }
}
