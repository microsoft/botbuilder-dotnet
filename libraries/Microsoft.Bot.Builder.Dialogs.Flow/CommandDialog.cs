using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// CommandDialog - Call a IDialog and then execute Command when completed
    /// </summary>
    public class CommandDialog : ComponentDialog, IDialog
    {
        public CommandDialog(string dialogId = null) : base(dialogId)
        {
        }

        /// <summary>
        /// Command to perform for the dialog
        /// </summary>
        public IDialogCommand Command { get; set; }

        /// <summary>
        /// Define the expression which gets the result of this dialog
        /// </summary>
        public IExpressionEval Result { get; set; }

        /// <summary>
        /// When this dialog is started we start the inner dialog
        /// </summary>
        /// <param name="dialogContext"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog?.State;
            if (this.Command == null)
            {
                return await EndThisDialog(dialogContext, null, state, cancellationToken);
            }

            if (this.Command.Id == null)
            {
                this.Command.Id = this.Id;
            }

            options = MergeDefaultOptions(options);
            state[$"{this.Id}.options"] = options;
            state[$"{this.Id}.CurrentCommandId"] = null;
            state[$"{this.Id}.Result"] = new JObject();
            return await OnTurnAsync(dialogContext, DialogReason.BeginCalled, null, cancellationToken);
        }

        /// <summary>
        /// The only time we continue is when a ContinueDialogAction is executed, in which case we start the inner dialog again (since it ended already)
        /// </summary>
        /// <param name="dialogContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await OnTurnAsync(dialogContext, DialogReason.ContinueCalled, null, cancellationToken);
        }

        /// <summary>
        /// Inner dialog has completed
        /// </summary>
        /// <param name="dialogContext"></param>
        /// <param name="reason"></param>
        /// <param name="result"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dialogContext, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = dialogContext.ActiveDialog.State;
            state["DialogTurnResult"] = result;
            return await OnTurnAsync(dialogContext, DialogReason.NextCalled, result, cancellationToken);
        }


        private async Task<DialogTurnResult> OnTurnAsync(DialogContext dialogContext, DialogReason reason, object result, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;

            var commandResult = await this.Command.Execute(dialogContext, cancellationToken);
            if (commandResult is DialogTurnResult dialogTurnResult)
            {
                return dialogTurnResult;
            }
            else
            {
                // hit end of command, treat this as a end of dialog
                return await EndThisDialog(dialogContext, result, state, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndThisDialog(DialogContext dialogContext, object result, System.Collections.Generic.IDictionary<string, object> state, CancellationToken cancellationToken)
        {
            object dialogResult = result;
            if (this.Result != null)
            {
                dialogResult = await this.Result.Evaluate(state);
            }
            return await dialogContext.EndDialogAsync(dialogResult, cancellationToken).ConfigureAwait(false);
        }

        private object MergeDefaultOptions(object options)
        {
            dynamic opts;
            if (options != null && DefaultOptions != null)
            {
                opts = JObject.FromObject(options);
                opts.Merge(this.DefaultOptions);
            }
            else
            {
                opts = options ?? this.DefaultOptions;
            }

            options = options ?? this.DefaultOptions;
            return options;
        }
    }
}
//var state = dialogContext.ActiveDialog.State;
//var options = state[$"{this.Id}.options"];

//var currentId = ((string)state[$"{this.Id}.CurrentCommandId"]);

//// While we are in completed state process the commandSet.  
//while (true)
//{
//    foreach (var command in Commands
//        .SkipWhile(command => currentId != null && command.Id != currentId)
//        .SkipWhile(command => currentId != null && command.Id == currentId))
//    {
//        state[$"{this.Id}.CurrentCommandId"] = command.Id;
//        // execute dialog command
//        var commandResult = await command.Execute(dialogContext, cancellationToken).ConfigureAwait(false);
//        if (commandResult is DialogTurnResult)
//        {
//            return commandResult as DialogTurnResult;
//        }
//        if (commandResult is string && !String.IsNullOrEmpty((string)commandResult))
//        {
//            currentId = (string)commandResult;
//            continue; // go up and restart the command loop with new starting point
//        }
//    }
//    // hit end of command, treat this as a end of dialog
//    return await dialogContext.EndDialogAsync(state[$"{this.Id}.Result"], cancellationToken).ConfigureAwait(false);
//}
