using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.QnAMaker
{
    public class QnAMakerDialog : Microsoft.Bot.Builder.AI.QnA.QnAMakerDialog
    {
        private Expression knowledgebaseId;
        private Expression endpointkey;
        private Expression hostname;

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

        protected override AI.QnA.QnAMaker GetQnAMakerClient(DialogContext dc)
        {
            return base.GetQnAMakerClient(dc);
        }

        protected override QnAMakerOptions GetQnAMakerOptions(DialogContext dc)
        {
            return base.GetQnAMakerOptions(dc);
        }

        protected override QnADialogResponseOptions GetQnAResponseOptions(DialogContext dc)
        {
            return base.GetQnAResponseOptions(dc);
        }
    }
}
