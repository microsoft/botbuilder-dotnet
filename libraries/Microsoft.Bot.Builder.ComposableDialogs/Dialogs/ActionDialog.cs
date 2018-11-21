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
        public string CallDialogId { get; set; }

        /// <summary>
        /// Settings for the dialog
        /// </summary>
        public object CallDialogOptions { get; set; } = new ExpandoObject();

        /// <summary>
        /// Action to perform when dialog is completed
        /// </summary>
        public IAction OnCompleted { get; set; }

        /// <summary>
        /// When this dialog is started we start the inner dialog
        /// </summary>
        /// <param name="dialogContext"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog.State;
            options = options ?? this.CallDialogOptions;
            state[$"{this.Id}.options"] = options;

            return await BeginInnerDialog(dialogContext, options, cancellationToken);
        }

        /// <summary>
        /// The only time we continue is when a ContinueDialogAction is executed, in which case we start the inner dialog again (since it ended already)
        /// </summary>
        /// <param name="dialogContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog.State;
            dynamic options = state[$"{this.Id}.options"];

            return await BeginInnerDialog(dialogContext, options, cancellationToken);
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


        private async Task<DialogTurnResult> BeginInnerDialog(DialogContext dialogContext, object options, CancellationToken cancellationToken)
        {
            // start the inner dialog
            var result = await dialogContext.BeginDialogAsync(this.CallDialogId, options, cancellationToken);
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
