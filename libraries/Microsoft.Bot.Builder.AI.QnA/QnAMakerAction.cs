// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAMaker dialog which uses QnAMaker to get an answer.
    /// </summary>
    public class QnAMakerAction : Dialog
    {
        private QnAMaker qnamaker;
        private readonly HttpClient httpClient;
        private Expression knowledgebaseId;
        private Expression endpointkey;

        public QnAMakerAction(
            string knowledgeBaseId, 
            string endpointKey, 
            string hostName, 
            string noAnswer = QnAMakerActionBuilder.DefaultNoAnswer, 
            float threshold = QnAMakerActionBuilder.DefaultThreshold, 
            string activeLearningCardTitle = QnAMakerActionBuilder.DefaultCardTitle, 
            string cardNoMatchText = QnAMakerActionBuilder.DefaultCardNoMatchText, 
            string cardNoMatchResponse = QnAMakerActionBuilder.DefaultCardNoMatchResponse, 
            Metadata[] strictFilters = null,  
            HttpClient httpClient = null, 
            [CallerFilePath] string sourceFilePath = "", 
            [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            this.KnowledgeBaseId = knowledgeBaseId ?? throw new ArgumentNullException(nameof(knowledgeBaseId));
            this.HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            this.EndpointKey = endpointKey ?? throw new ArgumentNullException(nameof(endpointKey));
            this.Threshold = threshold;
            this.NoAnswer = noAnswer;
            this.ActiveLearningCardTitle = activeLearningCardTitle;
            this.CardNoMatchText = cardNoMatchText;
            this.CardNoMatchResponse = cardNoMatchResponse;
            this.StrictFilters = strictFilters;
            this.httpClient = httpClient;
        }

        [JsonConstructor]
        public QnAMakerAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId
        {
            get { return knowledgebaseId?.ToString(); }
            set { knowledgebaseId = value != null ? new ExpressionEngine().Parse(value) : null;  }
        }

        [JsonProperty("hostname")]
        public string HostName
        { get; set; }

        [JsonProperty("endpointKey")]
        public string EndpointKey
        {
            get { return endpointkey?.ToString(); }
            set { endpointkey = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("threshold")]
        public float Threshold { get; set; }

        [JsonProperty("noAnswer")]
        public string NoAnswer { get; set; }

        [JsonProperty("activeLearningCardTitle")]
        public string ActiveLearningCardTitle { get; set; }

        [JsonProperty("cardNoMatchText")]
        public string CardNoMatchText { get; set; }

        [JsonProperty("cardNoMatchResponse")]
        public string CardNoMatchResponse { get; set; }

        [JsonProperty("strictFilters")]
        public Metadata[] StrictFilters { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = this.EndpointKey,
                Host = this.HostName,
                KnowledgeBaseId = this.KnowledgeBaseId
            };

            var qnamakerOptions = new QnAMakerOptions
            {
                ScoreThreshold = this.Threshold,
                StrictFilters = this.StrictFilters
            };

            if (qnamaker == null)
            {
                qnamaker = new QnAMaker(endpoint, qnamakerOptions, httpClient);
            }

            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (dc.Context?.Activity?.Type != ActivityTypes.Message)
            {
                return EndOfTurn;
            }

            return await ExecuteAdaptiveQnAMakerDialog(dc, qnamaker, qnamakerOptions, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> ExecuteQnAMakerDialog(DialogContext dc, QnAMaker qnaMaker, QnAMakerOptions qnamakerOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var questionResults = await qnamaker.GetAnswersAsync(dc.Context, qnamakerOptions).ConfigureAwait(false);

            if (questionResults == null || questionResults.Length == 0)
            {
                await dc.Context.SendActivityAsync(this.NoAnswer, cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(false, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (questionResults.Length > 0)
                {
                    await dc.Context.SendActivityAsync(questionResults[0].Answer, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await dc.Context.SendActivityAsync(this.NoAnswer, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            return await dc.EndDialogAsync(false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> ExecuteAdaptiveQnAMakerDialog(DialogContext dc, QnAMaker qnaMaker, QnAMakerOptions qnamakerOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialog = new QnAMakerActionBuilder(qnaMaker).BuildDialog(dc);

            // Set values for active dialog.
            qnamakerOptions.NoAnswer = NoAnswer;
            qnamakerOptions.ActiveLearningCardTitle = ActiveLearningCardTitle;
            qnamakerOptions.CardNoMatchText = CardNoMatchText;
            qnamakerOptions.CardNoMatchResponse = CardNoMatchResponse;

            var dialogOptions = new Dictionary<string, object>
            {
                [QnAMakerActionBuilder.QnAOptions] = qnamakerOptions
            };

            return await dc.BeginDialogAsync(QnAMakerActionBuilder.QnAMakerDialogName, dialogOptions, cancellationToken).ConfigureAwait(false);
        }
    }
}
