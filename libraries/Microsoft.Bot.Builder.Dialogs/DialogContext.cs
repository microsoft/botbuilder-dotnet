// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Current dialog state.</param>
        internal DialogContext(DialogSet dialogs, ITurnContext turnContext, DialogState state)
        {
            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            Context = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            Stack = state.DialogStack;
        }

        /// <summary>
        /// Gets the set of dialogs that can be called from this context.
        /// </summary>
        /// <value>
        /// The set of dialogs that can be called from this context.
        /// </value>
        public DialogSet Dialogs { get; private set; }

        /// <summary>
        /// Gets the context for the current turn of conversation.
        /// </summary>
        /// <value>
        /// The context for the current turn of conversation.
        /// </value>
        public ITurnContext Context { get; private set; }

        /// <summary>
        /// Gets the current dialog stack.
        /// </summary>
        /// <value>
        /// The current dialog stack.
        /// </value>
        public List<DialogInstance> Stack { get; private set; }

        /// <summary>
        /// Gets or sets the The parent DialogContext if any. Used when searching for dialogs to start.
        /// </summary>
        /// <value>
        /// The The parent DialogContext if any. Used when searching for dialogs to start.
        /// </value>
        public DialogContext Parent { get; set; }

        /// <summary>
        /// Gets the cached instance of the active dialog on the top of the stack or <c>null</c> if the stack is empty.
        /// </summary>
        /// <value>
        /// The cached instance of the active dialog on the top of the stack or <c>null</c> if the stack is empty.
        /// </value>
        public DialogInstance ActiveDialog
        {
            get
            {
                if (Stack.Any())
                {
                    return Stack.First();
                }

                return null;
            }
        }

        /// <summary>
        /// Pushes a new dialog onto the dialog stack.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to start.</param>
        /// <param name="options">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> BeginDialogAsync(string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            // Look up dialog
            var dialog = FindDialog(dialogId);
            if (dialog == null)
            {
                throw new Exception(
                    $"DialogContext.BeginDialogAsync(): A dialog with an id of '{dialogId}' wasn't found." +
                    " The dialog must be included in the current or parent DialogSet." +
                    " For example, if subclassing a ComponentDialog you can call AddDialog() within your constructor.");
            }

            // Push new instance onto stack
            var instance = new DialogInstance
            {
                Id = dialogId,
                State = new Dictionary<string, object>(),
            };

            Stack.Insert(0, instance);

            // Call dialog's BeginAsync() method
            return await dialog.BeginDialogAsync(this, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper function to simplify formatting the options for calling a prompt dialog. This helper will
        /// take a `PromptOptions` argument and then call
        /// <see cref="BeginDialogAsync(string, object, CancellationToken)"/>.
        /// </summary>
        /// <param name="dialogId">ID of the prompt to start.</param>
        /// <param name="options">Contains a Prompt, potentially a RetryPrompt and if using ChoicePrompt, Choices.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> PromptAsync(string dialogId, PromptOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return await BeginDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Continues execution of the active dialog, if there is one, by passing the context object to
        /// its `Dialog.ContinueDialogAsync()` method. You can check `turnContext.Responded` after the call completes
        /// to determine if a dialog was run and a reply was sent to the user.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> ContinueDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check for a dialog on the stack
            if (ActiveDialog != null)
            {
                // Look up dialog
                var dialog = FindDialog(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogContext.ContinueDialogAsync(): Can't continue dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Continue execution of dialog
                return await dialog.ContinueDialogAsync(this, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Empty);
            }
        }

        /// <summary>
        /// Ends a dialog by popping it off the stack and returns an optional result to the dialog's
        /// parent. The parent dialog is the dialog that started the dialog being ended via a call to
        /// either <see cref="BeginDialogAsync(string, object, CancellationToken)"/>
        /// or <see cref="PromptAsync(string, PromptOptions, CancellationToken)"/>.
        /// The parent dialog will have its `Dialog.ResumeDialogAsync()` method invoked with any returned
        /// result. If the parent dialog hasn't implemented a `ResumeDialogAsync()` method then it will be
        /// automatically ended as well and the result passed to its parent. If there are no more
        /// parent dialogs on the stack then processing of the turn will end.
        /// </summary>
        /// <param name="result"> (Optional) result to pass to the parent dialogs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> EndDialogAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await EndActiveDialogAsync(DialogReason.EndCalled, cancellationToken).ConfigureAwait(false);

            // Resume previous dialog
            if (ActiveDialog != null)
            {
                // Look up dialog
                var dialog = FindDialog(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogContext.EndDialogAsync(): Can't resume previous dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Return result to previous dialog
                return await dialog.ResumeDialogAsync(this, DialogReason.EndCalled, result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Complete, result);
            }
        }

        /// <summary>
        /// Deletes any existing dialog stack thus cancelling all dialogs on the stack.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The dialog context.</returns>
        public async Task<DialogTurnResult> CancelAllDialogsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Stack.Any())
            {
                while (Stack.Any())
                {
                    await EndActiveDialogAsync(DialogReason.CancelCalled, cancellationToken).ConfigureAwait(false);
                }

                return new DialogTurnResult(DialogTurnStatus.Cancelled);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Empty);
            }
        }

        /// <summary>
        /// If the dialog cannot be found within the current `DialogSet`, the parent `DialogContext`
        /// will be searched if there is one.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to search for.</param>
        /// <returns>The first Dialog found that matches the dialogId.</returns>
        public Dialog FindDialog(string dialogId)
        {
            var dialog = Dialogs.Find(dialogId);
            if (dialog == null && Parent != null)
            {
                dialog = Parent.FindDialog(dialogId);
            }

            return dialog;
        }

        /// <summary>
        /// Ends the active dialog and starts a new dialog in its place. This is particularly useful
        /// for creating loops or redirecting to another dialog.
        /// </summary>
        /// <param name="dialogId">ID of the new dialog to start.</param>
        /// <param name="options">(Optional) additional argument(s) to pass to the new dialog.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> ReplaceDialogAsync(string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // End the current dialog and giving the reason.
            await EndActiveDialogAsync(DialogReason.ReplaceCalled, cancellationToken).ConfigureAwait(false);

            // Start replacement dialog
            return await BeginDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls reprompt on the currently active dialog, if there is one. Used with Prompts that have a reprompt behavior.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RepromptDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check for a dialog on the stack
            if (ActiveDialog != null)
            {
                // Look up dialog
                var dialog = FindDialog(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogSet.RepromptDialogAsync(): Can't find A dialog with an id of '{ActiveDialog.Id}'.");
                }

                // Ask dialog to re-prompt if supported
                await dialog.RepromptDialogAsync(Context, ActiveDialog, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task EndActiveDialogAsync(DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            var instance = ActiveDialog;
            if (instance != null)
            {
                // Look up dialog
                var dialog = FindDialog(instance.Id);
                if (dialog != null)
                {
                    // Notify dialog of end
                    await dialog.EndDialogAsync(Context, instance, reason, cancellationToken).ConfigureAwait(false);
                }

                // Pop dialog off stack
                Stack.RemoveAt(0);
            }
        }
    }
}
