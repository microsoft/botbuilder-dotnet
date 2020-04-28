// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Memory;
using static Microsoft.Bot.Builder.Dialogs.Debugging.DebugSupport;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Provides context for the current state of the dialog stack.
    /// </summary>
    /// <remarks>The <see cref="Context"/> property contains the <see cref="ITurnContext"/>
    /// for the current turn.</remarks>
    [System.Diagnostics.DebuggerDisplay("{GetType().Name}[{ActiveDialog?.Id}]")]
    public class DialogContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class from the turn context.
        /// </summary>
        /// <param name="dialogs">The dialog set to create the dialog context for.</param>
        /// <param name="turnContext">The current turn context.</param>
        /// <param name="state">The state property from which to retrieve the dialog context.</param>
        public DialogContext(DialogSet dialogs, ITurnContext turnContext, DialogState state)
        {
            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            Context = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            Stack = state.DialogStack;
            State = new DialogStateManager(this);
            Services = new TurnContextStateCollection();

            ObjectPath.SetPathValue(turnContext.TurnState, TurnPath.Activity, Context.Activity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="parentDialogContext">Parent dialog context.</param>
        /// <param name="state">Current dialog state.</param>
        public DialogContext(
            DialogSet dialogs,
            DialogContext parentDialogContext,
            DialogState state)
            : this(dialogs, parentDialogContext.Context, state)
        {
            Parent = parentDialogContext ?? throw new ArgumentNullException(nameof(parentDialogContext));
            
            if (Parent.Services != null)
            {
                // copy parent services into this dialogcontext.
                foreach (var service in Parent.Services)
                {
                    Services[service.Key] = service.Value;
                }
            }
        }

        /// <summary>
        /// Gets the set of dialogs which are active for the current dialog container.
        /// </summary>
        /// <value>
        /// The set of dialogs which are active for the current dialog container.
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
        /// Gets or sets the parent <see cref="DialogContext"/>, if any. Used when searching for the ID of a dialog to start.
        /// </summary>
        /// <value>
        /// The parent <see cref="DialogContext"/>, if any. Used when searching for the ID of a dialog to start.
        /// </value>
        public DialogContext Parent { get; set; }

        /// <summary>
        /// Gets dialog context for child if there is an active child.
        /// </summary>
        /// <value>
        /// Dialog context for child if there is an active child.
        /// </value>
        public DialogContext Child
        {
            get
            {
                var instance = this.ActiveDialog;

                if (instance != null)
                {
                    var dialog = FindDialog(instance.Id);

                    if (dialog is DialogContainer container)
                    {
                        return container.CreateChildContext(this);
                    }
                }

                return null;
            }
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
        /// Gets or sets the DialogStateManager which manages view of all memory scopes.
        /// </summary>
        /// <value>
        /// DialogStateManager with unified memory view of all memory scopes.
        /// </value>
        public DialogStateManager State { get; set; }

        /// <summary>
        /// Gets the services collection which is contextual to this dialog context.
        /// </summary>
        /// <value>Services collection.</value>
        public TurnContextStateCollection Services { get; private set; }

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
        /// <seealso cref="EndDialogAsync(object, CancellationToken)"/>
        /// <seealso cref="PromptAsync(string, PromptOptions, CancellationToken)"/>
        /// <seealso cref="Dialog.BeginDialogAsync(DialogContext, object, CancellationToken)"/>
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
            await this.DebuggerStepAsync(dialog, DialogEvents.BeginDialog, cancellationToken).ConfigureAwait(false);
            return await dialog.BeginDialogAsync(this, options: options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper function to simplify formatting the options for calling a prompt dialog. This helper will
        /// take an <paramref name="options"/> argument and then call
        /// <see cref="BeginDialogAsync(string, object, CancellationToken)"/>.
        /// </summary>
        /// <param name="dialogId">ID of the prompt dialog to start.</param>
        /// <param name="options">Information to pass to the prompt dialog being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        /// <seealso cref="BeginDialogAsync(string, object, CancellationToken)"/>
        /// <seealso cref="Prompt{T}.BeginDialogAsync(DialogContext, object, CancellationToken)"/>
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
        /// Continues execution of the active dialog, if there is one, by passing the current
        /// <see cref="DialogContext"/> to the active dialog's
        /// <see cref="Dialog.ContinueDialogAsync(DialogContext, CancellationToken)"/> method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.
        ///
        /// Check the <see cref="TurnContext.Responded"/> property after the call completes
        /// to determine if the active dialog sent a reply message to the user.
        /// </remarks>
        /// <seealso cref="Dialog.ContinueDialogAsync(DialogContext, CancellationToken)"/>
        public async Task<DialogTurnResult> ContinueDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // if we are continuing and haven't emitted the activityReceived event, emit it
            // NOTE: This is backward compatible way for activity received to be fired even if you have legacy dialog loop
            if (!this.Context.TurnState.ContainsKey("activityReceivedEmitted"))
            {
                this.Context.TurnState["activityReceivedEmitted"] = true;

                // Dispatch "activityReceived" event
                // - This will queue up any interruptions.
                await this.EmitEventAsync(DialogEvents.ActivityReceived, value: this.Context.Activity, bubble: true, fromLeaf: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (this.ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = this.FindDialog(this.ActiveDialog.Id);

                if (dialog == null)
                {
                    throw new Exception($"Failed to continue dialog. A dialog with id {this.ActiveDialog.Id} could not be found.");
                }

                // Continue dialog execution
                return await dialog.ContinueDialogAsync(this, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Empty);
            }
        }

        /// <summary>
        /// Ends a dialog by popping it off the stack and returns an optional result to the dialog's
        /// parent. The parent dialog is the dialog the started the on being ended via a call to
        /// either <see cref="BeginDialogAsync(string, object, CancellationToken)"/> or
        /// <see cref="PromptAsync(string, PromptOptions, CancellationToken)"/>. The parent dialog
        /// will have its <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/>
        /// method invoked with any returned result. If the parent dialog hasn't implemented a `ResumeDialogAsync`
        /// method, then it will be automatically ended as well and the result passed to its parent.
        /// If there are no more parent dialogs on the stack then processing of the turn will end.
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
        /// return a result to the parent dialog. If the parent dialog does not implement `ResumeDialogAsync`,
        /// then the parent will end, too, and the result passed to the next parent context.
        ///
        /// The returned <see cref="DialogTurnResult"/> contains the return value in its
        /// <see cref="DialogTurnResult.Result"/> property.</remarks>
        /// <seealso cref="BeginDialogAsync(string, object, CancellationToken)"/>
        /// <seealso cref="PromptAsync(string, PromptOptions, CancellationToken)"/>
        /// <seealso cref="ReplaceDialogAsync(string, object, CancellationToken)"/>
        /// <seealso cref="Dialog.EndDialogAsync(ITurnContext, DialogInstance, DialogReason, CancellationToken)"/>
        public async Task<DialogTurnResult> EndDialogAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{this.ActiveDialog.Id}.EndDialogAsync() You can't pass a cancellation token as the result of a dialog when calling EndDialog.");
            }

            // End the active dialog
            await EndActiveDialogAsync(DialogReason.EndCalled, result: result, cancellationToken: cancellationToken).ConfigureAwait(false);

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
                await this.DebuggerStepAsync(dialog, "ResumeDialog", cancellationToken).ConfigureAwait(false);
                return await dialog.ResumeDialogAsync(this, DialogReason.EndCalled, result: result, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Complete, result);
            }
        }

        /// <summary>
        /// Deletes any existing dialog stack thus canceling all dialogs on the stack.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates that dialogs were canceled after the
        /// turn was processed by the dialog or that the stack was already empty.
        ///
        /// In general, the parent context is the dialog or bot turn handler that started the dialog.
        /// If the parent is a dialog, the stack calls the parent's
        /// <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/>
        /// method to return a result to the parent dialog. If the parent dialog does not implement
        /// `ResumeDialogAsync`, then the parent will end, too, and the result is passed to the next
        /// parent context.</remarks>
        /// <seealso cref="EndDialogAsync(object, CancellationToken)"/>
        public async Task<DialogTurnResult> CancelAllDialogsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.CancelAllDialogsAsync(false, eventName: null, eventValue: null, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes any existing dialog stack thus canceling all dialogs on the stack.
        /// </summary>
        /// <param name="cancelParents">If true the cancellation will bubble up through any parent dialogs as well.</param>
        /// <param name="eventName">The event.</param>
        /// <param name="eventValue">The event value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates that dialogs were canceled after the
        /// turn was processed by the dialog or that the stack was already empty.
        ///
        /// In general, the parent context is the dialog or bot turn handler that started the dialog.
        /// If the parent is a dialog, the stack calls the parent's
        /// <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/>
        /// method to return a result to the parent dialog. If the parent dialog does not implement
        /// `ResumeDialogAsync`, then the parent will end, too, and the result is passed to the next
        /// parent context.</remarks>
        /// <seealso cref="EndDialogAsync(object, CancellationToken)"/>
        public async Task<DialogTurnResult> CancelAllDialogsAsync(bool cancelParents, string eventName = null, object eventValue = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (eventValue is CancellationToken)
            {
                throw new ArgumentException($"{nameof(eventValue)} cannot be a cancellation token");
            }

            eventName = eventName ?? DialogEvents.CancelDialog;

            if (Stack.Any() || Parent != null)
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
                            var eventHandled = await dialogContext.EmitEventAsync(eventName, eventValue, bubble: false, fromLeaf: false, cancellationToken: cancellationToken).ConfigureAwait(false);

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
                        dialogContext = cancelParents ? dialogContext.Parent : null;
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
        /// <seealso cref="BeginDialogAsync(string, object, CancellationToken)"/>
        /// <seealso cref="EndDialogAsync(object, CancellationToken)"/>
        public async Task<DialogTurnResult> ReplaceDialogAsync(string dialogId, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // End the current dialog and giving the reason.
            await EndActiveDialogAsync(DialogReason.ReplaceCalled, cancellationToken: cancellationToken).ConfigureAwait(false);

            ObjectPath.SetPathValue(this.Context.TurnState, "turn.__repeatDialogId", dialogId);

            // Start replacement dialog
            return await BeginDialogAsync(dialogId, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls the currently active dialog's
        /// <see cref="Dialog.RepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/>
        /// method. Used with dialogs that implement a re-prompt behavior.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="Dialog.RepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/>
        public async Task RepromptDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Emit 'RepromptDialog' event
            var handled = await EmitEventAsync(name: DialogEvents.RepromptDialog, value: null, bubble: false, fromLeaf: false, cancellationToken: cancellationToken).ConfigureAwait(false);

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
                    await this.DebuggerStepAsync(dialog, DialogEvents.RepromptDialog, cancellationToken).ConfigureAwait(false);
                    await dialog.RepromptDialogAsync(Context, ActiveDialog, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Find the dialog id for the given context.
        /// </summary>
        /// <param name="dialogId">dialog id to find.</param>
        /// <returns>dialog with that id.</returns>
        public Dialog FindDialog(string dialogId)
        {
            if (this.Dialogs != null)
            {
                var dialog = this.Dialogs.Find(dialogId);
                if (dialog != null)
                {
                    return dialog;
                }
            }

            if (this.Parent != null)
            {
                return this.Parent.FindDialog(dialogId);
            }

            return null;
        }

        /// <summary>
        /// Searches for a dialog with a given ID.
        /// Emits a named event for the current dialog, or someone who started it, to handle.
        /// </summary>
        /// <param name="name">Name of the event to raise.</param>
        /// <param name="value">Value to send along with the event.</param>
        /// <param name="bubble">Flag to control whether the event should be bubbled to its parent if not handled locally. Defaults to a value of `true`.</param>
        /// <param name="fromLeaf">Whether the event is emitted from a leaf node.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the event was handled.</returns>
        public async Task<bool> EmitEventAsync(string name, object value = null, bool bubble = true, bool fromLeaf = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Initialize event
            var dialogEvent = new DialogEvent()
            {
                Bubble = bubble,
                Name = name,
                Value = value,
            };

            var dc = this;

            // Find starting dialog
            if (fromLeaf)
            {
                while (true)
                {
                    var childDc = dc.Child;

                    if (childDc != null)
                    {
                        dc = childDc;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Dispatch to active dialog first
            var instance = dc.ActiveDialog;

            if (instance != null)
            {
                var dialog = dc.FindDialog(instance.Id);

                if (dialog != null)
                {
                    await this.DebuggerStepAsync(dialog, name, cancellationToken).ConfigureAwait(false);
                    return await dialog.OnDialogEventAsync(dc, dialogEvent, cancellationToken).ConfigureAwait(false);
                }
            }

            return false;
        }

        private async Task EndActiveDialogAsync(DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            var instance = ActiveDialog;
            if (instance != null)
            {
                // Lookup dialog
                var dialog = Dialogs.Find(instance.Id);
                if (dialog != null)
                {
                    // Notify dialog of end
                    await this.DebuggerStepAsync(dialog, "EndDialog", cancellationToken).ConfigureAwait(false);
                    await dialog.EndDialogAsync(Context, instance, reason, cancellationToken).ConfigureAwait(false);
                }

                // Pop dialog off stack
                Stack.RemoveAt(0);

                // set Turn.LastResult to result
                ObjectPath.SetPathValue(this.Context.TurnState, TurnPath.LastResult, result);
            }
        }
    }
}
