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
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAMaker dialog which uses QnAMaker to get an answer.
    /// </summary>
    public class QnAMakerDialog : Dialog
    {
        private QnAMaker qnamaker;
        private readonly HttpClient httpClient;
        private Expression knowledgebaseId;
        private Expression endpointkey;
        private Expression hostname;

        public QnAMakerDialog(
            string knowledgeBaseId,
            string endpointKey,
            string hostName,
            Activity noAnswer = null,
            float threshold = QnAMakerActionBuilder.DefaultThreshold,
            string activeLearningCardTitle = QnAMakerActionBuilder.DefaultCardTitle,
            string cardNoMatchText = QnAMakerActionBuilder.DefaultCardNoMatchText,
            Activity cardNoMatchResponse = null,
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
            this.ActiveLearningCardTitle = activeLearningCardTitle;
            this.CardNoMatchText = cardNoMatchText;
            this.StrictFilters = strictFilters;
            this.httpClient = httpClient;
            this.NoAnswer = new StaticActivityTemplate(noAnswer);
            this.CardNoMatchResponse = new StaticActivityTemplate(cardNoMatchResponse);
        }

        [JsonConstructor]
        public QnAMakerDialog([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId
        {
            get { return knowledgebaseId?.ToString(); }
            set { knowledgebaseId = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("hostname")]
        public string HostName
        {
            get { return hostname?.ToString(); }
            set { hostname = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("endpointKey")]
        public string EndpointKey
        {
            get { return endpointkey?.ToString(); }
            set { endpointkey = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("threshold")]
        public float Threshold { get; set; }

        [JsonProperty("noAnswer")]
        public ITemplate<Activity> NoAnswer { get; set; }

        [JsonProperty("activeLearningCardTitle")]
        public string ActiveLearningCardTitle { get; set; }

        [JsonProperty("cardNoMatchText")]
        public string CardNoMatchText { get; set; }

        [JsonProperty("cardNoMatchResponse")]
        public ITemplate<Activity> CardNoMatchResponse { get; set; }

        [JsonProperty("strictFilters")]
        public Metadata[] StrictFilters { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = endpointkey.TryEvaluate(dc.State).error == null ? endpointkey.TryEvaluate(dc.State).value.ToString() : this.EndpointKey,
                Host = endpointkey.TryEvaluate(dc.State).error == null ? hostname.TryEvaluate(dc.State).value.ToString() : this.HostName,
                KnowledgeBaseId = endpointkey.TryEvaluate(dc.State).error == null ? knowledgebaseId.TryEvaluate(dc.State).value.ToString() : this.KnowledgeBaseId
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

            if (dc.Context?.Activity?.Type != ActivityTypes.Message)
            {
                return EndOfTurn;
            }

            return await ExecuteAdaptiveQnAMakerDialog(dc, qnamaker, qnamakerOptions, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> ExecuteAdaptiveQnAMakerDialog(DialogContext dc, QnAMaker qnaMaker, QnAMakerOptions qnamakerOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialog = new QnAMakerActionBuilder(qnaMaker).BuildDialog(dc);

            // Set values for active dialog.
            var qnaDialogResponseOptions = new QnADialogResponseOptions
            {
                NoAnswer = NoAnswer,
                ActiveLearningCardTitle = ActiveLearningCardTitle,
                CardNoMatchText = CardNoMatchText,
                CardNoMatchResponse = CardNoMatchResponse
            };

            var dialogOptions = new Dictionary<string, object>
            {
                [QnAMakerActionBuilder.QnAOptions] = qnamakerOptions,
                [QnAMakerActionBuilder.QnADialogResponseOptions] = qnaDialogResponseOptions
            };

            return await dc.BeginDialogAsync(QnAMakerActionBuilder.QnAMakerDialogName, dialogOptions, cancellationToken).ConfigureAwait(false);
        }
    }
}
