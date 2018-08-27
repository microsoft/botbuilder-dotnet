using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class InterruptableDialog : ComponentDialog
    {
        public InterruptableDialog(string dialogId)
            : base(dialogId)
        {
        }

        protected override async Task<DialogStatus> OnDialogContinueAsync(DialogContext dc)
        {
            var result = await this.OnDialogInterruptionAsync(dc).ConfigureAwait(false);

            if (result == DialogStatus.Interrupted)
            {
                // try to reprompt
                return await dc.RepromptAsync().ConfigureAwait(false);
            }
            else if (result == DialogStatus.Waiting)
            {
                return result;
            }

            return await base.OnDialogContinueAsync(dc).ConfigureAwait(false);
        }

        protected abstract Task<DialogStatus> OnDialogInterruptionAsync(DialogContext dc);
    }
}
