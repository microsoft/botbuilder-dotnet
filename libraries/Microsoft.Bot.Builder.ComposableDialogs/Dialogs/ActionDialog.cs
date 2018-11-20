using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ComposableDialogs.Dialogs
{
    /// <summary>
    /// ActionDialog 
    /// </summary>
    public class ActionDialog : Dialog, IDialog
    {
        private const string PersistedOptions = "options";

        public ActionDialog(string dialogId = null) : base(dialogId)
        {
        }

        /// <summary>
        /// Dialog to call
        /// </summary>
        public string DialogId { get; set; }

        /// <summary>
        /// Settings for the dialog
        /// </summary>
        public Dictionary<string, object> DialogSettings { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Action to perform when dialog is completed
        /// </summary>
        public IAction OnCompleted { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dc.ActiveDialog.State;
            state[PersistedOptions] = options;

            // start the inner dialog
            var result = await dc.BeginDialogAsync(this.DialogId, options, cancellationToken);
            return await processResult(dc, options, result, cancellationToken);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dc.ActiveDialog.State;

            var result = await base.ContinueDialogAsync(dc, cancellationToken);

            return await processResult(dc, state[PersistedOptions], result, cancellationToken);
        }

        private async Task<DialogTurnResult> processResult(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            if (result.Status == DialogTurnStatus.Waiting)
            {
                return result;
            }

            // call the IDialogAction handler
            if (this.OnCompleted != null)
            {
                return await this.OnCompleted.Execute(dialogContext, options, result, cancellationToken);
            }

            // no routing, return the result as our result 
            return result;
        }
    }
}
