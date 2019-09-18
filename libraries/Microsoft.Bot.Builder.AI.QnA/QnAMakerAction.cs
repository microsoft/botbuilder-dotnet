using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAMaker action which uses QnAMaker to get an answer.
    /// </summary>
    public class QnAMakerAction : Dialog
    {
        private const float DefaultThreshold = 0.03F;
        private QnAMaker qnamaker;
        private readonly HttpClient httpClient;
        private readonly string defaultNoAnswer = "No QnAMaker answers found.";

        public QnAMakerAction(string knowledgeBaseId, string endpointKey, string hostName, string noAnswer, float threshold = DefaultThreshold, Metadata[] strictFilters = null, HttpClient httpClient = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            this.KnowledgeBaseId = knowledgeBaseId ?? throw new ArgumentNullException(nameof(knowledgeBaseId));
            this.HostName = hostName ?? throw new ArgumentNullException(nameof(HostName));
            this.EndpointKey = endpointKey ?? throw new ArgumentNullException(nameof(EndpointKey));
            this.Threshold = threshold;
            this.NoAnswer = noAnswer ?? defaultNoAnswer;
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
        public string KnowledgeBaseId { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("endpointKey")]
        public string EndpointKey { get; set; }

        [JsonProperty("threshold")]
        public float Threshold { get; set; }

        [JsonProperty("noAnswer")]
        public string NoAnswer { get; set; }

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
                return Dialog.EndOfTurn;
            }

            return await ExecuteQnAMakerDialog(dc, qnamaker, qnamakerOptions, cancellationToken).ConfigureAwait(false);
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
    }
}
