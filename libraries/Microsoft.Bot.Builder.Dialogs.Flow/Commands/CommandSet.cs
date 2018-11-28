using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Execute set of commands in sequence
    /// </summary>
    /// <remarks>
    /// Commands will be processed as long as there DialogTurnResult.Status == Complete
    /// </remarks>
    public class CommandSet : IDialogCommand
    {
        public CommandSet(List<IDialogCommand> actions = null)
        {
            if (actions != null)
                this.Commands = actions;
        }

        public IList<IDialogCommand> Commands { get; set; } = new List<IDialogCommand>();

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;

            // While we are in completed state process the commandSet.  
            foreach (var action in Commands)
            {
                state["DialogTurnResult"] = result;
                switch (result.Status)
                {
                    case DialogTurnStatus.Complete:
                        // We are in a completed state, execute the next command
                        result = await action.Execute(dialogContext, options, result, cancellationToken);
                        break;

                    case DialogTurnStatus.Waiting:
                        // a new dialog was placed on the stack and we are waiting on it
                        return result;

                    case DialogTurnStatus.Cancelled:
                        // don't process commands on canceled. IS THIS RIGHT?
                        return result;

                    case DialogTurnStatus.Empty:
                        // Dialog stack is empty, but we are still operating...what the?
                        throw new System.Exception("The dialog stack is empty but there should still be a dialog on the stack! Somebody called EndDialog multiple times for the current turn.");
                }
            }
            return result;
        }
    }
}
