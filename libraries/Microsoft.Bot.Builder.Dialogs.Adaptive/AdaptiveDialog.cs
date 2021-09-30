﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Functions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// The Adaptive Dialog models conversation using events and events to adapt dynamicaly to changing conversation flow.
    /// </summary>
    public class AdaptiveDialog : DialogContainer, IDialogDependencies
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.AdaptiveDialog";

        internal const string ConditionTracker = "dialog._tracker.conditions";

        private const string AdaptiveKey = "_adaptive";
        private const string DefaultOperationKey = "$defaultOperation";
        private const string ExpectedOnlyKey = "$expectedOnly";
        private const string InstanceKey = "$instance";
        private const string NoneIntentKey = "None";
        private const string OperationsKey = "$operations";
        private const string PropertyEnding = "Property";
        private const string RequiresValueKey = "$requiresValue";
        private const string UtteranceKey = "utterance";

        // unique key for change tracking of the turn state (TURN STATE ONLY)
        private readonly string changeTurnKey = Guid.NewGuid().ToString();

        private RecognizerSet recognizerSet = new RecognizerSet();

        private object syncLock = new object();
        private bool installedDependencies;

        private bool needsTracker = false;

        private SchemaHelper dialogSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialog"/> class.
        /// </summary>
        /// <param name="dialogId">Optional, dialog identifier.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AdaptiveDialog(string dialogId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogId)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets recognizer for processing incoming user input.
        /// </summary>
        /// <value>
        /// Recognizer for processing incoming user input.
        /// </value>
        [JsonProperty("recognizer")]
        public Recognizer Recognizer { get; set; }

        /// <summary>
        /// Gets or sets language Generator override.
        /// </summary>
        /// <value>
        /// Language Generator override.
        /// </value>
        [JsonProperty("generator")]
        public LanguageGenerator Generator { get; set; }

        /// <summary>
        /// Gets or sets trigger handlers to respond to conditions which modifying the executing plan. 
        /// </summary>
        /// <value>
        /// Trigger handlers to respond to conditions which modifying the executing plan. 
        /// </value>
        [JsonProperty("triggers")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public virtual List<OnCondition> Triggers { get; set; } = new List<OnCondition>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets an expression indicating whether to end the dialog when there are no actions to execute.
        /// </summary>
        /// <remarks>
        /// If true, when there are no actions to execute, the current dialog will end
        /// If false, when there are no actions to execute, the current dialog will simply end the turn and still be active.
        /// </remarks>
        /// <value>
        /// Whether to end the dialog when there are no actions to execute.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("autoEndDialog")]
        public BoolExpression AutoEndDialog { get; set; } = true;

        /// <summary>
        /// Gets or sets the selector for picking the possible events to execute.
        /// </summary>
        /// <value>
        /// The selector for picking the possible events to execute.
        /// </value>
        [JsonProperty("selector")]
        public TriggerSelector Selector { get; set; }

        /// <summary>
        /// Gets or sets the property to return as the result when the dialog ends when there are no more Actions and AutoEndDialog = true.
        /// </summary>
        /// <value>
        /// The property to return as the result when the dialog ends when there are no more Actions and AutoEndDialog = true.
        /// </value>
        [JsonProperty("defaultResultProperty")]
        public string DefaultResultProperty { get; set; } = "dialog.result";

        /// <summary>
        /// Gets or sets schema that describes what the dialog works over.
        /// </summary>
        /// <value>JSON Schema for the dialog.</value>
        [JsonProperty("schema")]
#pragma warning disable CA2227 // Collection properties should be read only
        public JObject Schema
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get => dialogSchema?.Schema;
            set
            {
                dialogSchema = new SchemaHelper(value);
            }
        }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} should not ever be a cancellation token");
            }

            EnsureDependenciesInstalled();

            await this.CheckForVersionChangeAsync(dc, cancellationToken).ConfigureAwait(false);

            // replace initial activeDialog.State with clone of options
            if (options != null)
            {
                dc.ActiveDialog.State = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(options));
            }

            if (!dc.State.ContainsKey(DialogPath.EventCounter))
            {
                dc.State.SetValue(DialogPath.EventCounter, 0u);
            }

            if (dialogSchema != null && !dc.State.ContainsKey(DialogPath.RequiredProperties))
            {
                // RequiredProperties control what properties must be filled in.
                // Initialize if not present from schema.
                dc.State.SetValue(DialogPath.RequiredProperties, dialogSchema.Required());
            }

            if (needsTracker)
            {
                if (!dc.State.ContainsKey(ConditionTracker))
                {
                    foreach (var trigger in Triggers)
                    {
                        if (trigger.RunOnce && trigger.Condition != null)
                        {
                            var paths = dc.State.TrackPaths(trigger.Condition.ToExpression().References());
                            var triggerPath = $"{ConditionTracker}.{trigger.Id}.";
                            dc.State.SetValue(triggerPath + "paths", paths);
                            dc.State.SetValue(triggerPath + "lastRun", 0u);
                        }
                    }
                }
            }

            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            activeDialogState[AdaptiveKey] = new AdaptiveDialogState();

            // Evaluate events and queue up step changes
            var dialogEvent = new DialogEvent
            {
                Name = AdaptiveEvents.BeginDialog,
                Value = options,
                Bubble = false
            };
            
            var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "Kind", Kind }
                };
            TelemetryClient.TrackEvent("AdaptiveDialogStart", properties);
            TelemetryClient.TrackDialogView(Id);

            await OnDialogEventAsync(dc, dialogEvent, cancellationToken).ConfigureAwait(false);

            // Continue step execution
            return await ContinueActionsAsync(dc, options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            EnsureDependenciesInstalled();

            await this.CheckForVersionChangeAsync(dc, cancellationToken).ConfigureAwait(false);

            // Continue step execution
            return await ContinueActionsAsync(dc, options: null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a child dialog completed its turn, returning control to this dialog.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">Optional, A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            await this.CheckForVersionChangeAsync(dc, cancellationToken).ConfigureAwait(false);

            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // resumeDialog() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);

            return EndOfTurn;
        }

        /// <summary>
        /// Called when the dialog is ending.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="instance">State information associated with the instance of this dialog on the dialog stack.</param>
        /// <param name="reason">Reason why the dialog ended.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "Kind", Kind }
                };

            if (reason == DialogReason.CancelCalled)
            {
                TelemetryClient.TrackEvent("AdaptiveDialogCancel", properties);
            }
            else if (reason == DialogReason.EndCalled)
            {
                TelemetryClient.TrackEvent("AdaptiveDialogComplete", properties);
            }

            return base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }

        /// <summary>
        /// RepromptDialog with dialogContext.
        /// </summary>
        /// <remarks>AdaptiveDialogs use the DC, which is available because AdaptiveDialogs handle the new AdaptiveEvents.RepromptDialog.</remarks>
        /// <param name="dc">dc.</param>
        /// <param name="instance">instance.</param>
        /// <param name="cancellationToken">ct.</param>
        /// <returns>task.</returns>
        public virtual async Task RepromptDialogAsync(DialogContext dc, DialogInstance instance, CancellationToken cancellationToken = default)
        {
            // Forward to current sequence step
            var state = (instance.State as Dictionary<string, object>)[AdaptiveKey] as AdaptiveDialogState;

            if (state.Actions.Any())
            {
                // We need to mockup a DialogContext so that we can call RepromptDialog
                // for the active step
                var childContext = CreateChildContext(dc);
                await childContext.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a child <see cref="DialogContext"/> for the given context.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>The child <see cref="DialogContext"/> or null if no <see cref="AdaptiveDialogState.Actions"/> are found for the given context.</returns>
        public override DialogContext CreateChildContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            AdaptiveDialogState state = null;

            if (activeDialogState.TryGetValue(AdaptiveKey, out var currentState))
            {
                state = currentState as AdaptiveDialogState;
            }

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[AdaptiveKey] = state;
            }

            if (state.Actions != null && state.Actions.Any())
            {
                var childContext = new DialogContext(this.Dialogs, dc, state.Actions.First());
                OnSetScopedServices(childContext);
                return childContext;
            }

            return null;
        }

        /// <summary>
        /// Gets <see cref="Dialog"/> enumerated dependencies.
        /// </summary>
        /// <returns><see cref="Dialog"/> enumerated dependencies.</returns>
        public IEnumerable<Dialog> GetDependencies()
        {
            EnsureDependenciesInstalled();

            // Expose required nested dependencies for parent dialog
            foreach (var dlg in Dialogs.GetDialogs())
            {
                if (dlg is IAdaptiveDialogDependencies dependencies)
                {
                    foreach (var item in dependencies.GetExternalDependencies())
                    {
                        yield return item;
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// Gets the internal version string.
        /// </summary>
        /// <returns>Internal version string.</returns>
        protected override string GetInternalVersion()
        {
            StringBuilder sb = new StringBuilder();

            // change the container version if any dialogs are added or removed.
            sb.Append(this.Dialogs.GetVersion());

            // change version if the schema has changed.
            if (this.Schema != null)
            {
                sb.Append(JsonConvert.SerializeObject(this.Schema));
            }

            // change if triggers type/constraint change 
            foreach (var trigger in Triggers)
            {
                sb.Append(trigger.GetExpression().ToString());
            }

            return StringUtils.Hash(sb.ToString());
        }

        /// <summary>
        /// Called before an event is bubbled to its parent.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="dialogEvent">The <see cref="DialogEvent"/> being raised.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected override async Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default)
        {
            var actionContext = ToActionContext(dc);

            // Process event and queue up any potential interruptions
            return await ProcessEventAsync(actionContext, dialogEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called after an event was bubbled to all parents and wasn't handled.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="dialogEvent">The <see cref="DialogEvent"/> being raised.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected override async Task<bool> OnPostBubbleEventAsync(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default)
        {
            var actionContext = ToActionContext(dc);

            // Process event and queue up any potential interruptions
            return await ProcessEventAsync(actionContext, dialogEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Event processing implementation.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current turn of conversation.</param>
        /// <param name="dialogEvent">The <see cref="DialogEvent"/> being raised.</param>
        /// <param name="preBubble">A flag indicator for preBubble processing.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> used to signal this operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> representation of a boolean indicator or the result.</returns>
        protected virtual async Task<bool> ProcessEventAsync(ActionContext actionContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Save into turn
            actionContext.State.SetValue(TurnPath.DialogEvent, dialogEvent);

            var activity = actionContext.State.GetValue<Activity>(TurnPath.Activity);

            // some dialogevents get promoted into turn state for general access outside of the dialogevent.
            // This allows events to be fired (in the case of ChooseIntent), or in interruption (Activity) 
            // Triggers all expressed against turn.recognized or turn.activity, and this mapping maintains that 
            // any event that is emitted updates those for the rest of rule evaluation.
            switch (dialogEvent.Name)
            {
                case AdaptiveEvents.RecognizedIntent:
                    {
                        // we have received a RecognizedIntent event
                        // get the value and promote to turn.recognized, topintent,topscore and lastintent
                        var recognizedResult = actionContext.State.GetValue<RecognizerResult>($"{TurnPath.DialogEvent}.value");

                        // #3572 set these here too (Even though the emitter may have set them) because this event can be emitted by declarative code.
                        var (name, score) = recognizedResult.GetTopScoringIntent();
                        actionContext.State.SetValue(TurnPath.Recognized, recognizedResult);
                        actionContext.State.SetValue(TurnPath.TopIntent, name);
                        actionContext.State.SetValue(TurnPath.TopScore, score);
                        actionContext.State.SetValue(DialogPath.LastIntent, name);

                        // process entities for ambiguity processing (We do this regardless of who handles the event)
                        ProcessEntities(actionContext, activity);
                        break;
                    }

                case AdaptiveEvents.ActivityReceived:
                    {
                        // We received an ActivityReceived event, promote the activity into turn.activity
                        actionContext.State.SetValue(TurnPath.Activity, dialogEvent.Value);
                        activity = ObjectPath.GetPathValue<Activity>(dialogEvent, "Value");
                        break;
                    }
            }

            EnsureDependenciesInstalled();

            // Count of events processed
            var count = actionContext.State.GetValue<uint>(DialogPath.EventCounter);
            actionContext.State.SetValue(DialogPath.EventCounter, ++count);

            // Look for triggered evt
            var handled = await QueueFirstMatchAsync(actionContext, dialogEvent, cancellationToken).ConfigureAwait(false);

            if (handled)
            {
                return true;
            }

            // Default processing
            if (preBubble)
            {
                switch (dialogEvent.Name)
                {
                    case AdaptiveEvents.BeginDialog:
                        if (actionContext.State.GetBoolValue(TurnPath.ActivityProcessed) == false)
                        {
                            // Emit leading ActivityReceived event
                            var activityReceivedEvent = new DialogEvent()
                            {
                                Name = AdaptiveEvents.ActivityReceived,
                                Value = actionContext.Context.Activity,
                                Bubble = false
                            };

                            handled = await ProcessEventAsync(actionContext, dialogEvent: activityReceivedEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case AdaptiveEvents.ActivityReceived:
                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance (ignore handled)
                            var recognizeUtteranceEvent = new DialogEvent
                            {
                                Name = AdaptiveEvents.RecognizeUtterance,
                                Value = activity,
                                Bubble = false
                            };
                            await ProcessEventAsync(actionContext, dialogEvent: recognizeUtteranceEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                            // Emit leading RecognizedIntent event
                            var recognized = actionContext.State.GetValue<RecognizerResult>(TurnPath.Recognized);
                            var recognizedIntentEvent = new DialogEvent
                            {
                                Name = AdaptiveEvents.RecognizedIntent,
                                Value = recognized,
                                Bubble = false
                            };
                            handled = await ProcessEventAsync(actionContext, dialogEvent: recognizedIntentEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        // Has an interruption occured?
                        // - Setting this value to true causes any running inputs to re-prompt when they're
                        //   continued.  The developer can clear this flag if they want the input to instead
                        //   process the users uterrance when its continued.
                        if (handled)
                        {
                            actionContext.State.SetValue(TurnPath.Interrupted, true);
                        }

                        break;

                    case AdaptiveEvents.RecognizeUtterance:
                        {
                            if (activity.Type == ActivityTypes.Message)
                            {
                                // Recognize utterance
                                var recognizedResult = await OnRecognizeAsync(actionContext, activity, cancellationToken).ConfigureAwait(false);

                                // TODO figure out way to not use turn state to pass this value back to caller.
                                actionContext.State.SetValue(TurnPath.Recognized, recognizedResult);

                                // Bug #3572 set these here, because if allowedInterruption is true then event is not emitted, but folks still want the value.
                                var (name, score) = recognizedResult.GetTopScoringIntent();
                                actionContext.State.SetValue(TurnPath.TopIntent, name);
                                actionContext.State.SetValue(TurnPath.TopScore, score);
                                actionContext.State.SetValue(DialogPath.LastIntent, name);

                                if (Recognizer != null)
                                {
                                    await actionContext.DebuggerStepAsync(Recognizer, AdaptiveEvents.RecognizeUtterance, cancellationToken).ConfigureAwait(false);
                                }

                                handled = true;
                            }
                        }

                        break;

                    case AdaptiveEvents.RepromptDialog:
                        {
                            // AdaptiveDialogs handle new RepromptDialog as it gives access to the dialogContext.
                            await this.RepromptDialogAsync(actionContext, actionContext.ActiveDialog, cancellationToken).ConfigureAwait(false);
                            handled = true;
                        }

                        break;
                }
            }
            else
            {
                switch (dialogEvent.Name)
                {
                    case AdaptiveEvents.BeginDialog:
                        if (actionContext.State.GetBoolValue(TurnPath.ActivityProcessed) == false)
                        {
                            var activityReceivedEvent = new DialogEvent
                            {
                                Name = AdaptiveEvents.ActivityReceived,
                                Value = activity,
                                Bubble = false
                            };

                            handled = await ProcessEventAsync(actionContext, dialogEvent: activityReceivedEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case AdaptiveEvents.ActivityReceived:
                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Empty sequence?
                            if (!actionContext.Actions.Any())
                            {
                                // Emit trailing unknownIntent event
                                var unknownIntentEvent = new DialogEvent
                                {
                                    Name = AdaptiveEvents.UnknownIntent,
                                    Bubble = false
                                };
                                handled = await ProcessEventAsync(actionContext, dialogEvent: unknownIntentEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                handled = false;
                            }
                        }

                        // Has an interruption occured?
                        // - Setting this value to true causes any running inputs to re-prompt when they're
                        //   continued.  The developer can clear this flag if they want the input to instead
                        //   process the users uterrance when its continued.
                        if (handled)
                        {
                            actionContext.State.SetValue(TurnPath.Interrupted, true);
                        }

                        break;
                }
            }

            return handled;
        }

        /// <summary>
        /// Waits for pending actions to complete and moves on to <see cref="OnEndOfActions"/>.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Options used in evaluation. </param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representation of <see cref="DialogTurnResult"/>.</returns>
        protected async Task<DialogTurnResult> ContinueActionsAsync(DialogContext dc, object options, CancellationToken cancellationToken)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException("You cannot pass a cancellation token as options");
            }

            // Apply any queued up changes
            var actionContext = ToActionContext(dc);
            await actionContext.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);

            // Get a unique instance ID for the current stack entry.
            // We need to do this because things like cancellation can cause us to be removed
            // from the stack and we want to detect this so we can stop processing actions.
            var instanceId = GetUniqueInstanceId(actionContext);

            // Initialize local interruption detection
            // - Any steps containing a dialog stack after the first step indicates the action was interrupted. We
            //   want to force a re-prompt and then end the turn when we encounter an interrupted step.
            var interrupted = false;

            // Execute queued actions
            var actionDC = CreateChildContext(actionContext);
            while (actionDC != null)
            {
                // DEBUG: To debug step execution set a breakpoint on line below and add a watch 
                //        statement for actionContext.Actions.
                DialogTurnResult result;
                if (actionDC.Stack.Count == 0)
                {
                    // Start step
                    var nextAction = actionContext.Actions.First();
                    result = await actionDC.BeginDialogAsync(nextAction.DialogId, nextAction.Options, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Set interrupted flag
                    if (interrupted && !actionDC.State.TryGetValue(TurnPath.Interrupted, out _))
                    {
                        actionDC.State.SetValue(TurnPath.Interrupted, true);
                    }

                    // Continue step execution
                    result = await actionDC.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                }

                // Is the step waiting for input or were we cancelled?
                if (result.Status == DialogTurnStatus.Waiting || GetUniqueInstanceId(actionContext) != instanceId)
                {
                    return result;
                }

                // End current step
                await EndCurrentActionAsync(actionContext, cancellationToken).ConfigureAwait(false);

                if (result.Status == DialogTurnStatus.CompleteAndWait)
                {
                    // Child dialog completed, but wants us to wait for a new activity
                    result.Status = DialogTurnStatus.Waiting;
                    return result;
                }

                var parentChanges = false;
                DialogContext root = actionContext;
                var parent = actionContext.Parent;
                while (parent != null)
                {
                    var ac = parent as ActionContext;
                    if (ac != null && ac.Changes != null && ac.Changes.Count > 0)
                    {
                        parentChanges = true;
                    }

                    root = parent;
                    parent = root.Parent;
                }

                // Execute next step
                if (parentChanges)
                {
                    // Recursively call ContinueDialogAsync() to apply parent changes and continue
                    // execution.
                    return await root.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                }

                // Apply any local changes and fetch next action
                await actionContext.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);
                actionDC = CreateChildContext(actionContext);
                interrupted = true;
            }

            return await OnEndOfActionsAsync(actionContext, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// OnSetScopedServices provides ability to set scoped services for the current dialogContext.
        /// </summary>
        /// <remarks>
        /// Use dialogContext.Services.Set(object) to set a scoped object that will be inherited by all children dialogContexts.
        /// </remarks>
        /// <param name="dialogContext">dialog Context.</param>
        protected virtual void OnSetScopedServices(DialogContext dialogContext)
        {
            if (Generator != null)
            {
                dialogContext.Services.Set(this.Generator);
                Expression.Functions.Add(MissingPropertiesFunction.Name, new MissingPropertiesFunction(dialogContext));
            }
        }

        /// <summary>
        /// Removes the current most action from the given <see cref="ActionContext"/> if there are any.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects.</param>
        /// <returns>A <see cref="Task"/> representing a boolean indicator for the result.</returns>
#pragma warning disable CA1801 // Review unused parameters (we can't remove the cancellationToken parameter withoutt breaking binary compat).
        protected Task<bool> EndCurrentActionAsync(ActionContext actionContext, CancellationToken cancellationToken = default)
#pragma warning restore CA1801 // Review unused parameters
        {
            if (actionContext.Actions.Any())
            {
                actionContext.Actions.RemoveAt(0);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Awaits for completed actions to finish processing entity assignments and finishes turn.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects.</param>
        /// <returns>A <see cref="Task"/> representation of <see cref="DialogTurnResult"/>.</returns>
        protected async Task<DialogTurnResult> OnEndOfActionsAsync(ActionContext actionContext, CancellationToken cancellationToken = default)
        {
            // Is the current dialog still on the stack?
            if (actionContext.ActiveDialog != null)
            {
                // Completed actions so continue processing entity assignments
                var handled = await ProcessQueuesAsync(actionContext, cancellationToken).ConfigureAwait(false);

                if (handled)
                {
                    // Still processing assignments
                    return await ContinueActionsAsync(actionContext, null, cancellationToken).ConfigureAwait(false);
                }
                else if (this.AutoEndDialog.GetValue(actionContext.State))
                {
                    actionContext.State.TryGetValue<object>(DefaultResultProperty, out var result);
                    return await actionContext.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                }

                return EndOfTurn;
            }

            return new DialogTurnResult(DialogTurnStatus.Cancelled);
        }

        /// <summary>
        /// Recognizes intent for current activity given the class recognizer set, if set is null no intent will be recognized.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current turn of conversation.</param>
        /// <param name="activity"><see cref="Activity"/> to recognize.</param>
        /// <param name="cancellationToken">Optional, a <see cref="CancellationToken"/> that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing a <see cref="RecognizerResult"/>.</returns>
        protected async Task<RecognizerResult> OnRecognizeAsync(ActionContext actionContext, Activity activity, CancellationToken cancellationToken = default)
        {
            if (Recognizer != null)
            {
                lock (this.recognizerSet)
                {
                    if (!this.recognizerSet.Recognizers.Any())
                    {
                        this.recognizerSet.Recognizers.Add(this.Recognizer);
                        this.recognizerSet.Recognizers.Add(new ValueRecognizer());
                    }
                }

                var result = await recognizerSet.RecognizeAsync(actionContext, activity, cancellationToken).ConfigureAwait(false);

                if (result.Intents.Any())
                {
                    // just deal with topIntent
                    IntentScore topScore = null;
                    var topIntent = string.Empty;
                    foreach (var intent in result.Intents)
                    {
                        if (topScore == null || topScore.Score < intent.Value.Score)
                        {
                            topIntent = intent.Key;
                            topScore = intent.Value;
                        }
                    }

                    result.Intents.Clear();
                    result.Intents.Add(topIntent, topScore);
                }
                else
                {
                    result.Intents.Add(NoneIntentKey, new IntentScore { Score = 0.0 });
                }

                return result;
            }

            // none intent if there is no recognizer
            return new RecognizerResult
            {
                Text = activity.Text ?? string.Empty,
                Intents = new Dictionary<string, IntentScore> { { NoneIntentKey, new IntentScore { Score = 0.0 } } },
            };
        }

        /// <summary>
        /// Ensures all dependencies for the class are installed.
        /// </summary>
        protected virtual void EnsureDependenciesInstalled()
        {
            if (!installedDependencies)
            {
                lock (this.syncLock)
                {
                    if (!installedDependencies)
                    {
                        installedDependencies = true;

                        var id = 0;
                        foreach (var trigger in Triggers)
                        {
                            if (trigger is IDialogDependencies depends)
                            {
                                foreach (var dlg in depends.GetDependencies())
                                {
                                    Dialogs.Add(dlg);
                                }
                            }

                            if (trigger.RunOnce)
                            {
                                needsTracker = true;
                            }

                            if (trigger.Priority == null)
                            {
                                // Constant expression defined from order
                                trigger.Priority = id;
                            }

                            if (trigger.Id == null)
                            {
                                trigger.Id = id++.ToString(CultureInfo.InvariantCulture);
                            }
                        }

                        // Wire up selector
                        if (Selector == null)
                        {
                            // Default to most specific then first
                            Selector = new MostSpecificSelector { Selector = new FirstSelector() };
                        }

                        this.Selector.Initialize(Triggers, evaluate: true);
                    }
                }
            }
        }

        // This function goes through the entity assignments and emits events if present.
        private async Task<bool> ProcessQueuesAsync(ActionContext actionContext, CancellationToken cancellationToken)
        {
            DialogEvent evt;
            bool handled;
            var assignments = EntityAssignments.Read(actionContext);
            var nextAssignment = assignments.NextAssignment();
            if (nextAssignment != null)
            {
                object val = nextAssignment;
                if (nextAssignment.Alternative != null)
                {
                    val = nextAssignment.Alternatives.ToList();
                }

                if (nextAssignment.RaisedCount++ == 0)
                {
                    // Reset retries when new form event is first issued
                    actionContext.State.RemoveValue(DialogPath.Retries);
                }

                evt = new DialogEvent() { Name = nextAssignment.Event, Value = val, Bubble = false };
                if (nextAssignment.Event == AdaptiveEvents.AssignEntity)
                {
                    // TODO: For now, I'm going to dereference to a one-level array value.  There is a bug in the current code in the distinction between
                    // @ which is supposed to unwrap down to non-array and @@ which returns the whole thing. @ in the curent code works by doing [0] which
                    // is not enough.
                    var entity = nextAssignment.Value.Value;
                    if (!(entity is JArray))
                    {
                        entity = new object[] { entity };
                    }

                    actionContext.State.SetValue($"{TurnPath.Recognized}.entities.{nextAssignment.Value.Name}", entity);
                    assignments.Dequeue(actionContext);
                }

                actionContext.State.SetValue(DialogPath.LastEvent, evt.Name);
                handled = await this.ProcessEventAsync(actionContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!handled)
                {
                    // If event wasn't handled, remove it
                    if (nextAssignment != null && nextAssignment.Event != AdaptiveEvents.AssignEntity)
                    {
                        assignments.Dequeue(actionContext);
                    }

                    // See if more assignements or end of actions
                    handled = await this.ProcessQueuesAsync(actionContext, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                // Emit end of actions
                evt = new DialogEvent() { Name = AdaptiveEvents.EndOfActions, Bubble = false };
                actionContext.State.SetValue(DialogPath.LastEvent, evt.Name);
                handled = await this.ProcessEventAsync(actionContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return handled;
        }

        private string GetUniqueInstanceId(DialogContext dc)
        {
            return dc.Stack.Count > 0 ? $"{dc.Stack.Count}:{dc.ActiveDialog.Id}" : string.Empty;
        }

        private async Task<bool> QueueFirstMatchAsync(ActionContext actionContext, DialogEvent dialogEvent, CancellationToken cancellationToken)
        {
            var selection = await Selector.SelectAsync(actionContext, cancellationToken).ConfigureAwait(false);
            if (selection.Any())
            {
                var condition = selection[0];
                await actionContext.DebuggerStepAsync(condition, dialogEvent, cancellationToken).ConfigureAwait(false);
                Trace.TraceInformation($"Executing Dialog: {Id} Rule[{condition.Id}]: {condition.GetType().Name}: {condition.GetExpression()}");

                var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "Expression", condition.GetExpression().ToString() },
                    { "Kind", $"Microsoft.{condition.GetType().Name}" },
                    { "ConditionId", condition.Id },
                };
                TelemetryClient.TrackEvent("AdaptiveDialogTrigger", properties);

                var changes = await condition.ExecuteAsync(actionContext).ConfigureAwait(false);

                if (changes != null && changes.Any())
                {
                    actionContext.QueueChanges(changes[0]);
                    return true;
                }
            }

            return false;
        }

        private ActionContext ToActionContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            var state = activeDialogState[AdaptiveKey] as AdaptiveDialogState;

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[AdaptiveKey] = state;
            }

            if (state.Actions == null)
            {
                state.Actions = new List<ActionState>();
            }

            var actionContext = new ActionContext(dc.Dialogs, dc, new DialogState { DialogStack = dc.Stack }, state.Actions, changeTurnKey);
            actionContext.Parent = dc.Parent;
            return actionContext;
        }

        // Process entities to identify ambiguity and possible assigment to properties.  Broadly the steps are:
        // Normalize entities to include meta-data
        // Check to see if an entity is in response to a previous ambiguity event
        // Assign entities to possible properties
        // Merge new assignments into existing assignments of ambiguity events
        private void ProcessEntities(ActionContext actionContext, Activity activity)
        {
            if (dialogSchema != null)
            {
                if (actionContext.State.TryGetValue<string>(DialogPath.LastEvent, out var lastEvent))
                {
                    actionContext.State.RemoveValue(DialogPath.LastEvent);
                }

                var assignments = EntityAssignments.Read(actionContext);
                var entities = NormalizeEntities(actionContext);
                var utterance = activity?.AsMessageActivity()?.Text;

                // Utterance is a special entity that corresponds to the full utterance
                entities[UtteranceKey] = new List<EntityInfo>
                {
                    new EntityInfo { Priority = float.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = UtteranceKey, Score = 0.0, Type = "string", Value = utterance, Text = utterance }
                };
                var recognized = AssignEntities(actionContext, entities, assignments, lastEvent);
                var unrecognized = SplitUtterance(utterance, recognized);

                actionContext.State.SetValue(TurnPath.UnrecognizedText, unrecognized);
                actionContext.State.SetValue(TurnPath.RecognizedEntities, recognized);
                assignments.Write(actionContext);
            }
        }

        // Split an utterance into unrecognized parts of text
        private List<string> SplitUtterance(string utterance, List<EntityInfo> recognized)
        {
            var unrecognized = new List<string>();
            var current = 0;
            foreach (var entity in recognized)
            {
                if (entity.Start > current)
                {
                    unrecognized.Add(utterance.Substring(current, entity.Start - current).Trim());
                }

                current = entity.End;
            }

            if (current < utterance.Length)
            {
                unrecognized.Add(utterance.Substring(current));
            }

            return unrecognized;
        }

        // Expand object that contains entities which can be op, property or leaf entity
        private void ExpandEntityObject(
            JObject entities, string op, string property, JObject rootInstance, List<string> operations, List<string> properties, uint turn, string text, Dictionary<string, List<EntityInfo>> entityToInfo)
        {
            foreach (var token in entities)
            {
                var entityName = token.Key;
                var instances = entities[InstanceKey][entityName] as JArray;
                ExpandEntities(entityName, token.Value as JArray, instances, rootInstance, op, property, operations, properties, turn, text, entityToInfo);
            }
        }

        private string StripProperty(string name)
            => name.EndsWith(PropertyEnding, StringComparison.InvariantCulture) ? name.Substring(0, name.Length - PropertyEnding.Length) : name;

        // Expand the array of entities for a particular entity
        private void ExpandEntities(
            string name, JArray entities, JArray instances, JObject rootInstance, string op, string property, List<string> operations, List<string> properties, uint turn, string text, Dictionary<string, List<EntityInfo>> entityToInfo)
        {
            if (!name.StartsWith("$", StringComparison.InvariantCulture))
            {
                // Entities representing schema properties end in "Property" to prevent name collisions with the property itself.
                var propName = StripProperty(name);
                string entityName = null;
                var isOp = false;
                var isProperty = false;
                if (operations.Contains(name))
                {
                    op = name;
                    isOp = true;
                }
                else if (properties.Contains(propName))
                {
                    property = propName;
                    isProperty = true;
                }
                else
                {
                    entityName = name;
                }

                for (var entityIndex = 0; entityIndex < entities.Count; ++entityIndex)
                {
                    var entity = entities[entityIndex];
                    var instance = instances[entityIndex] as JObject;
                    var root = rootInstance;
                    if (root == null)
                    {
                        // Keep the root entity name and position to help with overlap
                        root = instance.DeepClone() as JObject;
                        root["type"] = $"{name}{entityIndex}";
                    }

                    if (entityName != null)
                    {
                        ExpandEntity(entityName, entity, instance, root, op, property, turn, text, entityToInfo);
                    }
                    else if (entity is JObject entityObject)
                    {
                        if (entityObject.Count == 0)
                        {
                            if (isOp)
                            {
                                // Handle operator with no children
                                ExpandEntity(op, null, instance, root, op, property, turn, text, entityToInfo);
                            }
                            else if (isProperty)
                            {
                                // Handle property with no children
                                ExpandEntity(property, null, instance, root, op, property, turn, text, entityToInfo);
                            }
                        }
                        else
                        {
                            ExpandEntityObject(entityObject, op, property, root, operations, properties, turn, text, entityToInfo);
                        }
                    }
                    else if (isOp)
                    {
                        // Handle global operator with no children in model
                        ExpandEntity(op, null, instance, root, op, property, turn, text, entityToInfo);
                    }
                }
            }
        }

        // Expand a leaf entity into EntityInfo.
        private void ExpandEntity(string name, object value, dynamic instance, dynamic rootInstance, string op, string property, uint turn, string text, Dictionary<string, List<EntityInfo>> entityToInfo)
        {
            if (instance != null && rootInstance != null)
            {
                if (!entityToInfo.TryGetValue(name, out List<EntityInfo> infos))
                {
                    infos = new List<EntityInfo>();
                    entityToInfo[name] = infos;
                }

                var info = new EntityInfo
                {
                    WhenRecognized = turn,
                    Name = name,
                    Value = value,
                    Operation = op,
                    Property = property,
                    Start = (int)rootInstance.startIndex,
                    End = (int)rootInstance.endIndex,
                    RootEntity = rootInstance.type,
                    Text = (string)(rootInstance.text ?? string.Empty),
                    Type = (string)(instance.type ?? null),
                    Score = (double)(instance.score ?? 0.0d),
                    Priority = 0,
                };

                info.Coverage = (info.End - info.Start) / (double)text.Length;
                infos.Add(info);
            }
        }

        // Combine entity values and $instance meta-data and expand out op/property
        // Structure of entities.  
        //{
        //  "<op>": [
        //    // Op property
        //    {
        //      "<property>": [
        //        // Property without entities
        //        {},
        //        // Property with entities
        //        {
        //          "<entity>": [],
        //          "$instance": []
        //        }
        //      ],
        //      "$instance": []
        //    },
        //    // Op entity
        //    {
        //    "<entity> ": [],
        //      "$instance": []
        //    }
        //  ],
        //  // Direct property
        //  "<property>": [
        //    {},
        //    {
        //    "<entity>": [],
        //      "$instance": []
        //    }
        //  ],
        //  // Direct entity
        //  "<entity>": [],
        //  "$instance": []
        //}
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(ActionContext actionContext)
        {
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();
            var text = actionContext.State.GetValue<string>(TurnPath.Recognized + ".text");
            if (actionContext.State.TryGetValue<dynamic>(TurnPath.Recognized + ".entities", out var entities))
            {
                var turn = actionContext.State.GetValue<uint>(DialogPath.EventCounter);
                var operations = dialogSchema.Schema[OperationsKey]?.ToObject<List<string>>() ?? new List<string>();
                var properties = dialogSchema.Property.Children.Select((prop) => prop.Name).ToList<string>();
                ExpandEntityObject(entities, null, null, null, operations, properties, turn, text, entityToInfo);
            }

            // When there are multiple possible resolutions for the same entity that overlap, pick the one that covers the
            // most of the utterance.
            foreach (var infos in entityToInfo.Values)
            {
                infos.Sort((entity1, entity2) =>
                {
                    var val = 0;
                    if (entity1.Start == entity2.Start)
                    {
                        if (entity1.End > entity2.End)
                        {
                            val = -1;
                        }
                        else if (entity1.End < entity2.End)
                        {
                            val = +1;
                        }
                    }
                    else if (entity1.Start < entity2.Start)
                    {
                        val = -1;
                    }
                    else
                    {
                        val = +1;
                    }

                    return val;
                });
                for (var i = 0; i < infos.Count; ++i)
                {
                    var current = infos[i];
                    for (var j = i + 1; j < infos.Count;)
                    {
                        var alt = infos[j];
                        if (current.Covers(alt))
                        {
                            _ = infos.Remove(alt);
                        }
                        else
                        {
                            ++j;
                        }
                    }
                }
            }

            return entityToInfo;
        }

        // An entity matches an assignment if the detected operation/property match
        private bool MatchesAssignment(EntityInfo entity, EntityAssignment assignment)
         => (entity.Operation == null || entity.Operation == assignment.Operation)
            && (entity.Property == null || entity.Property == assignment.Property);

        // Generate candidate assignments including property and operation
        private IEnumerable<EntityAssignment> Candidates(Dictionary<string, List<EntityInfo>> entities, string[] expected, string lastEvent, EntityAssignment nextAssignment, JObject askDefault, JObject dialogDefault)
        {
            var globalExpectedOnly = dialogSchema.Schema[ExpectedOnlyKey]?.ToObject<List<string>>() ?? new List<string>();
            var requiresValue = dialogSchema.Schema[RequiresValueKey]?.ToObject<List<string>>() ?? new List<string>();
            var assignments = new List<EntityAssignment>();

            // Add entities with a recognized property
            foreach (var alternatives in entities.Values)
            {
                foreach (var alternative in alternatives)
                {
                    if (alternative.Property != null && (alternative.Value != null || !requiresValue.Contains(alternative.Operation)))
                    {
                        assignments.Add(new EntityAssignment
                        {
                            Value = alternative,
                            Property = alternative.Property,
                            Operation = alternative.Operation,
                            IsExpected = expected.Contains(alternative.Property)
                        });
                    }
                }
            }

            // Find possible mappings for entities without a property or where property entities are expected
            foreach (var propSchema in dialogSchema.Property.Children)
            {
                var isExpected = expected.Contains(propSchema.Name);
                var expectedOnly = propSchema.ExpectedOnly ?? globalExpectedOnly;
                foreach (var propEntity in propSchema.Entities)
                {
                    var entityName = StripProperty(propEntity);
                    if (entities.TryGetValue(entityName, out var matches) && (isExpected || !expectedOnly.Contains(entityName)))
                    {
                        foreach (var entity in matches)
                        {
                            if (entity.Property == null)
                            {
                                assignments.Add(new EntityAssignment
                                {
                                    Value = entity,
                                    Property = propSchema.Name,
                                    Operation = entity.Operation,
                                    IsExpected = isExpected
                                });
                            }
                            else if (entity.Property == entityName && entity.Value == null && entity.Operation == null && isExpected)
                            {
                                // Recast property with no value as match for property entities
                                assignments.Add(new EntityAssignment
                                {
                                    Value = entity,
                                    Property = propSchema.Name,
                                    Operation = null,
                                    IsExpected = isExpected,
                                });
                            }
                        }
                    }
                }
            }

            // Add default operations
            foreach (var assignment in assignments)
            {
                if (assignment.Operation == null)
                {
                    // Assign missing operation
                    if (lastEvent == AdaptiveEvents.ChooseEntity
                        && assignment.Value.Property == nextAssignment.Property)
                    {
                        // Property and value match ambiguous entity
                        assignment.Operation = AdaptiveEvents.ChooseEntity;
                        assignment.IsExpected = true;
                    }
                    else
                    {
                        // Assign default operator
                        assignment.Operation = DefaultOperation(assignment, askDefault, dialogDefault);
                    }
                }
            }

            // Add choose property matches
            if (lastEvent == AdaptiveEvents.ChooseProperty)
            {
                foreach (var alternatives in entities.Values)
                {
                    foreach (var alternative in alternatives)
                    {
                        if (alternative.Value == null)
                        {
                            // If alternative matches one alternative it answers chooseProperty
                            var matches = nextAssignment.Alternatives.Where(a => MatchesAssignment(alternative, a));
                            if (matches.Count() == 1)
                            {
                                assignments.Add(new EntityAssignment
                                {
                                    Value = alternative,
                                    Operation = AdaptiveEvents.ChooseProperty,
                                    IsExpected = true
                                });
                            }
                        }
                    }
                }
            }

            // Add pure operations
            foreach (var alternatives in entities.Values)
            {
                foreach (var alternative in alternatives)
                {
                    if (alternative.Operation != null && alternative.Property == null && alternative.Value == null)
                    {
                        var assignment = new EntityAssignment
                        {
                            Value = alternative,
                            Property = null,
                            Operation = alternative.Operation,
                            IsExpected = false
                        };
                        assignments.Add(assignment);
                    }
                }
            }

            // Preserve expectedProperties if there is no property
            foreach (var assignment in assignments)
            {
                if (assignment.Property == null)
                {
                    assignment.ExpectedProperties = expected.ToList();
                }
            }

            return assignments;
        }

        private void AddAssignment(EntityAssignment assignment, EntityAssignments assignments)
        {
            // Entities without a property or operation are available as entities only when found
            if (assignment.Property != null || assignment.Operation != null)
            {
                if (assignment.Alternative != null)
                {
                    assignment.Event = AdaptiveEvents.ChooseProperty;
                }
                else if (assignment.Value.Value is JArray arr)
                {
                    if (arr.Count > 1)
                    {
                        assignment.Event = AdaptiveEvents.ChooseEntity;
                    }
                    else
                    {
                        assignment.Event = AdaptiveEvents.AssignEntity;
                        assignment.Value.Value = arr[0];
                    }
                }
                else
                {
                    assignment.Event = AdaptiveEvents.AssignEntity;
                }

                assignments.Assignments.Add(assignment);
            }
        }

        // Have each property pick which overlapping entity is the best one
        // This can happen because LUIS will return both 'wheat' and 'whole wheat' as the same list entity.
        private IEnumerable<EntityAssignment> RemoveOverlappingPerProperty(IEnumerable<EntityAssignment> candidates)
        {
            var perProperty = from candidate in candidates
                              group candidate by candidate.Property;
            foreach (var propChoices in perProperty)
            {
                var entityPreferences = dialogSchema.PathToSchema(propChoices.Key).Entities;
                var choices = propChoices.ToList();

                // Assume preference by order listed in mappings
                // Alternatives would be to look at coverage or other metrics
                foreach (var entity in entityPreferences)
                {
                    EntityAssignment candidate;
                    do
                    {
                        candidate = null;
                        foreach (var mapping in choices)
                        {
                            if (mapping.Value.Name == entity)
                            {
                                candidate = mapping;
                                break;
                            }
                        }

                        if (candidate != null)
                        {
                            // Remove any overlapping entities without a common root
                            choices.RemoveAll(choice => choice == candidate || (!choice.Value.SharesRoot(candidate.Value) && choice.Value.Overlaps(candidate.Value)));
                            yield return candidate;
                        }
                    }
                    while (candidate != null);
                }

                // Keep remaining properties for things like show/clear that are not property specific
                foreach (var choice in choices)
                {
                    yield return choice;
                }
            }
        }

        // Return the default operation for an assignment by looking at the per-ask and dialog defaults
        private string DefaultOperation(EntityAssignment assignment, JObject askDefault, JObject dialogDefault)
        {
            string operation = null;
            if (assignment.Property != null)
            {
                if (askDefault != null && (askDefault.TryGetValue(assignment.Value.Name, out var askOp) || askDefault.TryGetValue(string.Empty, out askOp)))
                {
                    operation = askOp.Value<string>();
                }
                else if (dialogDefault != null
                        && (dialogDefault.TryGetValue(assignment.Property, out var entities)
                            || dialogDefault.TryGetValue(string.Empty, out entities))
                        && ((entities as JObject).TryGetValue(assignment.Value.Name, out var dialogOp)
                            || (entities as JObject).TryGetValue(string.Empty, out dialogOp)))
                {
                    operation = dialogOp.Value<string>();
                }
            }

            return operation;
        }

        // Choose between competing interpretations
        // This works by:
        // * Generate candidate assignments including inferred property and operator if missing
        // * Order by expected, then default operation to prefer expected things
        // * Pick a candidate, identify alternatives and remove from pool of candidates
        // * Alternatives overlap and are filtered by non-default op and biggest interpretation containing alternative 
        // * The new assignments are then ordered by recency and phrase order and merged with existing assignments
        private List<EntityInfo> AssignEntities(ActionContext actionContext, Dictionary<string, List<EntityInfo>> entities, EntityAssignments existing, string lastEvent)
        {
            var assignments = new EntityAssignments();
            if (!actionContext.State.TryGetValue<string[]>(DialogPath.ExpectedProperties, out var expected))
            {
                expected = Array.Empty<string>();
            }

            // default op from the last Ask action.
            var askDefaultOp = actionContext.State.GetValue<JObject>(DialogPath.DefaultOperation);

            // default operation from the current adaptive dialog.
            var defaultOp = dialogSchema.Schema[DefaultOperationKey]?.ToObject<JObject>();

            var nextAssignment = existing.NextAssignment();
            var candidates = (from candidate in RemoveOverlappingPerProperty(Candidates(entities, expected, lastEvent, nextAssignment, askDefaultOp, defaultOp))
                              orderby
                                candidate.IsExpected descending,
                                candidate.Operation == DefaultOperation(candidate, askDefaultOp, defaultOp) descending
                              select candidate).ToList();
            var usedEntities = new HashSet<EntityInfo>(from candidate in candidates select candidate.Value);
            List<string> expectedChoices = null;
            var choices = new List<EntityAssignment>();
            while (candidates.Any())
            {
                var candidate = candidates.First();

                // Alternatives are either for the same entity or from different roots
                var alternatives = (from alt in candidates
                                    where candidate.Value.Overlaps(alt.Value) && (!candidate.Value.SharesRoot(alt.Value) || candidate.Value == alt.Value)
                                    select alt).ToList();
                candidates = candidates.Except(alternatives).ToList();
                foreach (var alternative in alternatives)
                {
                    usedEntities.Add(alternative.Value);
                }

                if (candidate.IsExpected && candidate.Value.Name != UtteranceKey)
                {
                    // If expected binds entity, drop unexpected alternatives unless they have an explicit operation
                    alternatives.RemoveAll(a => !a.IsExpected && a.Value.Operation == null);
                }

                // Find alternative that covers the largest amount of utterance
                candidate = (from alternative in alternatives orderby alternative.Value.Name == UtteranceKey ? 0 : alternative.Value.End - alternative.Value.Start descending select alternative).First();

                // Remove all alternatives that are fully contained in largest
                alternatives.RemoveAll(a => candidate.Value.Covers(a.Value));

                var mapped = false;
                if (candidate.Operation == AdaptiveEvents.ChooseEntity)
                {
                    // Property has resolution so remove entity ambiguity
                    var entityChoices = existing.Dequeue(actionContext);
                    candidate.Operation = entityChoices.Operation;
                    if (candidate.Value.Value is JArray values && values.Count > 1)
                    {
                        // Resolve ambiguous response to one of the original choices
                        var originalChoices = entityChoices.Value.Value as JArray;
                        var intersection = values.Intersect(originalChoices);
                        if (intersection.Any())
                        {
                            candidate.Value.Value = intersection;
                        }
                    }
                }
                else if (candidate.Operation == AdaptiveEvents.ChooseProperty)
                {
                    choices = nextAssignment.Alternatives.ToList();
                    var choice = choices.Find(a => MatchesAssignment(candidate.Value, a));
                    if (choice != null)
                    {
                        // Resolve choice, pretend it was expected and add to assignments
                        expectedChoices = new List<string>();
                        choice.IsExpected = true;
                        choice.Alternative = null;
                        if (choice.Property != null)
                        {
                            expectedChoices.Add(choice.Property);
                        }
                        else if (choice.ExpectedProperties != null)
                        {
                            expectedChoices.AddRange(choice.ExpectedProperties);
                        }

                        AddAssignment(choice, assignments);
                        choices.RemoveAll(c => c.Value.Overlaps(choice.Value));
                        mapped = true;
                    }
                }

                candidate.AddAlternatives(alternatives);
                if (!mapped)
                {
                    AddAssignment(candidate, assignments);
                }
            }

            if (expectedChoices != null)
            {
                // When choosing between property assignments, make the assignments be expected.
                if (expectedChoices.Any())
                {
                    actionContext.State.SetValue(DialogPath.ExpectedProperties, expectedChoices);
                }

                // Add back in any non-overlapping choices that have not been resolved
                while (choices.Any())
                {
                    var choice = choices.First();
                    var overlaps = from alt in choices where choice.Value.Overlaps(alt.Value) select alt;
                    choice.AddAlternatives(overlaps);
                    AddAssignment(choice, assignments);
                    choices.RemoveAll(c => c.Value.Overlaps(choice.Value));
                }

                existing.Dequeue(actionContext);
            }

            var operations = new EntityAssignmentComparer(dialogSchema.Schema[OperationsKey]?.ToObject<string[]>() ?? Array.Empty<string>());
            MergeAssignments(assignments, existing, operations);
            return usedEntities.ToList();
        }

        // a replaces b when it refers to the same singleton property and is newer or later in same utterance and it is not a bare property
        // -1 a replaces b
        //  0 no replacement
        // +1 b replaces a
        private int Replaces(EntityAssignment a, EntityAssignment b)
        {
            var replaces = 0;
            foreach (var aAlt in a.Alternatives)
            {
                foreach (var bAlt in b.Alternatives)
                {
                    if (aAlt.Property == bAlt.Property && aAlt.Value.Value != null && bAlt.Value.Value != null)
                    {
                        var prop = dialogSchema.PathToSchema(aAlt.Property);
                        if (!prop.IsArray)
                        {
                            replaces = -aAlt.Value.WhenRecognized.CompareTo(bAlt.Value.WhenRecognized);
                            if (replaces == 0)
                            {
                                replaces = -aAlt.Value.Start.CompareTo(bAlt.Value.Start);
                            }

                            if (replaces != 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return replaces;
        }

        // Merge new assignments into old so there is only one operation per singleton
        // and we prefer newer assignments.
        private void MergeAssignments(EntityAssignments newAssignments, EntityAssignments old, EntityAssignmentComparer comparer)
        {
            var list = old.Assignments;
            foreach (var assign in newAssignments.Assignments)
            {
                // Only one outstanding operation per singleton property
                var add = true;
                var newList = new List<EntityAssignment>();
                foreach (var oldAssign in list)
                {
                    var keep = true;
                    if (add)
                    {
                        switch (Replaces(assign, oldAssign))
                        {
                            case -1: keep = false; break;
                            case +1: add = false; break;
                        }
                    }

                    if (keep)
                    {
                        newList.Add(oldAssign);
                    }
                }

                if (add)
                {
                    newList.Add(assign);
                }

                list = newList;
            }

            old.Assignments = list;
            list.Sort(comparer);
        }
    }
}
