// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContext
    {
        private List<string> activeTags = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="state">Current dialog state.</param>
        public DialogContext(DialogSet dialogs, DialogContext parentDialogContext, DialogState state, StateMap conversationState = null, StateMap userState = null)
        {
            Dialogs = dialogs;
            ParentContext = parentDialogContext ?? throw new ArgumentNullException(nameof(parentDialogContext));
            Context = ParentContext.Context;
            Stack = state.DialogStack;
            conversationState = conversationState ?? state?.ConversationState ?? new StateMap();
            userState = userState ?? state?.UserState ?? new StateMap();

            State = new DialogContextState(this, userState, conversationState);
        }

        public DialogContext(DialogSet dialogs, ITurnContext turnContext, DialogState state, StateMap conversationState = null, StateMap userState = null)
        {
            ParentContext = null;
            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            Context = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            Stack = state.DialogStack;
            conversationState = conversationState ?? state?.ConversationState ?? new StateMap();
            userState = userState ?? state?.UserState ?? new StateMap();

            State = new DialogContextState(this, userState, conversationState);
        }

        public DialogContext ParentContext { get; protected set; }

        public DialogSet Dialogs { get; private set; }

        public ITurnContext Context { get; private set; }

        public List<DialogInstance> Stack { get; private set; }

        public DialogContextState State { get; private set; }

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
                DialogInstance instance = null;

                if (Stack.Any())
                {
                    // For DialogCommand instances we need to return the inherited state.
                    var frame = Stack.First();

                    instance = new DialogInstance()
                    {
                        Id = frame.Id,
                        State = GetActiveDialogState(this, frame.State),
                    };
                }

                return instance;
            }
        }

        /// <summary>
        /// Returns a list of all `Dialog.tags` that are currently on the dialog stack. 
        /// Any duplicate tags are removed from the returned list and the order of the tag reflects the
        /// order of the dialogs on the stack.
        /// The returned list will also include any tags applied as "globalTags". These tags are
        /// retrieved by calling context.TurnState.get('globalTags')` and will therefore need to be
        /// assigned for every turn of conversation using context.TurnState.set('globalTags', ['myTag'])`.
        /// </summary>
        public List<string> ActiveTags
        {
            get
            {
                // Cache tags on first request
                if (activeTags == null)
                {
                    // Get parent tags that are active
                    if (ParentContext != null)
                    {
                        activeTags = ParentContext.ActiveTags;
                    }
                    else
                    {
                        activeTags = Context.TurnState.Get<List<string>>("globalTags") ?? new List<string>();
                    }
                }

                // Add tags for current dialog stack
                foreach (var instance in Stack)
                {
                    var dialog = FindDialog(instance.Id);

                    if (dialog != null && dialog.Tags.Any())
                    {
                        activeTags = activeTags.Union(dialog.Tags).ToList();
                    }
                }
                return activeTags;
            }
        }

        public StateMap DialogState
        {
            get
            {
                return ActiveDialog?.State as StateMap;
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

            // Lookup dialog
            var dialog = this.FindDialog(dialogId);
            if (dialog == null)
            {
                throw new Exception($"DialogContext.BeginDialogAsync(): A dialog with an id of '{dialogId}' wasn't found.");
            }

            // Process dialogs input bindings
            // - If the stack is empty, any 'dialog.*' bindings will be pulled from the active dialog on
            //   the parents stack.
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }

            // TODO: reevaluate assumptions here, breaking options with this approach
            if (options?.GetType() == typeof(Dictionary<string, object>))
            {
                foreach (var option in dialog.InputBindings)
                {
                    var bindingKey = option.Key;
                    var bindingValue = option.Value;

                    var value = State.GetValue<string>(bindingValue);

                    (options as Dictionary<string, object>)[bindingKey] = value;
                }
            }

            // Check for inherited state
            // Local stack references are positive numbers and negative numbers are references on the
            // parents stack.
            object state = null;

            if (dialog is DialogCommand)
            {
                if (Stack.Count > 0)
                {
                    state = Stack.Count - 1;
                }
                else if (ParentContext != null)
                {
                    // We can't use -0 so index 0 in the parent's stack is encoded as -1
                    state = 0 - ParentContext.Stack.Count;
                }

                // Find stack entry to inherit
                for (int i = Stack.Count - 1; i >= 0; i--)
                {
                    if (Stack[i].GetType() == typeof(object))
                    {
                        state = i;
                        break;
                    }
                }
            }

            if (state == null)
            {
                state = new StateMap();
            }

            // Push new instance onto stack.
            var instance = new DialogInstance
            {
                Id = dialogId,
                State = state,
            };

            Stack.Insert(0, instance);
            activeTags = null;

            // Call dialogs BeginAsync() method.
            return await dialog.BeginDialogAsync(this, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper function to simplify formatting the options for calling a prompt dialog. This helper will
        /// take a `PromptOptions` argument and then call[begin(context, dialogId, options)](#begin).
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
        /// its `Dialog.ContinueDialogAsync()` method. You can check `context.responded` after the call completes
        /// to determine if a dialog was run and a reply was sent to the user.
        /// </summary>
        /// <remarks>
        /// The ConsultDialogAsync method will be called to find the preferred function to invoke.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> ContinueDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Consult dialog for processor to invoke.
            var consultation = await ConsultDialogAsync(cancellationToken).ConfigureAwait(false);

            if (consultation != null)
            {
                return await consultation.Processor(this).ConfigureAwait(false);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Empty);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> EndDialogAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // End the active dialog
            await EndActiveDialogAsync(DialogReason.EndCalled, result).ConfigureAwait(false);
            activeTags = null;

            // Resume parent dialog
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = this.FindDialog(ActiveDialog.Id);
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
        public Task<DialogTurnResult> CancelAllDialogsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return CancelAllDialogsAsync(null, null, cancellationToken);
        }

        public async Task<DialogTurnResult> CancelAllDialogsAsync(string eventName = "cancelDialog", object eventValue = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            activeTags = null;

            if (Stack.Any() || ParentContext != null)
            {
                // Cancel all local and parent dialogs while checking for interception
                var notify = false;
                var dialogContext = this;

                while (dialogContext != null)
                {
                    if (dialogContext.Stack.Any())
                    {
                        // Check to see if the dialog wants to handle the event
                        if (notify)
                        {
                            var eventHandled = await dialogContext.EmitEventAsync(eventName, eventValue, false).ConfigureAwait(false);

                            if (eventHandled)
                            {
                                break;
                            }
                        }

                        // End the active dialog
                        await dialogContext.EndActiveDialogAsync(DialogReason.CancelCalled).ConfigureAwait(false);
                    }
                    else
                    {
                        dialogContext = dialogContext.ParentContext;
                    }
                    notify = true;
                }

                return new DialogTurnResult(DialogTurnStatus.Cancelled);
            }
            else
            {
                // Stack was empty and no parent
                return new DialogTurnResult(DialogTurnStatus.Empty);
            }
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
            // Pop stack
            if (Stack.Any())
            {
                Stack.RemoveAt(0);
            }

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
            // Emit 'RepromptDialog' event
            var handled = await EmitEventAsync("repromptDialog", null, false).ConfigureAwait(false);

            if (!handled)
            {
                // Check for a dialog on the stack
                if (ActiveDialog != null)
                {
                    // Lookup dialog
                    var dialog = this.FindDialog(ActiveDialog.Id);
                    if (dialog == null)
                    {
                        throw new Exception($"DialogSet.RepromptDialogAsync(): Can't find A dialog with an id of '{ActiveDialog.Id}'.");
                    }

                    // Ask dialog to re-prompt if supported
                    await dialog.RepromptDialogAsync(Context, ActiveDialog, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Find the dialog id for the given context. 
        /// </summary>
        /// <param name="dialogId">dialog id to find</param>
        /// <returns>dialog with that id</returns>
        public IDialog FindDialog(string dialogId)
        {
            if (this.Dialogs != null)
            {
                var dialog = this.Dialogs.Find(dialogId);
                if (dialog != null)
                {
                    return dialog;
                }
            }

            if (this.ParentContext != null)
            {
                return this.ParentContext.FindDialog(dialogId);
            }

            return null;
        }

        /// <summary>
        /// Searches for a dialog with a given ID.
        /// Emits a named event for the current dialog, or someone who started it, to handle.
        /// </summary>
        /// <param name="name">Name of the event to raise.</param>
        /// <param name="value">(Optional) value to send along with the event.</param>
        /// <param name="bubble">(Optional) flag to control whether the event should be bubbled to its parent if not handled locally. Defaults to a value of `true`.</param>
        /// <returns>True if the event was handled.</returns>
        public async Task<bool> EmitEventAsync(string name, object value = null, bool bubble = true)
        {
            // Initialize event
            var dialogEvent = new DialogEvent()
            {
                Bubble = bubble,
                Name = name,
                Value = value,
            };

            // Dispatch to active dialog first
            var handled = false;
            var dc = this;

            while (true)
            {
                var instance = dc.ActiveDialog;

                if (instance != null)
                {
                    var dialog = dc.FindDialog(instance.Id);

                    if (dialog != null)
                    {
                        handled = await dialog.OnDialogEventAsync(dc, dialogEvent).ConfigureAwait(false);
                    }
                }

                // Break out if not bubbling or no parent
                if (!handled && dialogEvent.Bubble && ParentContext != null)
                {
                    dc = ParentContext;
                }
                else
                {
                    break;
                }
            }

            return handled;
        }

        /// <summary>
        /// Queries the active dialog about its desire to process the current utterance.
        /// </summary>
        /// <remarks>
        /// If there's an active multi-turn dialog on the stack, the dialog will return a processor 
        /// function that can be invoked to continue execution of the multi-turn dialog.
        /// </remarks>
        /// <returns>Consultation result.</returns>
        public async Task<DialogConsultation> ConsultDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check for a dialog on the stack
            if (ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = FindDialog(ActiveDialog.Id);

                if (dialog == null)
                {
                    throw new Exception($"`DialogContext.ConsultDialogAsync(): Can't consult dialog. A dialog with an id of '{ActiveDialog.Id}' wasn't found.");
                }

                // Consult dialog
                return await dialog.ConsultDialogAsync(this, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        private async Task EndActiveDialogAsync(DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var instance = ActiveDialog;
            if (instance != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(instance.Id);
                if (dialog != null)
                {
                    // Notify dialog of end
                    await dialog.EndDialogAsync(Context, instance, reason, cancellationToken).ConfigureAwait(false);
                }

                // Pop dialog off stack
                Stack.RemoveAt(0);

                // Process dialogs output binding
                if (!string.IsNullOrEmpty(dialog?.OutputBinding) && result != null)
                {
                    this.State.SetValue(dialog.OutputBinding, result);
                }
            }
        }

        private object GetActiveDialogState(DialogContext dialogContext, object state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.GetType() == typeof(int))
            {
                // Positive values are indexes within the current DC and negative values are indexes in
                // the parent DC.
                int stateIndex = (int)state;

                if (stateIndex >= 0)
                {
                    if (stateIndex < dialogContext.Stack.Count)
                    {
                        return this.GetActiveDialogState(dialogContext, dialogContext.Stack[stateIndex].State);
                    }
                    else

                    {
                        throw new Exception("DialogContext.ActiveDialog: Can't find inherited state. Index out of range.");
                    }
                }
                else if (dialogContext.ParentContext != null)
                {
                    return this.GetActiveDialogState(dialogContext.ParentContext, -stateIndex - 1);
                }
                else
                {
                    throw new Exception("DialogContext.ActiveDialog: Can't find inherited state. No parent DialogContext.");
                }
            }
            else
            {
                return state;
            }
        }
    }
}
