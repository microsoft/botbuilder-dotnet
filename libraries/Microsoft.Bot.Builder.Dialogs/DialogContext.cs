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
        private object _finalResult;

        public DialogSet Dialogs { get; set; }
        public ITurnContext Context { get; set; }
        public Stack<DialogInstance> Stack { get; set; }

        /// <summary>
        /// Creates a new DialogContext instance.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="context">Context for the current turn of conversation with the user.</param>
        /// <param name="stack">Current dialog stack.</param>
        internal DialogContext(DialogSet dialogs, ITurnContext context, Stack<DialogInstance> stack)
        {
            Dialogs = dialogs;
            Context = context;
            Stack = stack;
        }

        /// <summary>
        /// Returns the cached instance of the active dialog on the top of the stack or `undefined` if the stack is empty.
        /// </summary>
        public DialogInstance Instance
        {
            get
            {
                if (Stack.Any())
                {
                    return Stack.Peek();
                }
                return null;
            }
        }

        public DialogResult DialogResult
        {
            get
            {
                return new DialogResult
                {
                    Active = Stack.Count > 0,
                    Result = _finalResult
                };
            }
        }


        /// <summary>
        /// Pushes a new dialog onto the dialog stack.
        /// </summary>
        /// <param name="dialogId">ID of the dialog to start.</param>
        /// <param name="dialogArgs">(Optional) additional argument(s) to pass to the dialog being started.</param>
        public async Task Begin(string dialogId, object dialogArgs = null)
        {
            if (string.IsNullOrEmpty(dialogId))
                throw new ArgumentNullException(nameof(dialogId));

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
                State = new object()
            };

            Stack.Push(instance);

            // Call dialogs begin() method.
            await dialog.DialogBegin(this, dialogArgs);
        }

        /// <summary>
        /// Helper function to simplify formatting the options for calling a prompt dialog. This helper will
        /// construct a `PromptOptions` structure and then call[begin(context, dialogId, options)](#begin).
        /// </summary>
        /// <param name="dialogId">ID of the prompt to start.</param>
        /// <param name="prompt">Initial prompt to send the user.</param>
        /// <param name="choicesOrOptions">(Optional) array of choices to prompt the user for or additional prompt options.</param>
            
        public Task Prompt(string dialogId, string prompt, PromptOptions options = null)
        {
            if (string.IsNullOrEmpty(dialogId))
                throw new ArgumentNullException(nameof(dialogId));

            if (options == null)
            {
                options = new PromptOptions();
            }
            if (prompt != null)
            {
                options.PromptString = prompt;
            }
            return Begin(dialogId, options);
        }
        public Task Prompt(string dialogId, Activity prompt, PromptOptions options = null)
        {
            if (string.IsNullOrEmpty(dialogId))
                throw new ArgumentNullException(nameof(dialogId));

            if (options == null)
            {
                options = new PromptOptions();
            }
            if (prompt != null)
            {
                options.PromptActivity = prompt;
            }
            return Begin(dialogId, options);
        }

        /// <summary>
        /// Continues execution of the active dialog, if there is one, by passing the context object to
        /// its `Dialog.continue()` method. You can check `context.responded` after the call completes
        /// to determine if a dialog was run and a reply was sent to the user.
        /// </summary>
        public async Task Continue()
        {
            // Check for a dialog on the stack
            if (Instance != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(Instance.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogSet.continue(): Can't continue dialog. A dialog with an id of '{Instance.Id}' wasn't found.");
                }

                // Check for existence of a continue() method
                if (dialog.HasDialogContinue)
                {
                        // Continue execution of dialog
                        await dialog.DialogContinue(this);
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
        /// @param result (Optional) result to pass to the parent dialogs `Dialog.resume()` method.
        public async Task End(object result = null)
        {
            // Pop active dialog off the stack
            if (Stack.Any())
            {
                Stack.Pop();
            }

            // Resume previous dialog
            if (Instance != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(Instance.Id);
                if (dialog == null)
                {
                    throw new Exception($"DialogContext.end(): Can't resume previous dialog. A dialog with an id of '{Instance.Id}' wasn't found.");
                }

                // Check for existence of a resumeDialog() method
                if (dialog.HasDialogResume)
                {
                    // Return result to previous dialog
                    await dialog.DialogResume(this, result);
                }
                else
                {
                    // Just end the dialog and pass result to parent dialog
                    await End(result);
                }
            }
            else
            {
                _finalResult = result;
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
        public async Task Replace(string dialogId, object dialogArgs = null)
        {
            // Pop stack
            if (Stack.Any())
            {
                Stack.Pop();
            }

            // Start replacement dialog
            await Begin(dialogId, dialogArgs);
        }
    }
}
