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
    /// <summary>
    /// An adaptive dialog that supports multi-step and adaptive-learning QnA Maker services.
    /// </summary>
    /// <remarks>An instance of this class targets a specific QnA Maker knowledge base, determined at run-time.
    /// It supports knowledge bases that include follow-up prompt and active learning features.</remarks>
    public class QnAMakerDialog2 : QnAMakerDialog
    {
        /// <summary>
        /// The declarative name for this type.
        /// </summary>
        /// <remarks>Used by the framework to serialize and deserialize an instance of this type to JSON.</remarks>
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.QnAMakerDialog";

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerDialog2"/> class.
        /// The JSON serializer uses this constructor to deserialize objects of this class.
        /// </summary>
        /// <param name="sourceFilePath">The source file path, for debugging. Defaults to the full path
        /// of the source file that contains the caller.</param>
        /// <param name="sourceLineNumber">The line number, for debugging. Defaults to the line number
        /// in the source file at which the method is called.</param>
        [JsonConstructor]
        public QnAMakerDialog2([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {
        }

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
        public ITemplate<Activity> NoAnswer { get; set; }

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
        public ITemplate<Activity> CardNoMatchResponse { get; set; }

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
        public new BoolExpression LogPersonalInformation { get; set; } = "=settings.telemetry.logPersonalInformation";

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
        /// Gets an <see cref="IQnAMakerClient"/> to use to access the QnA Maker knowledge base.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the QnA Maker client to use.</remarks>
        protected async override Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
        {
            var qnaClient = dc.Context.TurnState.Get<IQnAMakerClient>();
            if (qnaClient != null)
            {
                // return mock client
                return qnaClient;
            }

            var (epKey, _) = this.EndpointKey.TryGetValue(dc.State);
            var (hn, _) = this.HostName.TryGetValue(dc.State);
            var (kbId, _) = this.KnowledgeBaseId.TryGetValue(dc.State);
            var (logPersonalInformation, _) = this.LogPersonalInformation.TryGetValue(dc.State);

            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = epKey,
                Host = hn,
                KnowledgeBaseId = kbId
            };
            var options = await GetQnAMakerOptionsAsync(dc).ConfigureAwait(false);

            return new QnAMaker(endpoint, options, this.HttpClient, this.TelemetryClient, (bool)logPersonalInformation);
        }

        /// <summary>
        /// Gets the options for the QnA Maker client that the dialog will use to query the knowledge base.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the QnA Maker options to use.</remarks>
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

        /// <summary>
        /// Gets the options the dialog will use to display query results to the user.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result contains the response options to use.</remarks>
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
