// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Provides context for the current state of the dialog stack.
    /// </summary>
    /// <remarks>The <see cref="Context"/> property contains the <see cref="ITurnContext"/>
    /// for the current turn.</remarks>
    public class DialogContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Current dialog state.</param>
        public DialogContext(DialogSet dialogs, ITurnContext turnContext, DialogState state)
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
        /// Gets or sets the parent <see cref="DialogContext"/>, if any. Used when searching for the
        /// ID of a dialog to start.
        /// </summary>
        /// <value>
        /// The parent dialog context; or <c>null</c> if this dialog context does not have a parent.
        /// </value>
        public DialogContext Parent { get; set; }

        /// <summary>
        /// Gets the cached <see cref="DialogInstance"/> of the active dialog on the top of the stack
        /// or <c>null</c> if the stack is empty.
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
        /// Starts a new dialog and pushes it onto the dialog stack.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to start.</param>
        /// <param name="options">Optional, information to pass to the dialog being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        /// <seealso cref="PromptAsync(string, PromptOptions, CancellationToken)"/>
        /// <seealso cref="EndDialogAsync(object, CancellationToken)"/>
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
        /// Starts a new prompt dialog and pushes it onto the dialog stack.
        /// </summary>
        /// <param name="dialogId">ID of the prompt dialog to start.</param>
        /// <param name="options">Information to pass to the prompt dialog being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        /// <seealso cref="BeginDialogAsync(string, object, CancellationToken)"/>
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
        /// Continues execution of the active dialog, if there is one, by passing the current <see cref="DialogContext"/> to
        /// the active dialog's <see cref="Dialog.ContinueDialogAsync(DialogContext, CancellationToken)"/> method.
        /// Check the <see cref="TurnContext.Responded"/> property after the call completes
        /// to determine if the active dialog sent a reply message to the user.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
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
        /// Ends a dialog by popping it off the stack and returns an optional result to the dialog's logical parent,
        /// which is the next dialog on the stack, or the bot's turn handler if this was the last dialog on the stack.
        /// </summary>
        /// <param name="result">Optional, result to pass to the parent context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates that the dialog ended after the
        /// turn was processed by the dialog.
        ///
        /// In general, the parent context is the dialog or bot turn handler that started the dialog.
        /// If the parent is a dialog, the stack calls the parent's
        /// <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/> method to
        /// return a result to the parent dialog. If the parent dialog does not implement `ResumeDialogAsyn`,
        /// then the parent will end, too, and the result passed to the next parent context.
        ///
        /// The returned <see cref="DialogTurnResult"/> contains the return value in its
        /// <see cref="DialogTurnResult.Result"/> property.</remarks>
        /// <seealso cref="BeginDialogAsync(string, object, CancellationToken)"/>
        /// <seealso cref="PromptAsync(string, PromptOptions, CancellationToken)"/>
        /// <seealso cref="ReplaceDialogAsync(string, object, CancellationToken)"/>
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
        /// Deletes any existing dialog stack, thus cancelling all dialogs on the stack.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates that dialogs were cancelled after the
        /// turn was processed by the dialog.
        ///
        /// In general, the parent context is the dialog or bot turn handler that started the dialog.
        /// If the parent is a dialog, the stack calls the parent's
        /// <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/> method to
        /// return a result to the parent dialog. If the parent dialog does not implement `ResumeDialogAsyn`,
        /// then the parent will end, too, and the result passed to the next parent context.</remarks>
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
        /// Searches the current <see cref="DialogSet"/> and its parents for a <see cref="Dialog"/> by its ID.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to search for.</param>
        /// <returns>The first dialog found that matches the <paramref name="dialogId"/>.</returns>
        /// <remarks>If the dialog cannot be found within the current dialog set, the parent
        /// <see cref="DialogContext"/> will be searched if there is one.</remarks>
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
        /// Starts a new dialog and replaces on the stack the currently active dialog with the new one.
        /// This is particularly useful for creating loops or redirecting to another dialog.
        /// </summary>
        /// <param name="dialogId">ID of the new dialog to start.</param>
        /// <param name="options">Optional, information to pass to the dialog being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        public async Task<DialogTurnResult> ReplaceDialogAsync(string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // End the current dialog and giving the reason.
            await EndActiveDialogAsync(DialogReason.ReplaceCalled, cancellationToken).ConfigureAwait(false);

            // Start replacement dialog
            return await BeginDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls the currently active dialog's <see cref="Dialog.RepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/>,
        /// if there is one. Used with prompt dialogs that have a reprompt behavior.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
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
