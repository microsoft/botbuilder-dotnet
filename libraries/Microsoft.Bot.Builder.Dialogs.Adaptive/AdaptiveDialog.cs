// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Debugging;
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
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.AdaptiveDialog";

        internal const string ConditionTracker = "dialog._tracker.conditions";

        private const string AdaptiveKey = "_adaptive";

        // unique key for language generator turn property, (TURN STATE ONLY)
        private readonly string generatorTurnKey = Guid.NewGuid().ToString();

        // unique key for change tracking of the turn state (TURN STATE ONLY)
        private readonly string changeTurnKey = Guid.NewGuid().ToString();

        private RecognizerSet recognizerSet = new RecognizerSet();

        private bool installedDependencies;

        private bool needsTracker = false;

        private SchemaHelper dialogSchema;

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
        public virtual List<OnCondition> Triggers { get; set; } = new List<OnCondition>();

        /// <summary>
        /// Gets or sets a value indicating whether to end the dialog when there are no actions to execute.
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
        public bool AutoEndDialog { get; set; } = true;

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
        public JObject Schema
        {
            get => dialogSchema?.Schema;
            set
            {
                dialogSchema = new SchemaHelper(value);
            }
        }

        [JsonIgnore]
        public override IBotTelemetryClient TelemetryClient
        {
            get
            {
                return base.TelemetryClient;
            }

            set
            {
                base.TelemetryClient = value ?? NullBotTelemetryClient.Instance;
                Dialogs.TelemetryClient = base.TelemetryClient;
            }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} should not ever be a cancellation token");
            }

            EnsureDependenciesInstalled();

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

            // replace initial activeDialog.State with clone of options
            if (options != null)
            {
                dc.ActiveDialog.State = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(options));
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

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            EnsureDependenciesInstalled();

            // Continue step execution
            return await ContinueActionsAsync(dc, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // resumeDialog() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog).ConfigureAwait(false);

            return EndOfTurn;
        }

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

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default)
        {
            try
            {
                // This is to handle corner case of reprompt occuring outside of normal flow (such as interruption being resumed)
                OnPushScopedServices(turnContext);

                // Forward to current sequence step
                var state = (instance.State as Dictionary<string, object>)[AdaptiveKey] as AdaptiveDialogState;

                if (state.Actions.Any())
                {
                    // We need to mockup a DialogContext so that we can call RepromptDialog
                    // for the active step
                    var stepDc = new DialogContext(this.Dialogs, turnContext, state.Actions[0]);
                    await stepDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                OnPopScopedServices(turnContext);
            }
        }

        public override DialogContext CreateChildContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            var state = activeDialogState[AdaptiveKey] as AdaptiveDialogState;

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[AdaptiveKey] = state;
            }

            if (state.Actions != null && state.Actions.Any())
            {
                return new DialogContext(this.Dialogs, dc, state.Actions.First());
            }

            return null;
        }

        public IEnumerable<Dialog> GetDependencies()
        {
            EnsureDependenciesInstalled();

            yield break;
        }

        protected override async Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default)
        {
            var actionContext = ToActionContext(dc);

            // Process event and queue up any potential interruptions
            return await ProcessEventAsync(actionContext, dialogEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<bool> OnPostBubbleEventAsync(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default)
        {
            var actionContext = ToActionContext(dc);

            // Process event and queue up any potential interruptions
            return await ProcessEventAsync(actionContext, dialogEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

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
            var handled = await QueueFirstMatchAsync(actionContext, dialogEvent, preBubble, cancellationToken).ConfigureAwait(false);

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
                                var recognizedResult = await OnRecognize(actionContext, activity, cancellationToken).ConfigureAwait(false);

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

            try
            {
                OnPushScopedServices(dc.Context);

                // Execute queued actions
                var actionDC = CreateChildContext(actionContext);
                while (actionDC != null)
                {
                    // Continue current step
                    // DEBUG: To debug step execution set a breakpoint on line below and add a watch 
                    //        statement for actionContext.Actions.
                    var result = await actionDC.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

                    // Start step if not continued
                    if (result.Status == DialogTurnStatus.Empty && GetUniqueInstanceId(actionContext) == instanceId)
                    {
                        // Call begin dialog on our next step, passing the effective options we computed
                        var nextAction = actionContext.Actions.First();
                        result = await actionDC.BeginDialogAsync(nextAction.DialogId, nextAction.Options, cancellationToken).ConfigureAwait(false);
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
                }
            }
            finally
            {
                OnPopScopedServices(dc.Context);
            }

            return await OnEndOfActionsAsync(actionContext, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// OnPushScopedServices provides ability to Push scoped services to the turnState.
        /// </summary>
        /// <remarks>If you override this you should make sure to call the base.OnPushScopedServices().</remarks>
        /// <param name="turnContext">turnContext.</param>
        protected virtual void OnPushScopedServices(ITurnContext turnContext)
        {
            if (Generator != null)
            {
                turnContext.TurnState.Push(this.Generator);
            }
        }

        /// <summary>
        /// OnPopScopedServices provides ability to Pop scoped services from the turnState.
        /// </summary>
        /// <remarks>If you override this you should make sure to call the base.OnPopScopedServices().</remarks>
        /// <param name="turnContext">turnContext.</param>
        protected virtual void OnPopScopedServices(ITurnContext turnContext)
        {
            if (Generator != null)
            {
                turnContext.TurnState.Pop<LanguageGenerator>();
            }
        }

        protected Task<bool> EndCurrentActionAsync(ActionContext actionContext, CancellationToken cancellationToken = default)
        {
            if (actionContext.Actions.Any())
            {
                actionContext.Actions.RemoveAt(0);
            }

            return Task.FromResult(false);
        }

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
                else if (ShouldEnd(actionContext))
                {
                    actionContext.State.TryGetValue<object>(DefaultResultProperty, out var result);
                    return await actionContext.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                }

                return EndOfTurn;
            }

            return new DialogTurnResult(DialogTurnStatus.Cancelled);
        }

        protected async Task<RecognizerResult> OnRecognize(ActionContext actionContext, Activity activity, CancellationToken cancellationToken = default)
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
                    result.Intents.Add("None", new IntentScore { Score = 0.0 });
                }

                return result;
            }

            // none intent if there is no recognizer
            return new RecognizerResult
            {
                Text = activity.Text ?? string.Empty,
                Intents = new Dictionary<string, IntentScore> { { "None", new IntentScore { Score = 0.0 } } },
            };
        }

        // This function goes through the entity assignments and emits events if present.
        private async Task<bool> ProcessQueuesAsync(ActionContext actionContext, CancellationToken cancellationToken)
        {
            DialogEvent evt;
            var assignments = EntityAssignments.Read(actionContext);
            var nextAssignment = assignments.NextAssignment();
            if (nextAssignment != null)
            {
                object val = nextAssignment;
                if (nextAssignment.Alternative != null)
                {
                    val = nextAssignment.Alternatives.ToList();
                }

                evt = new DialogEvent() { Name = nextAssignment.Event, Value = val, Bubble = false };
                if (nextAssignment.Event == AdaptiveEvents.AssignEntity)
                {
                    // TODO: For now, I'm going to dereference to a one-level array value.  There is a bug in the current code in the distinction between
                    // @ which is supposed to unwrap down to non-array and @@ which returns the whole thing. @ in the curent code works by doing [0] which
                    // is not enough.
                    var entity = nextAssignment.Entity.Value;
                    if (!(entity is JArray))
                    {
                        entity = new object[] { entity };
                    }

                    actionContext.State.SetValue($"{TurnPath.Recognized}.entities.{nextAssignment.Entity.Name}", entity);
                    assignments.Dequeue(actionContext);
                }
            }
            else
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.EndOfActions, Bubble = false };
            }

            actionContext.State.SetValue(DialogPath.LastEvent, evt.Name);
            var handled = await this.ProcessEventAsync(actionContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!handled)
            {
                // If event wasn't handled, remove it from assignments and keep going if things changed
                if (nextAssignment != null && nextAssignment.Event != AdaptiveEvents.AssignEntity)
                {
                    assignments.Dequeue(actionContext);
                    handled = await this.ProcessQueuesAsync(actionContext, cancellationToken);
                }
            }

            return handled;
        }

        private string GetUniqueInstanceId(DialogContext dc)
        {
            return dc.Stack.Count > 0 ? $"{dc.Stack.Count}:{dc.ActiveDialog.Id}" : string.Empty;
        }

        private async Task<bool> QueueFirstMatchAsync(ActionContext actionContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken)
        {
            var selection = await Selector.Select(actionContext, cancellationToken).ConfigureAwait(false);
            if (selection.Any())
            {
                var evt = selection.First();
                await actionContext.DebuggerStepAsync(evt, dialogEvent, cancellationToken).ConfigureAwait(false);
                Trace.TraceInformation($"Executing Dialog: {Id} Rule[{evt.Id}]: {evt.GetType().Name}: {evt.GetExpression()}");

                var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "Expression", evt.GetExpression().ToString() },
                    { "Kind", $"Microsoft.{evt.GetType().Name}" },
                    { "Instance", JsonConvert.SerializeObject(evt, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }).ToString() }
                };
                TelemetryClient.TrackEvent("AdaptiveDialogTrigger", properties);

                var changes = await evt.ExecuteAsync(actionContext).ConfigureAwait(false);

                if (changes != null && changes.Any())
                {
                    actionContext.QueueChanges(changes[0]);
                    return true;
                }
            }

            return false;
        }

        private void EnsureDependenciesInstalled()
        {
            lock (this)
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
                            trigger.Id = id++.ToString();
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

        private bool ShouldEnd(DialogContext dc)
        {
            return AutoEndDialog;
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
                entities["utterance"] = new List<EntityInfo> { new EntityInfo { Priority = int.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = "utterance", Score = 0.0, Type = "string", Value = utterance, Text = utterance } };
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

        // Combine entity values and $instance meta-data
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(ActionContext actionContext)
        {
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();
            var text = actionContext.State.GetValue<string>(TurnPath.Recognized + ".text");
            if (actionContext.State.TryGetValue<dynamic>(TurnPath.Recognized + ".entities", out var entities))
            {
                var turn = actionContext.State.GetValue<uint>(DialogPath.EventCounter);
                var operations = dialogSchema.Schema["$operations"]?.ToObject<List<string>>() ?? new List<string>();
                var metaData = entities["$instance"];
                foreach (var entry in entities)
                {
                    var name = entry.Name;
                    if (operations.Contains(name))
                    {
                        for (var i = 0; i < entry.Value.Count; ++i)
                        {
                            var composite = entry.Value[i];
                            var childInstance = composite["$instance"];
                            foreach (var child in composite)
                            {
                                ExpandEntity(child, childInstance, name, turn, text, entityToInfo);
                            }
                        }
                    }
                    else
                    {
                        ExpandEntity(entry, metaData, null, turn, text, entityToInfo);
                    }
                }
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
                for (var i = 0; i < infos.Count(); ++i)
                {
                    var current = infos[i];
                    for (var j = i + 1; j < infos.Count();)
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

        private void ExpandEntity(dynamic entry, dynamic metaData, string op, uint turn, string text, Dictionary<string, List<EntityInfo>> entityToInfo)
        {
            var name = entry.Name;
            if (!name.StartsWith("$"))
            {
                var values = entry.Value;
                var instances = metaData?[name];
                for (var i = 0; i < values.Count; ++i)
                {
                    var val = values[i];
                    var instance = instances?[i];
                    if (!entityToInfo.TryGetValue(name, out List<EntityInfo> infos))
                    {
                        infos = new List<EntityInfo>();
                        entityToInfo[name] = infos;
                    }

                    var info = new EntityInfo
                    {
                        WhenRecognized = turn,
                        Name = name,
                        Value = val,
                        Operation = op
                    };
                    if (instance != null)
                    {
                        info.Start = (int)instance.startIndex;
                        info.End = (int)instance.endIndex;
                        info.Text = (string)(instance.text ?? string.Empty);
                        info.Type = (string)(instance.type ?? null);
                        info.Role = (string)(instance.role ?? null);
                        info.Score = (double)(instance.score ?? 0.0d);
                    }

                    // Eventually this could be passed in
                    info.Priority = info.Role == null ? 1 : 0;
                    info.Coverage = (info.End - info.Start) / (double)text.Length;
                    infos.Add(info);
                }
            }
        }

        // Generate possible entity to property mappings
        private IEnumerable<EntityAssignment> Candidates(Dictionary<string, List<EntityInfo>> entities, string[] expected)
        {
            var globalExpectedOnly = dialogSchema.Schema["$expectedOnly"]?.ToObject<List<string>>() ?? new List<string>();
            var used = new HashSet<string> { "utterance" };
            foreach (var propSchema in dialogSchema.Property.Children)
            {
                var isExpected = expected.Contains(propSchema.Name);
                var expectedOnly = propSchema.ExpectedOnly ?? globalExpectedOnly;
                foreach (var entityName in propSchema.Entities)
                {
                    if (entities.TryGetValue(entityName, out var matches) && (isExpected || !expectedOnly.Contains(entityName)))
                    {
                        used.Add(entityName);
                        foreach (var entity in matches)
                        {
                            yield return new EntityAssignment
                            {
                                Entity = entity,
                                Property = propSchema.Name,
                                Operation = entity.Operation,
                                IsExpected = isExpected
                            };
                        }
                    }
                }
            }

            // Unassigned entities
            var entityPreferences = EntityPreferences(null);
            foreach (var entry in entities)
            {
                if (!used.Contains(entry.Key) && entityPreferences.Contains(entry.Key))
                {
                    foreach (var entity in entry.Value)
                    {
                        yield return new EntityAssignment
                        {
                            Entity = entity,
                            Operation = entity.Operation
                        };
                    }
                }
            }
        }

        private void AddMapping(EntityAssignment mapping, EntityAssignments assignments)
        {
            // Entities without a property or operation are available as entities only when found
            if (mapping.Property != null || mapping.Operation != null)
            {
                if (mapping.Alternative != null)
                {
                    mapping.Event = AdaptiveEvents.ChooseProperty;
                }
                else if (mapping.Entity.Value is JArray arr)
                {
                    if (arr.Count > 1)
                    {
                        mapping.Event = AdaptiveEvents.ChooseEntity;
                    }
                    else
                    {
                        mapping.Event = AdaptiveEvents.AssignEntity;
                        mapping.Entity.Value = arr[0];
                    }
                }
                else
                {
                    mapping.Event = AdaptiveEvents.AssignEntity;
                }

                assignments.Assignments.Add(mapping);
            }
        }

        // Remove any entities that overlap a selected entity
        private void RemoveOverlappingEntities(EntityInfo entity, Dictionary<string, List<EntityInfo>> entities)
        {
            foreach (var infos in entities.Values)
            {
                infos.RemoveAll(e => e.Overlaps(entity));
            }
        }

        private IReadOnlyList<string> EntityPreferences(string property)
        {
            IReadOnlyList<string> result;
            if (property == null)
            {
                if (dialogSchema.Schema.ContainsKey("$entities"))
                {
                    result = dialogSchema.Schema["$entities"].ToObject<List<string>>();
                }
                else
                {
                    result = new List<string> { "PROPERTYName" };
                }
            }
            else
            {
                result = dialogSchema.PathToSchema(property).Entities;
            }

            return result;
        }

        // Have each property pick which overlapping entity is the best one
        private IEnumerable<EntityAssignment> RemoveOverlappingPerProperty(IEnumerable<EntityAssignment> candidates)
        {
            var perProperty = from candidate in candidates
                              group candidate by candidate.Property;
            foreach (var propChoices in perProperty)
            {
                var entityPreferences = EntityPreferences(propChoices.Key);
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
                            if (mapping.Entity.Name == entity)
                            {
                                candidate = mapping;
                                break;
                            }
                        }

                        if (candidate != null)
                        {
                            // Remove any overlapping entities
                            choices.RemoveAll(choice => choice.Entity.Overlaps(candidate.Entity));
                            yield return candidate;
                        }
                    }
                    while (candidate != null);
                }
            }
        }

        private List<EntityInfo> AssignEntities(ActionContext actionContext, Dictionary<string, List<EntityInfo>> entities, EntityAssignments existing, string lastEvent)
        {
            var assignments = new EntityAssignments();
            if (!actionContext.State.TryGetValue<string[]>(DialogPath.ExpectedProperties, out var expected))
            {
                expected = new string[0];
            }

            // default op from the last Ask action.
            var askDefaultOp = actionContext.State.GetValue<string>(DialogPath.DefaultOperation);

            // default operation from the current adaptive dialog.
            var defaultOp = dialogSchema.Schema["$defaultOperation"]?.ToObject<string>();

            var nextAssignment = existing.NextAssignment();
            var candidates = (from candidate in RemoveOverlappingPerProperty(Candidates(entities, expected))
                              orderby candidate.IsExpected descending
                              select candidate).ToList();
            var usedEntities = new HashSet<EntityInfo>(from candidate in candidates select candidate.Entity);
            var expectedChoices = new List<string>();
            var choices = new List<EntityAssignment>();
            while (candidates.Any())
            {
                var candidate = candidates.First();
                var alternatives = (from alt in candidates where candidate.Entity.Overlaps(alt.Entity) select alt).ToList();
                candidates = candidates.Except(alternatives).ToList();
                if (candidate.IsExpected && candidate.Entity.Name != "utterance")
                {
                    // If expected binds entity, drop alternatives
                    alternatives.RemoveAll(a => !a.IsExpected);
                }

                var mapped = false;
                if (lastEvent == AdaptiveEvents.ChooseEntity && candidate.Property == nextAssignment.Property)
                {
                    // Property has possible resolution so remove entity ambiguity
                    existing.Dequeue(actionContext);
                    lastEvent = null;
                }
                else if (lastEvent == AdaptiveEvents.ChooseProperty && candidate.Operation == null && candidate.Entity.Name == "PROPERTYName")
                {
                    // NOTE: This assumes the existence of an entity named PROPERTYName for resolving this ambiguity
                    choices = existing.NextAssignment().Alternatives.ToList();
                    var property = (candidate.Entity.Value as JArray)?[0]?.ToObject<string>();
                    var choice = choices.Find(p => p.Property == property);
                    if (choice != null)
                    {
                        // Resolve choice, pretend it was expected and add to assignments
                        choice.IsExpected = true;
                        choice.Alternative = null;
                        expectedChoices.Add(choice.Property);
                        AddMapping(choice, assignments);
                        choices.RemoveAll(c => c.Entity.Overlaps(choice.Entity));
                        mapped = true;
                    }
                }

                foreach (var alternative in alternatives)
                {
                    if (alternative.Operation == null)
                    {
                        alternative.Operation = alternative.IsExpected ? (askDefaultOp ?? defaultOp) : defaultOp;
                    }

                    usedEntities.Add(alternative.Entity);
                }

                candidate.AddAlternatives(alternatives);

                if (!mapped)
                {
                    AddMapping(candidate, assignments);
                }
            }

            if (expectedChoices.Any())
            {
                // When choosing between property assignments, make the assignments be expected.
                actionContext.State.SetValue(DialogPath.ExpectedProperties, expectedChoices);

                // Add back in any non-overlapping choices
                while (choices.Any())
                {
                    var choice = choices.First();
                    var overlaps = from alt in choices where choice.Entity.Overlaps(alt.Entity) select alt;
                    choice.AddAlternatives(overlaps);
                    AddMapping(choice, assignments);
                    choices.RemoveAll(c => c.Entity.Overlaps(choice.Entity));
                }

                existing.Dequeue(actionContext);
            }

            var operations = new EntityAssignmentComparer(dialogSchema.Schema["$operations"]?.ToObject<string[]>() ?? new string[0]);
            MergeAssignments(assignments, existing, operations);
            return usedEntities.ToList();
        }

        // a replaces b when it refers to the same singleton property and is newer or later in same utterance
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
                    if (aAlt.Property == bAlt.Property)
                    {
                        var prop = dialogSchema.PathToSchema(aAlt.Property);
                        if (!prop.IsArray)
                        {
                            replaces = -aAlt.Entity.WhenRecognized.CompareTo(bAlt.Entity.WhenRecognized);
                            if (replaces == 0)
                            {
                                replaces = -aAlt.Entity.Start.CompareTo(bAlt.Entity.Start);
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
