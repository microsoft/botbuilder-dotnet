using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.QnA
{
    public class QnAMakerDialog2 : QnAMakerDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.QnAMakerDialog";

        [JsonConstructor]
        public QnAMakerDialog2([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
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
        /// Gets or sets the Threshold score to filter results.
        /// </summary>
        /// <value>
        /// The threshold for the results.
        /// </value>
        [JsonProperty("threshold")]
        public NumberExpression Threshold { get; set; } = DefaultThreshold;

        /// <summary>
        /// Gets or sets the number of results you want.
        /// </summary>
        /// <value>
        /// The number of results you want.
        /// </value>
        [JsonProperty("top")]
        public IntExpression Top { get; set; } = DefaultTopN;

        /// <summary>
        /// Gets or sets the template for Default answer to return when none found in KB.
        /// </summary>
        /// <value>
        /// The template for the answer when there are no results.
        /// </value>
        [JsonProperty("noAnswer")]
        public ITemplate<Activity> NoAnswer { get; set; }

        /// <summary>
        /// Gets or sets the Title for active learning suggestions card.
        /// </summary>
        /// <value>
        /// Title for active learning suggestions card.
        /// </value>
        [JsonProperty("activeLearningCardTitle")]
        public StringExpression ActiveLearningCardTitle { get; set; }

        /// <summary>
        /// Gets or sets the Text for no match option.
        /// </summary>
        /// <value>
        /// The Text for no match option.
        /// </value>
        [JsonProperty("cardNoMatchText")]
        public StringExpression CardNoMatchText { get; set; }

        /// <summary>
        /// Gets or sets the template for Custom response when no match option was selected.
        /// </summary>
        /// <value>
        /// The template for Custom response when no match option was selected.
        /// </value>
        [JsonProperty("cardNoMatchResponse")]
        public ITemplate<Activity> CardNoMatchResponse { get; set; }

        /// <summary>
        /// Gets or sets the Metadata filters to use when calling the QnA Maker KB.
        /// </summary>
        /// <value>
        /// The metadata strict filters.
        /// </value>
        [JsonProperty("strictFilters")]
        public ArrayExpression<Metadata> StrictFilters { get; set; }

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
        public StringExpression RankerType { get; set; } = new StringExpression(RankerTypes.DefaultRankerType);

        protected async override Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
        {
            var qnaClient = dc.Context.TurnState.Get<IQnAMakerClient>();
            if (qnaClient != null)
            {
                // return mock client
                return qnaClient;
            }

            var (epKey, error) = this.EndpointKey.TryGetValue(dc.State);
            var (hn, error2) = this.HostName.TryGetValue(dc.State);
            var (kbId, error3) = this.KnowledgeBaseId.TryGetValue(dc.State);

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = (string)epKey,
                Host = (string)hn,
                KnowledgeBaseId = (string)kbId
            };
            var options = await GetQnAMakerOptionsAsync(dc).ConfigureAwait(false);
            return new QnAMaker(endpoint, options, this.HttpClient);
        }

        protected override Task<QnAMakerOptions> GetQnAMakerOptionsAsync(DialogContext dc)
        {
            return Task.FromResult(new QnAMakerOptions
            {
                ScoreThreshold = this.Threshold.GetValue(dc.State),
                StrictFilters = this.StrictFilters?.GetValue(dc.State)?.ToArray(),
                Top = this.Top.GetValue(dc.State),
                QnAId = 0,
                RankerType = this.RankerType.GetValue(dc.State),
                IsTest = this.IsTest
            });
        }

        protected async override Task<QnADialogResponseOptions> GetQnAResponseOptionsAsync(DialogContext dc)
        {
            var noAnswer = (this.NoAnswer != null) ? await this.NoAnswer.BindToData(dc.Context, dc.State).ConfigureAwait(false) : null;
            var cardNoMatchResponse = (this.CardNoMatchResponse != null) ? await this.CardNoMatchResponse.BindToData(dc.Context, dc.State).ConfigureAwait(false) : null;

            if (noAnswer != null)
            {
                var properties = new Dictionary<string, string>()
                {
                    { "template", JsonConvert.SerializeObject(this.NoAnswer) },
                    { "result", noAnswer == null ? string.Empty : JsonConvert.SerializeObject(noAnswer, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) },
                };
                TelemetryClient.TrackEvent("GeneratorResult", properties);
            }

            if (cardNoMatchResponse != null)
            {
                var properties = new Dictionary<string, string>()
                {
                    { "template", JsonConvert.SerializeObject(this.CardNoMatchResponse) },
                    { "result", cardNoMatchResponse == null ? string.Empty : JsonConvert.SerializeObject(cardNoMatchResponse, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) },
                };
                TelemetryClient.TrackEvent("GeneratorResult", properties);
            }

            var responseOptions = new QnADialogResponseOptions
            {
                ActiveLearningCardTitle = this.ActiveLearningCardTitle.GetValue(dc.State),
                CardNoMatchText = this.CardNoMatchText.GetValue(dc.State),
                NoAnswer = noAnswer,
                CardNoMatchResponse = cardNoMatchResponse,
            };

            return responseOptions;
        }
    }
}
