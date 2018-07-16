// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContext
    {
        private Action<IDictionary<string, object>> _onCompleted;

        public DialogSet Dialogs { get; set; }

        public ITurnContext Context { get; set; }

        public List<DialogInstance> Stack { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Current dialog state.</param>
        /// <param name="onCompleted">An action to perform when the dialog completes, that is,
        /// when <see cref="EndAsync(IDictionary{string, object})"/> is called on the current context.</param>
        internal DialogContext(DialogSet dialogs, ITurnContext context, IDictionary<string, object> state, Action<IDictionary<string, object>> onCompleted = null)
        {
            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            _onCompleted = onCompleted;

            if (!state.TryGetValue("dialogStack", out var value))
            {
                value = new List<DialogInstance>();
                state["dialogStack"] = value;
            }

            Stack = (List<DialogInstance>)state["dialogStack"];
        }

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
        /// <param name="dialogArgs">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task BeginAsync(string dialogId, IDictionary<string, object> dialogArgs = null)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            // Lookup dialog
            var dialog = Dialogs.Find(dialogId);
            if (dialog == null)
            {
                throw new Exception($"DialogContext.begin(): A dialog with an id of '{dialogId}' wasn't found.");
            }

            // Push new instance onto stack.
            var instance = new DialogInstance
            {
                Id = dialogId,
                State = new Dictionary<string, object>(),
            };

            Stack.Insert(0, instance);

            // Call dialogs begin() method.
            await dialog.DialogBeginAsync(this, dialogArgs);
        }

        /// <summary>
        /// Helper function to simplify formatting the options for calling a prompt dialog. This helper will
        /// construct a `PromptOptions` structure and then call[begin(context, dialogId, options)](#begin).
        /// </summary>
        /// <param name="dialogId">ID of the prompt to start.</param>
        /// <param name="prompt">Initial prompt to send the user.</param>
        /// <param name="options">(Optional) array of choices to prompt the user for or additional prompt options.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task PromptAsync(string dialogId, string prompt, PromptOptions options = null)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (options == null)
            {
                options = new PromptOptions();
            }

            if (prompt != null)
            {
                options.PromptString = prompt;
            }

            return BeginAsync(dialogId, options);
        }

        public Task PromptAsync(string dialogId, Activity prompt, PromptOptions options = null)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            if (options == null)
            {
                options = new PromptOptions();
            }

            if (prompt != null)
            {
                options.PromptActivity = prompt;
            }

            return BeginAsync(dialogId, options);
        }

        /// <summary>
        /// Continues execution of the active dialog, if there is one, by passing the context object to
        /// its `Dialog.continue()` method. You can check `context.responded` after the call completes
        /// to determine if a dialog was run and a reply was sent to the user.
        /// </summary>
        public async Task ContinueAsync()
        {
            // Check for a dialog on the stack
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogSet.continue(): Can't continue dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Check for existence of a continue() method
                if (dialog is IDialogContinue)
                {
                        // Continue execution of dialog
                        await ((IDialogContinue)dialog).DialogContinueAsync(this);
                }
            }
        }

        /// <summary>
        /// Ends a dialog by popping it off the stack and returns an optional result to the dialogs
        /// parent.The parent dialog is the dialog the started the on being ended via a call to
        /// either[begin()](#begin) or [prompt()](#prompt).
        /// The parent dialog will have its `Dialog.resume()` method invoked with any returned
        /// result. If the parent dialog hasn't implemented a `resume()` method then it will be
        /// automatically ended as well and the result passed to its parent. If there are no more
        /// parent dialogs on the stack then processing of the turn will end.
        /// </summary>
        /// <param name="result"> (Optional) result to pass to the parent dialogs.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EndAsync(IDictionary<string, object> result = null)
        {
            // Pop active dialog off the stack
            if (Stack.Any())
            {
                Stack.RemoveAt(0);
            }

            // Resume previous dialog
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogContext.end(): Can't resume previous dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Check for existence of a resumeDialog() method
                if (dialog is IDialogResume)
                {
                    // Return result to previous dialog
                    await ((IDialogResume)dialog).DialogResumeAsync(this, result);
                }
                else
                {
                    // Just end the dialog and pass result to parent dialog
                    await EndAsync(result);
                }
            }
            else
            {
                _onCompleted?.Invoke(result);
            }
        }

        /// <summary>
        /// Deletes any existing dialog stack thus cancelling all dialogs on the stack.
        /// </summary>
        public DialogContext EndAll()
        {
            // Cancel any active dialogs
            Stack.Clear();
            return this;
        }

        /// <summary>
        /// Ends the active dialog and starts a new dialog in its place. This is particularly useful
        /// for creating loops or redirecting to another dialog.
        /// </summary>
        /// <param name="dialogId">ID of the new dialog to start.</param>
        /// <param name="dialogArgs">(Optional) additional argument(s) to pass to the new dialog.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReplaceAsync(string dialogId, IDictionary<string, object> dialogArgs = null)
        {
            // Pop stack
            if (Stack.Any())
            {
                Stack.RemoveAt(0);
            }

            // Start replacement dialog
            await BeginAsync(dialogId, dialogArgs);
        }
    }
}
