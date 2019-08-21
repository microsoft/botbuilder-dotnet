using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
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
    public class QnAMakerDialog : Dialog
    {
        private QnAMaker qnamaker;

        public QnAMakerDialog(string dialogId = null, QnAMaker qnamaker = null)
            : base(dialogId)
        {
            this.qnamaker = qnamaker;
        }

        [JsonProperty("endpoint")]
        public QnAMakerEndpoint Endpoint { get; set; }

        [JsonProperty("options")]
        public QnAMakerOptions Options { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (qnamaker == null)
            {
                qnamaker = new QnAMaker(Endpoint, Options);
            }

            if (dc.Context.Activity.Type == ActivityTypes.Message)
            {
                var questionResults = await qnamaker.GetAnswersAsync(dc.Context, this.Options).ConfigureAwait(false);

                var topResult = questionResults.OrderByDescending(r => r.Score).FirstOrDefault();
                if (topResult != null && topResult.Score > 0)
                {
                    var template = new ActivityTemplate(topResult.Answer);
                    var activity = await template.BindToData(dc.Context, dc.State).ConfigureAwait(false);
                    var response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                    return await dc.EndDialogAsync(true, cancellationToken).ConfigureAwait(false);
                }
            }

            return await dc.EndDialogAsync(false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
