using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Replace the current Dialog with another dialog as an action
    /// </summary>
    public class CallDialog : IDialogCommand
    {
        public CallDialog() { }

        public CallDialog(string dialogId, object options = null)
        {
            this.DialogId = dialogId;
            this.Options = options;
        }

        /// <summary>
        /// The dialog Id to call
        /// </summary>
        public string DialogId { get; set; }

        /// <summary>
        /// The options for calling the dilaog
        /// </summary>
        public object Options { get; set; }

        public Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;
            state["DialogTurnResult"] = result;
            return dialogContext.ReplaceDialogAsync(DialogId, Options, cancellationToken);
        }
    }
}
