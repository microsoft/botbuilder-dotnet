using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.QnA
{
    public class QnAMakerDialog2 : QnAMakerDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.QnAMakerDialog";

        private Expression knowledgebaseIdExpression;
        private Expression endpointkeyExpression;
        private Expression hostnameExpression;

        [JsonConstructor]
        public QnAMakerDialog2([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {
        }

        [JsonProperty("knowledgeBaseId")]
        public string KnowledgeBaseId
        {
            get { return knowledgebaseIdExpression?.ToString(); }
            set { knowledgebaseIdExpression = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("hostname")]
        public string HostName
        {
            get { return hostnameExpression?.ToString(); }
            set { hostnameExpression = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("endpointKey")]
        public string EndpointKey
        {
            get { return endpointkeyExpression?.ToString(); }
            set { endpointkeyExpression = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("threshold")]
        public float Threshold { get; set; } = DefaultThreshold;

        [JsonProperty("top")]
        public int Top { get; set; } = DefaultTopN;
        
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

        protected async override Task<IQnAMakerClient> GetQnAMakerClientAsync(DialogContext dc)
        {
            var qnaClient = dc.Context.TurnState.Get<IQnAMakerClient>();
            if (qnaClient != null)
            {
                // return mock client
                return qnaClient;
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
            var options = await GetQnAMakerOptionsAsync(dc).ConfigureAwait(false);
            return new QnAMaker(endpoint, options, this.HttpClient);
        }

        protected override Task<QnAMakerOptions> GetQnAMakerOptionsAsync(DialogContext dc)
        {
            return Task.FromResult(new QnAMakerOptions
            {
                ScoreThreshold = this.Threshold,
                StrictFilters = this.StrictFilters,
                Top = this.Top
            });
        }

        protected async override Task<QnADialogResponseOptions> GetQnAResponseOptionsAsync(DialogContext dc)
        {
            var noAnswer = (this.NoAnswer != null) ? await this.NoAnswer.BindToData(dc.Context, dc.GetState()).ConfigureAwait(false) : null;
            var cardNoMatchResponse = (this.CardNoMatchResponse != null) ? await this.CardNoMatchResponse.BindToData(dc.Context, dc.GetState()).ConfigureAwait(false) : null;

            var responseOptions = new QnADialogResponseOptions
            {
                ActiveLearningCardTitle = this.ActiveLearningCardTitle,
                CardNoMatchText = this.CardNoMatchText,
                NoAnswer = noAnswer,
                CardNoMatchResponse = cardNoMatchResponse,
            };

            return responseOptions;
        }
    }
}
