using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class SendActivity : DialogCommand
    {
        public ITemplate<Activity> Activity { get; set; }

        public SendActivity() : base()
        {
        }

        public SendActivity(string text)
        {
            this.Activity = new ActivityTemplate(text);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = await Activity.BindToData(dc.Context, dc.State, (property, data) => dc.State.GetValue<object>(data, property)).ConfigureAwait(false);
            var response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"SendActivity({OutputBinding ?? string.Empty})";
        }
    }
}
