// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.QnA;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers
{
    /// <summary>
    /// IRecognizer implementation which uses QnAMaker KB to identify intents.
    /// </summary>
    public class QnAMakerRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.QnAMakerRecognizer";

        public const string QnAMatchIntent = "QnAMatch";
        
        private const string IntentPrefix = "intent=";

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
        public StringExpression KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets the Hostname for your QnA Maker service.
        /// </summary>
        /// <value>
        /// The host name of the QnA Maker knowledgebase.
        /// </value>
        [JsonProperty("hostname")]
        public StringExpression HostName { get; set; } 

        /// <summary>
        /// Gets or sets the Endpoint key for the QnA Maker KB.
        /// </summary>
        /// <value>
        /// The endpoint key for the QnA service.
        /// </value>
        [JsonProperty("endpointKey")]
        public StringExpression EndpointKey { get; set; } 

        /// <summary>
        /// Gets or sets the number of results you want.
        /// </summary>
        /// <value>
        /// The number of results you want.
        /// </value>
        [DefaultValue(3)]
        [JsonProperty("top")]
        public IntExpression Top { get; set; } = 3;

        /// <summary>
        /// Gets or sets the threshold score to filter results.
        /// </summary>
        /// <value>
        /// The threshold for the results.
        /// </value>
        [DefaultValue(0.3F)]
        [JsonProperty("threshold")]
        public NumberExpression Threshold { get; set; } = 0.3F;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets environment of knowledgebase to be called. 
        /// </summary>
        /// <value>
        /// A value indicating whether to call test or prod environment of knowledgebase. 
        /// </value>
        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets ranker Type.
        /// </summary>
        /// <value>
        /// The desired RankerType.
        /// </value>
        [JsonProperty("rankerType")]
        public StringExpression RankerType { get; set; } = RankerTypes.DefaultRankerType;

        [JsonIgnore]
        public HttpClient HttpClient { get; set; }

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, string text, string locale, CancellationToken cancellationToken)
        {
            var dcState = dialogContext.GetState();

            // Identify matched intents
            var utterance = text ?? string.Empty;

            var recognizerResult = new RecognizerResult()
            {
                Text = utterance,
                Intents = new Dictionary<string, IntentScore>(),
            };

            List<Metadata> filters = new List<Metadata>()
            {
                new Metadata() { Name = "dialogName", Value = dialogContext.ActiveDialog.Id }
            };

            // if there is $qna.metadata set add to filters
            var externalMetadata = dcState.GetValue<Metadata[]>("$qna.metadata");
            if (externalMetadata != null)
            {
                filters.AddRange(externalMetadata);
            }

            // Calling QnAMaker to get response.
            var qnaClient = await GetQnAMakerClientAsync(dialogContext).ConfigureAwait(false);
            var answers = await qnaClient.GetAnswersAsync(
                dialogContext.Context,
                new QnAMakerOptions
                {
                    Context = dcState.GetValue<QnARequestContext>("$qna.context"),
                    ScoreThreshold = this.Threshold.TryGetValue(dcState).Value,
                    StrictFilters = filters.ToArray(),
                    Top = this.Top.TryGetValue(dcState).Value,
                    QnAId = 0,
                    RankerType = this.RankerType.TryGetValue(dcState).Value,
                    IsTest = this.IsTest
                },
                null).ConfigureAwait(false);

            if (answers.Any())
            {
                QueryResult topAnswer = null;
                foreach (var answer in answers)
                {
                    if ((topAnswer == null) || (answer.Score > topAnswer.Score))
                    {
                        topAnswer = answer;
                    }
                }

                if (topAnswer.Answer.Trim().ToLower().StartsWith(IntentPrefix))
                {
                    recognizerResult.Intents.Add(topAnswer.Answer.Trim().Substring(IntentPrefix.Length).Trim(), new IntentScore() { Score = topAnswer.Score });
                }
                else
                {
                    recognizerResult.Intents.Add(QnAMatchIntent, new IntentScore() { Score = topAnswer.Score });
                }

                var answerArray = new JArray();
                answerArray.Add(topAnswer.Answer);
                ObjectPath.SetPathValue(recognizerResult, "entities.answer", answerArray);

                var instance = new JArray();
                instance.Add(JObject.FromObject(topAnswer));
                ObjectPath.SetPathValue(recognizerResult, "entities.$instance.answer", instance);

                recognizerResult.Properties["answers"] = answers;
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

            var dcState = dc.GetState();

            var (epKey, error) = this.EndpointKey.TryGetValue(dcState);
            var (hn, error2) = this.HostName.TryGetValue(dcState);
            var (kbId, error3) = this.KnowledgeBaseId.TryGetValue(dcState);

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = (string)epKey ?? throw new ArgumentNullException(nameof(EndpointKey), error),
                Host = (string)hn ?? throw new ArgumentNullException(nameof(HostName), error2),
                KnowledgeBaseId = (string)kbId ?? throw new ArgumentNullException(nameof(KnowledgeBaseId), error3)
            };

            return Task.FromResult<IQnAMakerClient>(new QnAMaker(endpoint, httpClient: this.HttpClient));
        }
    }
}
