using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Events;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnAMaker dialog which uses QnAMaker to get an answer.
    /// </summary>
    /// <remarks>
    /// The answer is treated as a inline LG expression, so it can be completely customized by LG system.
    /// </remarks>
    public class QnAMakerAction : Dialog
    {
        private QnAMaker qnamaker;
        private readonly HttpClient httpClient;

        public QnAMakerAction(string kbId, string hostName, string endpointKey, HttpClient httpClient = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            this.KbId = kbId;
            this.HostName = hostName;
            this.EndpointKey = endpointKey;
            this.httpClient = httpClient;
        }

        [JsonConstructor]
        public QnAMakerAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        [JsonProperty("kbId")]
        public string KbId { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("endpointKey")]
        public string EndpointKey { get; set; }

        [JsonProperty("threshold")]
        public float Threshold { get; set; }

        [JsonProperty("noAnswer")]
        public string NoAnswer { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var endpoint = new QnAMakerEndpoint
            {
                EndpointKey = this.EndpointKey,
                Host = this.HostName,
                KnowledgeBaseId = this.KbId
            };

            var qnamakerOptions = new QnAMakerOptions
            {
                ScoreThreshold = this.Threshold
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

            return await ExecuteAdaptiveQnAMakerDialog(dc, qnamaker, qnamakerOptions, cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> ExecuteQnAMakerDialog(DialogContext dc, QnAMaker qnaMaker, QnAMakerOptions qnamakerOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var questionResults = await qnamaker.GetAnswersAsync(dc.Context, qnamakerOptions).ConfigureAwait(false);

            // TODO: Get active learning flag: Need to update when this support is added to SDK
            var isActiveLearningEnabled = true;

            var queryResults = questionResults;

            var noAnswerMsg = this.NoAnswer ?? "No QnAMaker answers found.";

            if (queryResults == null || queryResults.Length == 0)
            {
                await dc.Context.SendActivityAsync(noAnswerMsg, cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(false, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var filteredQueryResults = queryResults;
                if (isActiveLearningEnabled)
                {
                    filteredQueryResults = qnamaker.GetLowScoreVariation(queryResults);
                }

                if (filteredQueryResults.Length > 1)
                {
                    await dc.Context.SendActivityAsync("Multiple answers triggred", cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else if (filteredQueryResults.Length > 0)
                {
                    await dc.Context.SendActivityAsync(filteredQueryResults[0].Answer, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await dc.Context.SendActivityAsync(noAnswerMsg, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            return await dc.EndDialogAsync(false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task<DialogTurnResult> ExecuteAdaptiveQnAMakerDialog(DialogContext dc, QnAMaker qnaMaker, QnAMakerOptions qnamakerOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialog = new DialogHelper(qnaMaker);
            var textPrompt = new TextPrompt("TextPrompt");
            dc.Dialogs.Add(dialog.QnAMakerActiveLearningDialog);
            dc.Dialogs.Add(textPrompt);

            return await dc.BeginDialogAsync(DialogHelper.ActiveLearningDialogName, qnamakerOptions, cancellationToken).ConfigureAwait(false);
        }
    }
}
