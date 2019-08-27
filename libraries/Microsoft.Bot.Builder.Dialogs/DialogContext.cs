// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Dialogs.Debugging.DebugSupport;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContext
    {
        private List<string> activeTags = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContext"/> class.
        /// </summary>
        /// <param name="dialogs">Parent dialog set.</param>
        /// <param name="parentDialogContext">Parent dialog state.</param>
        /// <param name="state">Current dialog state.</param>
        /// <param name="conversationState">Context for the current turn of conversation with the user.</param>
        /// <param name="userState">Context for the user state.</param>
        /// <param name="settings">Settings state.</param>
        public DialogContext(
            DialogSet dialogs, 
            DialogContext parentDialogContext, 
            DialogState state, 
            IDictionary<string, object> conversationState = null, 
            IDictionary<string, object> userState = null, 
            IDictionary<string, object> settings = null)
        {
            Dialogs = dialogs;
            Parent = parentDialogContext ?? throw new ArgumentNullException(nameof(parentDialogContext));
            Context = Parent.Context;
            Stack = state.DialogStack;
            settings = settings ?? Configuration.LoadSettings(Context.TurnState.Get<IConfiguration>());
            conversationState = conversationState ?? new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            userState = userState ?? new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            if (!Context.TurnState.TryGetValue("TurnStateMap", out object turnState))
            {
                turnState = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                Context.TurnState["TurnStateMap"] = turnState;
            }

            State = new DialogContextState(this, settings: settings, userState: userState, conversationState: conversationState, turnState: turnState as Dictionary<string, object>);
            State.SetValue(DialogContextState.TURN_ACTIVITY, Context.Activity);
        }

        public DialogContext(DialogSet dialogs, ITurnContext turnContext, DialogState state, IDictionary<string, object> conversationState = null, IDictionary<string, object> userState = null, IDictionary<string, object> settings = null)
        {
            Parent = null;
            Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            Context = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            Stack = state.DialogStack;
            settings = settings ?? Configuration.LoadSettings(Context.TurnState.Get<IConfiguration>());
            conversationState = conversationState ?? new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            userState = userState ?? new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            if (!Context.TurnState.TryGetValue("TurnStateMap", out object turnState))
            {
                turnState = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                Context.TurnState["TurnStateMap"] = turnState;
            }

            State = new DialogContextState(this, settings: settings, userState: userState, conversationState: conversationState, turnState: turnState as Dictionary<string, object>);
            State.SetValue(DialogContextState.TURN_ACTIVITY, Context.Activity);
        }

        /// <summary>
        /// Gets or sets parent context.
        /// </summary>
        /// <value>
        /// Parent context.
        /// </value>
        public DialogContext Parent { get; set; }

        /// <summary>
        /// Gets set of dialogs which are active for the current dialog container.
        /// </summary>
        /// <value>
        /// Set of dialogs which are active for the current dialog container.
        /// </value>
        public DialogSet Dialogs { get; private set; }

        /// <summary>
        /// Gets turn context.
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
        public IList<DialogInstance> Stack { get; private set; }

        /// <summary>
        /// Gets current active scoped state with (user|conversation|dialog|settings scopes).
        /// </summary>
        /// <value>
        /// Current active scoped state with (user|conversation|dialog|settings scopes).
        /// </value>
        public DialogContextState State { get; private set; }

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
                DialogInstance instance = null;

                if (Stack.Any())
                {
                    // For DialogCommand instances we need to return the inherited state.
                    var frame = Stack.First();

                    instance = new DialogInstance()
                    {
                        Id = frame.Id,
                        State = GetActiveDialogState(this, frame.State, frame.StackIndex),
                    };
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets a list of all `Dialog.tags` that are currently on the dialog stack.
        /// Any duplicate tags are removed from the returned list and the order of the tag reflects the
        /// order of the dialogs on the stack.
        /// The returned list will also include any tags applied as "globalTags". These tags are
        /// retrieved by calling context.TurnState.get('globalTags')` and will therefore need to be
        /// assigned for every turn of conversation using context.TurnState.set('globalTags', ['myTag'])`.
        /// </summary>
        /// <value>
        /// Returns a list of all `Dialog.tags` that are currently on the dialog stack.
        /// Any duplicate tags are removed from the returned list and the order of the tag reflects the
        /// order of the dialogs on the stack.
        /// The returned list will also include any tags applied as "globalTags". These tags are
        /// retrieved by calling context.TurnState.get('globalTags')` and will therefore need to be
        /// assigned for every turn of conversation using context.TurnState.set('globalTags', ['myTag'])`.
        /// </value>
        public List<string> ActiveTags
        {
            get
            {
                // Cache tags on first request
                if (activeTags == null)
                {
                    // Get parent tags that are active
                    if (Parent != null)
                    {
                        activeTags = Parent.ActiveTags;
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

        /// <summary>
        /// Gets the current dialog state for the active dialog.
        /// </summary>
        /// <value>
        /// The current dialog state for the active dialog.
        /// </value>
        public Dictionary<string, object> DialogState
        {
            get
            {
                return ActiveDialog?.State as Dictionary<string, object>;
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
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            // Lookup dialog
            var dialog = this.FindDialog(dialogId);
            if (dialog == null)
            {
                throw new Exception(
                    $"DialogContext.BeginDialogAsync(): A dialog with an id of '{dialogId}' wasn't found." +
                    " The dialog must be included in the current or parent DialogSet." +
                    " For example, if subclassing a ComponentDialog you can call AddDialog() within your constructor.");
            }

            // Process dialogs input bindings
            var bindings = new JObject();
            foreach (var binding in dialog.InputBindings)
            {
                var bindingKey = binding.Key;
                var bindingValue = binding.Value;

                if (State.TryGetValue<object>(bindingValue, out var value))
                {
                    bindings[bindingKey] = JToken.FromObject(value);
                }
            }

            // Check for inherited state
            // Local stack references are positive numbers and negative numbers are references on the
            // parents stack.
            Dictionary<string, object> state = null;
            int? stateIndex = null;

            if (ShouldInheritState(dialog))
            {
                if (Stack.Count > 0)
                {
                    stateIndex = Stack.Count - 1;
                }
                else if (Parent != null)
                {
                    // We can't use -0 so index 0 in the parent's stack is encoded as -1
                    stateIndex = 0 - Parent.Stack.Count;
                }

                // Find stack entry to inherit
                for (int i = Stack.Count - 1; i >= 0; i--)
                {
                    if (Stack[i].GetType() == typeof(object))
                    {
                        stateIndex = i;
                        break;
                    }
                }
            }

            if (state == null && !stateIndex.HasValue)
            {
                state = new Dictionary<string, object>();
            }

            // Push new instance onto stack.
            var instance = new DialogInstance
            {
                Id = dialogId,
                State = state,
                StackIndex = stateIndex
            };

            Stack.Insert(0, instance);
            activeTags = null;

            // take the bindings (dialog.xxx => dialog.yyy)
            foreach (var property in bindings)
            {
                // make sure the key is a dialog property this is only used for dialog bindings
                if (!property.Key.StartsWith("$") && !property.Key.ToLower().StartsWith("dialog."))
                {
                    throw new ArgumentOutOfRangeException($"{property.Key} is not a dialog property");
                }

                // Set the dialog property in the current state to the value from the bindings
                State.SetValue(property.Key, property.Value);
            }

            // set dialog result
            if (ShouldInheritState(dialog))
            {
                State.SetValue(DialogContextState.STEP_OPTIONS_PROPERTY, options);
            }
            else
            {
                State.SetValue(DialogContextState.DIALOG_OPTIONS, options);
            }

            // Call dialogs BeginAsync() method.
            await this.DebuggerStepAsync(dialog, DialogEvents.BeginDialog, cancellationToken).ConfigureAwait(false);
            return await dialog.BeginDialogAsync(this, options: options, cancellationToken: cancellationToken).ConfigureAwait(false);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> ContinueDialogAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Check for a dialog on the stack
            var instance = this.ActiveDialog;

            // if we are continuing and haven't emitted the activityReceived event, emit it
            // NOTE: This is backward compatible way for activity received to be fired even if you have legacy dialog loop
            if (!this.Context.TurnState.ContainsKey("activityReceivedEmitted"))
            {
                this.Context.TurnState["activityReceivedEmitted"] = true;

                // Dispatch "activityReceived" event
                // - This will queue up any interruptions.
                await this.EmitEventAsync(DialogEvents.ActivityReceived, value: this.Context.Activity, bubble: true, fromLeaf: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (instance != null)
            {
                // Lookup dialog
                var dialog = this.FindDialog(instance.Id);

                if (dialog == null)
                {
                    throw new Exception($"Failed to continue dialog. A dialog with id {instance.Id} could not be found.");
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
        /// Ends a dialog by popping it off the stack and returns an optional result to the dialogs
        /// parent.The parent dialog is the dialog the started the on being ended via a call to
        /// either[begin()](#begin) or [prompt()](#prompt).
        /// The parent dialog will have its `Dialog.resume()` method invoked with any returned
        /// result. If the parent dialog hasn't implemented a `resume()` method then it will be
        /// automatically ended as well and the result passed to its parent. If there are no more
        /// parent dialogs on the stack then processing of the turn will end.
        /// </summary>
        /// <param name="result">(Optional) result to pass to the parent dialogs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> EndDialogAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken token)
            {
                new ArgumentException($"{this.ActiveDialog.Id}.EndDialogAsync() You can't pass a cancellation token as the result of a dialog when calling EndDialog.");
            }

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
                await this.DebuggerStepAsync(dialog, DialogEvents.ResumeDialog, cancellationToken).ConfigureAwait(false);
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
        /// <param name="eventName">The event.</param>
        /// <param name="eventValue">The event value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The dialog context.</returns>
        public async Task<DialogTurnResult> CancelAllDialogsAsync(
            string eventName = DialogEvents.CancelDialog, 
            object eventValue = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (eventValue is CancellationToken)
            {
                throw new ArgumentException($"{nameof(eventValue)} cannot be a cancellation token");
            }

            activeTags = null;

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
                        dialogContext = dialogContext.Parent;
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
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // End the current dialog and giving the reason.
            await EndActiveDialogAsync(DialogReason.ReplaceCalled, cancellationToken: cancellationToken).ConfigureAwait(false);

            this.State.Turn["__repeatDialogId"] = dialogId;

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

        /// <summary>
        /// Specifies whether a given dialog should inherit dialog-level state. 
        /// </summary>
        /// <param name="dialog">The dialog to be tested.</param>
        /// <returns>Whether the passed dialog should inherit dialog-level state.</returns>
        protected virtual bool ShouldInheritState(IDialog dialog)
        {
            return dialog is DialogAction;
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
                    await this.DebuggerStepAsync(dialog, DialogEvents.EndDialog, cancellationToken).ConfigureAwait(false);
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

        private IDictionary<string, object> GetActiveDialogState(DialogContext dialogContext, IDictionary<string, object> state = null, int? stackIdx = null)
        {
            if (state == null && !stackIdx.HasValue)
            {
                throw new ArgumentNullException($"Either {nameof(state)} or {nameof(stackIdx)} must be provided");
            }

            if (stackIdx.HasValue)
            {
                // Positive values are indexes within the current DC and negative values are indexes in
                // the parent DC.
                int stackIndex = stackIdx.Value;

                for (int iStack = stackIndex; iStack < dialogContext.Stack.Count && iStack >= 0; iStack--)
                {
                    if (dialogContext.Stack[iStack].State != null)
                    {
                        return dialogContext.Stack[iStack].State;
                    }
                }

                if (dialogContext.Parent != null)
                {
                    return this.GetActiveDialogState(dialogContext.Parent, null, -stackIndex - 1);
                }

                return state;
            }
            else
            {
                return state;
            }
        }

        public class DialogEvents
        {
            public const string BeginDialog = "beginDialog";
            public const string ResumeDialog = "resumeDialog";
            public const string RepromptDialog = "repromptDialog";
            public const string CancelDialog = "cancelDialog";
            public const string EndDialog = "endDialog";
            public const string ActivityReceived = "activityReceived";
        }
    }
}
