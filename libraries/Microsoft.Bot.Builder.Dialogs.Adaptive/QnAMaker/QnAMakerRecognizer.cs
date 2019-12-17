// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers
{
    /// <summary>
    /// IRecognizer implementation which uses QnAMaker KB to identify intents.
    /// </summary>
    public class QnAMakerRecognizer : InputRecognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.QnAMakerRecognizer";

        private Expression knowledgebaseIdExpression;
        private Expression endpointkeyExpression;
        private Expression hostnameExpression;

        public QnAMakerRecognizer()
        {
        }

        /// <summary>
        /// Gets or sets the KnowledgeBase Id of your QnA Maker KnowledgeBase.
        /// </summary>
        /// <value>
        /// The knowledgebase Id.
        /// </value>
        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId
        {
            get { return knowledgebaseIdExpression?.ToString(); }
            set { knowledgebaseIdExpression = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets the Hostname for your QnA Maker service.
        /// </summary>
        /// <value>
        /// The host name of the QnA Maker knowledgebase.
        /// </value>
        [JsonProperty("hostname")]
        public string HostName
        {
            get { return hostnameExpression?.ToString(); }
            set { hostnameExpression = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets the Endpoint key for the QnA Maker KB.
        /// </summary>
        /// <value>
        /// The endpoint key for the QnA service.
        /// </value>
        [JsonProperty("endpointKey")]
        public string EndpointKey
        {
            get { return endpointkeyExpression?.ToString(); }
            set { endpointkeyExpression = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets the number of results you want.
        /// </summary>
        /// <value>
        /// The number of results you want.
        /// </value>
        [DefaultValue(3)]
        [JsonProperty("top")]
        public int Top { get; set; } = 3;

        /// <summary>
        /// Gets or sets the Threshold score to filter results.
        /// </summary>
        /// <value>
        /// The threshold for the results.
        /// </value>
        [DefaultValue(0.3F)]
        [JsonProperty("threshold")]
        public float Threshold { get; set; } = 0.3F;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets environment of knowledgebase to be called. 
        /// </summary>
        /// <value>
        /// A value indicating whether to call test or prod environment of knowledgebase. 
        /// </value>
        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets ranker Types.
        /// </summary>
        /// <value>
        /// Ranker Types.
        /// </value>
        [JsonProperty("rankerType")]
        public string RankerType { get; set; } = RankerTypes.DefaultRankerType;

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, string text, string locale, CancellationToken cancellationToken)
        {
            // Identify matched intents
            var utterance = text ?? string.Empty;

            var recognizerResult = new RecognizerResult()
            {
                Text = utterance,
                Intents = new Dictionary<string, IntentScore>(),
            };

            // Calling QnAMaker to get response.
            var qnaClient = await GetQnAMakerClientAsync(dialogContext).ConfigureAwait(false);
            var answers = await qnaClient.GetAnswersAsync(
                dialogContext.Context,
                new QnAMakerOptions
                {
                    ScoreThreshold = this.Threshold,
                    MetadataBoost = new Metadata[] { new Metadata() { Name = "dialogName", Value = string.Empty } },
                    Top = this.Top,
                    QnAId = 0,
                    RankerType = this.RankerType,
                    IsTest = this.IsTest
                },
                null).ConfigureAwait(false);

            if (answers.Any())
            {
                recognizerResult.Intents.Add("QnAMatch", new IntentScore() { Score = answers.First().Score });
                var answerResults = new JArray();
                var instance = new JArray();

                foreach (var answer in answers)
                {
                    answerResults.Add(JValue.FromObject(answer.Answer));
                    instance.Add(JObject.FromObject(answer));
                }

                recognizerResult.Entities["answer"] = answerResults;
                recognizerResult.Entities["$instance"] = instance;
                recognizerResult.Properties["answer"] = answers;
            }
            else
            {
                recognizerResult.Intents.Add("None", new IntentScore() { Score = 1.0f });
            }

            return recognizerResult;
        }

        protected virtual Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
        {
            var qnaClient = dc.Context.TurnState.Get<IQnAMakerClient>();
            if (qnaClient != null)
            {
                // return mock client
                return Task.FromResult(qnaClient);
            }

            var (epKey, error) = this.endpointkeyExpression.TryEvaluate(dc.GetState());
            var (hn, error2) = this.hostnameExpression.TryEvaluate(dc.GetState());
            var (kbId, error3) = this.knowledgebaseIdExpression.TryEvaluate(dc.GetState());

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = (string)epKey,
                Host = (string)hn,
                KnowledgeBaseId = (string)kbId
            };

            return Task.FromResult<IQnAMakerClient>(new QnAMaker(endpoint));
        }
    }
}
