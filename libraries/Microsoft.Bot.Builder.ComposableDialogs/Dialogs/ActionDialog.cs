using System.Dynamic;
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
        public object DialogOptions { get; set; } = new ExpandoObject();

        /// <summary>
        /// Action to perform when dialog is completed
        /// </summary>
        public IAction OnCompleted { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog.State;
            options = options ?? this.DialogOptions;
            state[$"{this.Id}.options"] = options;

            // start the inner dialog
            var result = await dialogContext.BeginDialogAsync(this.DialogId, options, cancellationToken);
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

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog.State;
            dynamic options = state[$"{this.Id}.options"];

            // start the inner dialog
            var result = await dialogContext.BeginDialogAsync(this.DialogId, options, cancellationToken);
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



        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dialogContext, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog.State;
            var options = state[$"{this.Id}.options"];

            switch (reason)
            {
                case DialogReason.EndCalled:
                    // call the IDialogAction handler
                    if (this.OnCompleted != null)
                    {
                        return await this.OnCompleted.Execute(dialogContext, options, new DialogTurnResult(DialogTurnStatus.Complete, result), cancellationToken);
                    }
                    return await dialogContext.EndDialogAsync(result, cancellationToken);

                default:
                    break;
            }

            // no routing, return the result as our result 
            return await dialogContext.EndDialogAsync(result, cancellationToken);
        }
    }
}
