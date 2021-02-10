// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.QnA.Recognizers
{
    /// <summary>
    /// IRecognizer implementation which uses QnAMaker KB to identify intents.
    /// </summary>
    public class QnAMakerRecognizer : Recognizer
    {
        /// <summary>
        /// The declarative type for this recognizer.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.QnAMakerRecognizer";

        /// <summary>
        /// Key used when adding the intent to the <see cref="RecognizerResult"/> intents collection.
        /// </summary>
        public const string QnAMatchIntent = "QnAMatch";

        private const string IntentPrefix = "intent=";

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerRecognizer"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets <see cref="Metadata"/> join operator.
        /// </summary>
        /// <value>
        /// A value used for Join operation of Metadata <see cref="Metadata"/>.
        /// </value>
        [JsonProperty("strictFiltersJoinOperator")]
        public JoinOperator StrictFiltersJoinOperator { get; set; }

        /// <summary>
        /// Gets or sets the whether to include the dialog name metadata for QnA context.
        /// </summary>
        /// <value>
        /// A bool or boolean expression.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("includeDialogNameInMetadata")]
        public BoolExpression IncludeDialogNameInMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets an expression to evaluate to set additional metadata name value pairs.
        /// </summary>
        /// <value>An expression to evaluate for pairs of metadata.</value>
        [JsonProperty("metadata")]
        public ArrayExpression<Metadata> Metadata { get; set; }

        /// <summary>
        /// Gets or sets an expression to evaluate to set the context.
        /// </summary>
        /// <value>An expression to evaluate to QnARequestContext to pass as context.</value>
        [JsonProperty("context")]
        public ObjectExpression<QnARequestContext> Context { get; set; }

        /// <summary>
        /// Gets or sets an expression or numberto use for the QnAId paratmer.
        /// </summary>
        /// <value>The expression or number.</value>
        [JsonProperty("qnaId")]
        public IntExpression QnAId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> to be used when calling the QnA Maker API.
        /// </summary>
        /// <value>
        /// A instance of <see cref="HttpClient"/>.
        /// </value>
        [JsonIgnore]
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the flag to determine if personal information should be logged in telemetry.
        /// </summary>
        /// <value>
        /// The flag to indicate in personal information should be logged in telemetry.
        /// </value>
        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; } = "=settings.telemetry.logPersonalInformation";

        /// <summary>
        /// Return results of the call to QnA Maker.
        /// </summary>
        /// <param name="dialogContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="activity">The incoming activity received from the user. The Text property value is used as the query text for QnA Maker.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="telemetryProperties">Additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>A <see cref="RecognizerResult"/> containing the QnA Maker result.</returns>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            // Identify matched intents
            var recognizerResult = new RecognizerResult
            {
                Text = activity.Text,
                Intents = new Dictionary<string, IntentScore>(),
            };

            if (string.IsNullOrEmpty(activity.Text))
            {
                recognizerResult.Intents.Add("None", new IntentScore());
                return recognizerResult;
            }

            var filters = new List<Metadata>();
            if (IncludeDialogNameInMetadata.GetValue(dialogContext.State))
            {
                filters.Add(new Metadata
                {
                    Name = "dialogName",
                    Value = dialogContext.ActiveDialog.Id
                });
            }

            // if there is $qna.metadata set add to filters
            var externalMetadata = Metadata?.GetValue(dialogContext.State);
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
                    Context = Context?.GetValue(dialogContext.State),
                    ScoreThreshold = Threshold.GetValue(dialogContext.State),
                    StrictFilters = filters.ToArray(),
                    Top = Top.GetValue(dialogContext.State),
                    QnAId = QnAId.GetValue(dialogContext.State),
                    RankerType = RankerType.GetValue(dialogContext.State),
                    IsTest = IsTest,
                    StrictFiltersJoinOperator = StrictFiltersJoinOperator
                },
                null).ConfigureAwait(false);

            if (answers.Any())
            {
                QueryResult topAnswer = null;
                foreach (var answer in answers)
                {
                    if (topAnswer == null || answer.Score > topAnswer.Score)
                    {
                        topAnswer = answer;
                    }
                }

                if (topAnswer.Answer.Trim().ToUpperInvariant().StartsWith(IntentPrefix.ToUpperInvariant(), StringComparison.Ordinal))
                {
                    recognizerResult.Intents.Add(topAnswer.Answer.Trim().Substring(IntentPrefix.Length).Trim(), new IntentScore { Score = topAnswer.Score });
                }
                else
                {
                    recognizerResult.Intents.Add(QnAMatchIntent, new IntentScore { Score = topAnswer.Score });
                }

                var answerArray = new JArray();
                answerArray.Add(topAnswer.Answer);
                ObjectPath.SetPathValue(recognizerResult, "entities.answer", answerArray);

                var instance = new JArray();
                var data = JObject.FromObject(topAnswer);
                data["startIndex"] = 0;
                data["endIndex"] = activity.Text.Length;
                instance.Add(data);
                ObjectPath.SetPathValue(recognizerResult, "entities.$instance.answer", instance);

                recognizerResult.Properties["answers"] = answers;
            }
            else
            {
                recognizerResult.Intents.Add("None", new IntentScore { Score = 1.0f });
            }

            TrackRecognizerResult(dialogContext, "QnAMakerRecognizerResult", FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties, dialogContext), telemetryMetrics);

            return recognizerResult;
        }

        /// <summary>
        /// Gets an instance of <see cref="IQnAMakerClient"/>.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> used to access state.</param>
        /// <returns>An instance of <see cref="IQnAMakerClient"/>.</returns>
        protected virtual Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
        {
            var qnaClient = dc.Context.TurnState.Get<IQnAMakerClient>();
            if (qnaClient != null)
            {
                // return mock client
                return Task.FromResult(qnaClient);
            }

            var httpClient = dc.Context.TurnState.Get<HttpClient>();
            if (httpClient == null)
            {
                httpClient = HttpClient;
            }

            var (epKey, error) = EndpointKey.TryGetValue(dc.State);
            var (hn, error2) = HostName.TryGetValue(dc.State);
            var (kbId, error3) = KnowledgeBaseId.TryGetValue(dc.State);
            var (logPersonalInfo, error4) = LogPersonalInformation.TryGetValue(dc.State);

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = epKey ?? throw new InvalidOperationException($"Unable to get a value for {nameof(EndpointKey)} from state. {error}"),
                Host = hn ?? throw new InvalidOperationException($"Unable to a get value for {nameof(HostName)} from state. {error2}"),
                KnowledgeBaseId = kbId ?? throw new InvalidOperationException($"Unable to get a value for {nameof(KnowledgeBaseId)} from state. {error3}")
            };

            return Task.FromResult<IQnAMakerClient>(new QnAMaker(endpoint, new QnAMakerOptions(), httpClient, TelemetryClient, logPersonalInfo));
        }

        /// <summary>
        /// Uses the RecognizerResult to create a list of properties to be included when tracking the result in telemetry.
        /// </summary>
        /// <param name="recognizerResult">Recognizer Result.</param>
        /// <param name="telemetryProperties">A list of properties to append or override the properties created using the RecognizerResult.</param>
        /// <param name="dialogContext">Dialog Context.</param>
        /// <returns>A dictionary that can be included when calling the TrackEvent method on the TelemetryClient.</returns>
        protected override Dictionary<string, string> FillRecognizerResultTelemetryProperties(RecognizerResult recognizerResult, Dictionary<string, string> telemetryProperties, DialogContext dialogContext = null)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext), "DialogContext needed for state in AdaptiveRecognizer.FillRecognizerResultTelemetryProperties method.");
            }

            var properties = new Dictionary<string, string>
            {
                { "TopIntent", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Key : null },
                { "TopIntentScore", recognizerResult.Intents.Any() ? recognizerResult.Intents.First().Value?.Score?.ToString("N1", CultureInfo.InvariantCulture) : null },
                { "Intents", recognizerResult.Intents.Any() ? JsonConvert.SerializeObject(recognizerResult.Intents) : null },
                { "Entities", recognizerResult.Entities?.ToString() },
                { "AdditionalProperties", recognizerResult.Properties.Any() ? JsonConvert.SerializeObject(recognizerResult.Properties) : null },
            };

            var (logPersonalInfo, error) = LogPersonalInformation.TryGetValue(dialogContext.State);
            
            if (logPersonalInfo && !string.IsNullOrEmpty(recognizerResult.Text))
            {
                properties.Add("Text", recognizerResult.Text);
                properties.Add("AlteredText", recognizerResult.AlteredText);
            }

            // Additional Properties can override "stock" properties.
            if (telemetryProperties != null)
            {
                return telemetryProperties.Concat(properties)
                    .GroupBy(kv => kv.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);
            }

            return properties;
        } 
    }
}
