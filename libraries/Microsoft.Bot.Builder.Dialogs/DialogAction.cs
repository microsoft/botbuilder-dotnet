using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class DialogAction : Dialog, IDialogDependencies
    {
        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            return OnRunCommandAsync(dc, options);
        }

        public virtual List<IDialog> ListDependencies()
        {
            return new List<IDialog>();
        }

        protected abstract Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken));

        protected async Task<DialogTurnResult> EndParentDialogAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            PopCommands(dc);

            if (dc.Stack.Count > 1 || dc.Parent == null)
            {
                return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.Parent.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        protected async Task<DialogTurnResult> ReplaceParentDialogAsync(DialogContext dc, string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            PopCommands(dc);

            if (dc.Stack.Count > 0 || dc.Parent == null)
            {
                return await dc.ReplaceDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.Parent.ReplaceDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        protected async Task<DialogTurnResult> RepeatParentDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            PopCommands(dc);

            var targetDialogId = dc.Parent.ActiveDialog.Id;

            var repeatedIds = dc.State.GetValue<List<string>>("__repeatedIds", new List<string>());
            if (repeatedIds.Contains(targetDialogId))
            {
                throw new ArgumentException($"Recursive loop detected, {targetDialogId} cannot be repeated twice in one turn.");
            }

            repeatedIds.Add(targetDialogId);

            var turnResult = await dc.Parent.ReplaceDialogAsync(dc.Parent.ActiveDialog.Id, options, cancellationToken).ConfigureAwait(false);
            turnResult.ParentEnded = true;
            return turnResult;
        }

        protected async Task<DialogTurnResult> CancelAllParentDialogsAsync(DialogContext dc, object result = null, string eventName = "cancelDialog", object eventValue = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            PopCommands(dc);

            if (dc.Stack.Count > 0 || dc.Parent == null)
            {
                return await dc.CancelAllDialogsAsync(eventName, eventValue, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var turnResult = await dc.Parent.CancelAllDialogsAsync(eventName, eventValue, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
        }

        private static void PopCommands(DialogContext dc)
        {
            // Pop all commands off the stack
            var i = dc.Stack.Count - 1;

            while (i > 0)
            {
                // Commands store the index of the state they are inheriting so we can tell a command
                // by looking to see if its state is of type int
                if (dc.Stack[i].StackIndex.HasValue)
                {
                    dc.Stack.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
