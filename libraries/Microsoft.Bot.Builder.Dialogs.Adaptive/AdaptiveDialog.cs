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
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Expressions;
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
        public const string DeclarativeType = "Microsoft.AdaptiveDialog";

        internal const string ConditionTracker = "dialog._tracker.conditions";

        private const string AdaptiveKey = "_adaptive";

        // unique key for language generator turn property, (TURN STATE ONLY)
        private readonly string generatorTurnKey = Guid.NewGuid().ToString();

        // unique key for change tracking of the turn state (TURN STATE ONLY)
        private readonly string changeTurnKey = Guid.NewGuid().ToString();

        private bool installedDependencies;

        private bool needsTracker = false;

        private SchemaHelper dialogSchema;

        public AdaptiveDialog(string dialogId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogId)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonIgnore]
        public IStatePropertyAccessor<BotState> BotState { get; set; }

        [JsonIgnore]
        public IStatePropertyAccessor<Dictionary<string, object>> UserState { get; set; }

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
        public ILanguageGenerator Generator { get; set; }

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
        public ITriggerSelector Selector { get; set; }

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
                var client = value ?? new NullBotTelemetryClient();
                this.Dialogs.TelemetryClient = client;
                base.TelemetryClient = client;
            }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} should not ever be a cancellation token");
            }

            EnsureDependenciesInstalled();

            var dcState = dc.GetState();

            if (!dcState.ContainsKey(DialogPath.EventCounter))
            {
                dcState.SetValue(DialogPath.EventCounter, 0u);
            }

            if (dialogSchema != null && !dcState.ContainsKey(DialogPath.RequiredProperties))
            {
                // RequiredProperties control what properties must be filled in.
                // Initialize if not present from schema.
                dcState.SetValue(DialogPath.RequiredProperties, dialogSchema.Required());
            }

            if (needsTracker)
            {
                if (!dcState.ContainsKey(ConditionTracker))
                {
                    foreach (var trigger in Triggers)
                    {
                        if (trigger.RunOnce && trigger.Condition != null)
                        {
                            var paths = dcState.TrackPaths(trigger.Condition.ToExpression().References());
                            var triggerPath = $"{ConditionTracker}.{trigger.Id}.";
                            dcState.SetValue(triggerPath + "paths", paths);
                            dcState.SetValue(triggerPath + "lastRun", 0u);
                        }
                    }
                }
            }

            SetLocalGenerator(dc.Context);

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

            await OnDialogEventAsync(dc, dialogEvent, cancellationToken).ConfigureAwait(false);

            // Continue step execution
            return await ContinueActionsAsync(dc, options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            EnsureDependenciesInstalled();

            SetLocalGenerator(dc.Context);

            // Continue step execution
            return await ContinueActionsAsync(dc, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            SetLocalGenerator(dc.Context);

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
            RestoreParentGenerator(turnContext);
            return base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default)
        {
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
                var ctx = new SequenceContext(this.Dialogs, dc, state.Actions.First(), state.Actions, changeTurnKey, this.Dialogs);
                ctx.Parent = dc;
                return ctx;
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
            var sequenceContext = ToSequenceContext(dc);

            // Process event and queue up any potential interruptions
            return await ProcessEventAsync(sequenceContext, dialogEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<bool> OnPostBubbleEventAsync(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default)
        {
            var sequenceContext = ToSequenceContext(dc);

            // Process event and queue up any potential interruptions
            return await ProcessEventAsync(sequenceContext, dialogEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<bool> ProcessEventAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dcState = sequenceContext.GetState();

            // Save into turn
            dcState.SetValue(TurnPath.DIALOGEVENT, dialogEvent);

            EnsureDependenciesInstalled();

            // Count of events processed
            var count = dcState.GetValue<uint>(DialogPath.EventCounter);
            dcState.SetValue(DialogPath.EventCounter, ++count);

            // Look for triggered evt
            var handled = await QueueFirstMatchAsync(sequenceContext, dialogEvent, preBubble, cancellationToken).ConfigureAwait(false);

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
                        if (dcState.GetBoolValue(TurnPath.ACTIVITYPROCESSED) == false)
                        {
                            // Emit leading ActivityReceived event
                            var activityReceivedEvent = new DialogEvent()
                            {
                                Name = AdaptiveEvents.ActivityReceived,
                                Value = sequenceContext.Context.Activity,
                                Bubble = false
                            };

                            handled = await ProcessEventAsync(sequenceContext, dialogEvent: activityReceivedEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case AdaptiveEvents.ActivityReceived:

                        if (sequenceContext.Context.Activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance (ignore handled)
                            var recognizeUtteranceEvent = new DialogEvent
                            {
                                Name = AdaptiveEvents.RecognizeUtterance,
                                Value = sequenceContext.Context.Activity,
                                Bubble = false
                            };
                            await ProcessEventAsync(sequenceContext, dialogEvent: recognizeUtteranceEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                            // Emit leading RecognizedIntent event
                            var recognized = dcState.GetValue<RecognizerResult>(TurnPath.RECOGNIZED);
                            var recognizedIntentEvent = new DialogEvent
                            {
                                Name = AdaptiveEvents.RecognizedIntent,
                                Value = recognized,
                                Bubble = false
                            };
                            ProcessEntities(sequenceContext);
                            handled = await ProcessEventAsync(sequenceContext, dialogEvent: recognizedIntentEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        // Has an interruption occured?
                        // - Setting this value to true causes any running inputs to re-prompt when they're
                        //   continued.  The developer can clear this flag if they want the input to instead
                        //   process the users uterrance when its continued.
                        if (handled)
                        {
                            dcState.SetValue(TurnPath.INTERRUPTED, true);
                        }

                        break;

                    case AdaptiveEvents.RecognizeUtterance:

                        if (sequenceContext.Context.Activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance
                            var recognized = await OnRecognize(sequenceContext, cancellationToken).ConfigureAwait(false);

                            dcState.SetValue(TurnPath.RECOGNIZED, recognized);

                            var (name, score) = recognized.GetTopScoringIntent();
                            dcState.SetValue(TurnPath.TOPINTENT, name);
                            dcState.SetValue(DialogPath.LastIntent, name);
                            dcState.SetValue(TurnPath.TOPSCORE, score);

                            if (Recognizer != null)
                            {
                                await sequenceContext.DebuggerStepAsync(Recognizer, AdaptiveEvents.RecognizeUtterance, cancellationToken).ConfigureAwait(false);
                            }

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
                        if (dcState.GetBoolValue(TurnPath.ACTIVITYPROCESSED) == false)
                        {
                            var activityReceivedEvent = new DialogEvent
                            {
                                Name = AdaptiveEvents.ActivityReceived,
                                Value = sequenceContext.Context.Activity,
                                Bubble = false
                            };

                            handled = await ProcessEventAsync(sequenceContext, dialogEvent: activityReceivedEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case AdaptiveEvents.ActivityReceived:

                        var activity = sequenceContext.Context.Activity;

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Empty sequence?
                            if (!sequenceContext.Actions.Any())
                            {
                                // Emit trailing unknownIntent event
                                var unknownIntentEvent = new DialogEvent
                                {
                                    Name = AdaptiveEvents.UnknownIntent,
                                    Bubble = false
                                };
                                handled = await ProcessEventAsync(sequenceContext, dialogEvent: unknownIntentEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
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
                            dcState.SetValue(TurnPath.INTERRUPTED, true);
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
            var sequenceContext = ToSequenceContext(dc);
            await sequenceContext.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);

            // Get a unique instance ID for the current stack entry.
            // We need to do this because things like cancellation can cause us to be removed
            // from the stack and we want to detect this so we can stop processing actions.
            var instanceId = GetUniqueInstanceId(sequenceContext);

            // Execute queued actions
            var actionContext = CreateChildContext(sequenceContext) as SequenceContext;
            while (actionContext != null)
            {
                // Continue current step
                // DEBUG: To debug step execution set a breakpoint on line below and add a watch 
                //        statement for sequenceContext.Actions.
                var result = await actionContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

                // Start step if not continued
                if (result.Status == DialogTurnStatus.Empty && GetUniqueInstanceId(sequenceContext) == instanceId)
                {
                    // Call begin dialog on our next step, passing the effective options we computed
                    var nextAction = actionContext.Actions.First();
                    result = await actionContext.BeginDialogAsync(nextAction.DialogId, nextAction.Options, cancellationToken).ConfigureAwait(false);
                }

                // Is the step waiting for input or were we cancelled?
                if (result.Status == DialogTurnStatus.Waiting || GetUniqueInstanceId(sequenceContext) != instanceId)
                {
                    return result;
                }

                // End current step
                await EndCurrentActionAsync(sequenceContext, cancellationToken).ConfigureAwait(false);

                if (result.Status == DialogTurnStatus.CompleteAndWait)
                {
                    // Child dialog completed, but wants us to wait for a new activity
                    result.Status = DialogTurnStatus.Waiting;
                    return result;
                }

                var parentChanges = false;
                DialogContext root = sequenceContext;
                var parent = sequenceContext.Parent;
                while (parent != null)
                {
                    var sc = parent as SequenceContext;
                    if (sc != null && sc.Changes != null && sc.Changes.Count > 0)
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
                await sequenceContext.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);
                actionContext = CreateChildContext(sequenceContext) as SequenceContext;
            }

            return await OnEndOfActionsAsync(sequenceContext, cancellationToken).ConfigureAwait(false);
        }

        protected Task<bool> EndCurrentActionAsync(SequenceContext sequenceContext, CancellationToken cancellationToken = default)
        {
            if (sequenceContext.Actions.Any())
            {
                sequenceContext.Actions.RemoveAt(0);
            }

            return Task.FromResult(false);
        }

        protected async Task<DialogTurnResult> OnEndOfActionsAsync(SequenceContext sequenceContext, CancellationToken cancellationToken = default)
        {
            // Is the current dialog still on the stack?
            if (sequenceContext.ActiveDialog != null)
            {
                var dcState = sequenceContext.GetState();

                // Completed actions so continue processing entity queues
                var handled = await ProcessQueuesAsync(sequenceContext, cancellationToken).ConfigureAwait(false);

                if (handled)
                {
                    // Still processing queues
                    return await ContinueActionsAsync(sequenceContext, null, cancellationToken).ConfigureAwait(false);
                }
                else if (ShouldEnd(sequenceContext))
                {
                    RestoreParentGenerator(sequenceContext.Context);
                    dcState.TryGetValue<object>(DefaultResultProperty, out var result);
                    return await sequenceContext.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                }

                return EndOfTurn;
            }

            return new DialogTurnResult(DialogTurnStatus.Cancelled);
        }

        protected async Task<RecognizerResult> OnRecognize(SequenceContext sequenceContext, CancellationToken cancellationToken = default)
        {
            var context = sequenceContext.Context;
            if (Recognizer != null)
            {
                var result = await Recognizer.RecognizeAsync(sequenceContext, cancellationToken).ConfigureAwait(false);

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
                Text = context.Activity.Text ?? string.Empty,
                Intents = new Dictionary<string, IntentScore> { { "None", new IntentScore { Score = 0.0 } } },
            };
        }

        // This function goes through the ambiguity queues and emits events if present.
        // In order ClearProperties, AssignEntity, ChooseProperties, ChooseEntity, EndOfActions.
        private async Task<bool> ProcessQueuesAsync(SequenceContext sequenceContext, CancellationToken cancellationToken)
        {
            var dcState = sequenceContext.GetState();

            DialogEvent evt;
            var queues = EntityEvents.Read(sequenceContext);
            var changed = false;
            if (queues.ClearProperties.Any())
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.ClearProperty, Value = queues.ClearProperties.Dequeue(), Bubble = false };
                changed = true;
            }
            else if (queues.AssignEntities.Any())
            {
                var val = queues.AssignEntities.Dequeue();
                evt = new DialogEvent() { Name = AdaptiveEvents.AssignEntity, Value = val, Bubble = false };

                // TODO: For now, I'm going to dereference to a one-level array value.  There is a bug in the current code in the distinction between
                // @ which is supposed to unwrap down to non-array and @@ which returns the whole thing. @ in the curent code works by doing [0] which
                // is not enough.
                var entity = val.Entity.Value;
                if (!(entity is JArray))
                {
                    entity = new object[] { entity };
                }

                dcState.SetValue($"{TurnPath.RECOGNIZED}.entities.{val.Entity.Name}", entity);
                changed = true;
            }
            else if (queues.ChooseProperties.Any())
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.ChooseProperty, Value = queues.ChooseProperties[0], Bubble = false };
            }
            else if (queues.ChooseEntities.Any())
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.ChooseEntity, Value = queues.ChooseEntities[0], Bubble = false };
            }
            else
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.EndOfActions, Bubble = false };
            }

            if (changed)
            {
                queues.Write(sequenceContext);
            }

            dcState.SetValue(DialogPath.LastEvent, evt.Name);
            var handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!handled)
            {
                // If event wasn't handled, remove it from queues and keep going if things changed
                if (queues.DequeueEvent(evt.Name))
                {
                    queues.Write(sequenceContext);
                    handled = await this.ProcessQueuesAsync(sequenceContext, cancellationToken);
                }
            }

            return handled;
        }

        private string GetUniqueInstanceId(DialogContext dc)
        {
            return dc.Stack.Count > 0 ? $"{dc.Stack.Count}:{dc.ActiveDialog.Id}" : string.Empty;
        }

        private async Task<bool> QueueFirstMatchAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken)
        {
            var selection = await Selector.Select(sequenceContext, cancellationToken).ConfigureAwait(false);
            if (selection.Any())
            {
                var evt = selection.First();
                await sequenceContext.DebuggerStepAsync(evt, dialogEvent, cancellationToken).ConfigureAwait(false);
                Trace.TraceInformation($"Executing Dialog: {Id} Rule[{evt.Id}]: {evt.GetType().Name}: {evt.GetExpression()}");
                var changes = await evt.ExecuteAsync(sequenceContext).ConfigureAwait(false);

                if (changes != null && changes.Any())
                {
                    sequenceContext.QueueChanges(changes[0]);
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

        private SequenceContext ToSequenceContext(DialogContext dc)
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

            var sequenceContext = new SequenceContext(dc.Dialogs, dc, new DialogState { DialogStack = dc.Stack }, state.Actions, changeTurnKey, this.Dialogs);
            sequenceContext.Parent = dc.Parent;
            return sequenceContext;
        }

        private void SetLocalGenerator(ITurnContext context)
        {
            if (Generator != null)
            {
                var previousGenerator = context.TurnState.Get<ILanguageGenerator>(generatorTurnKey);
                if (previousGenerator == null)
                {
                    previousGenerator = context.TurnState.Get<ILanguageGenerator>();
                    if (previousGenerator != null)
                    {
                        context.TurnState.Add(generatorTurnKey, previousGenerator);
                    }
                }

                context.TurnState.Set<ILanguageGenerator>(Generator);
            }
        }

        private void RestoreParentGenerator(ITurnContext context)
        {
            var previousGenerator = context.TurnState.Get<ILanguageGenerator>(generatorTurnKey);
            if (previousGenerator != null)
            {
                context.TurnState.Set(previousGenerator);
                context.TurnState.Remove(this.generatorTurnKey);
            }
        }

        // Process entities to identify ambiguity and possible assigment to properties.  Broadly the steps are:
        // Normalize entities to include meta-data
        // Check to see if an entity is in response to a previous ambiguity event
        // Assign entities to possible properties
        // Merge new queues into existing queues of ambiguity events
        private void ProcessEntities(SequenceContext context)
        {
            var dcState = context.GetState();

            if (dialogSchema != null)
            {
                if (dcState.TryGetValue<string>(DialogPath.LastEvent, out var lastEvent))
                {
                    dcState.RemoveValue(DialogPath.LastEvent);
                }

                var queues = EntityEvents.Read(context);
                var entities = NormalizeEntities(context);
                var utterance = context.Context.Activity?.AsMessageActivity()?.Text;
                if (!dcState.TryGetValue<string[]>(DialogPath.ExpectedProperties, out var expected))
                {
                    expected = new string[0];
                }

                // Utterance is a special entity that corresponds to the full utterance
                entities["utterance"] = new List<EntityInfo> { new EntityInfo { Priority = int.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = "utterance", Score = 0.0, Type = "string", Value = utterance, Text = utterance } };
                var recognized = AssignEntities(context, entities, expected, queues, lastEvent);
                var unrecognized = SplitUtterance(utterance, recognized);

                // TODO: Is this actually useful information?
                dcState.SetValue(TurnPath.UNRECOGNIZEDTEXT, unrecognized);
                dcState.SetValue(TurnPath.RECOGNIZEDENTITIES, recognized);
                var turn = dcState.GetValue<uint>(DialogPath.EventCounter);
                CombineOldEntityToProperties(queues, turn);
                queues.Write(context);
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

        // We have four kinds of ambiguity to deal with:
        // * Entity: Ambiguous interpretation of entity value: (peppers -> [green peppers, red peppers]  Tell this by entity value is array.  Doesn't matter if property singleton or not. Ask.
        // * Text: Ambiguous interpretation of text: (3 -> age or number) Identify by overlapping entities. Resolve by greater coverage, expected entity, ask.
        // * Singleton: two different entities which could fill property singleton.  Could be same type or different types.  Resolve by rule priority.
        // * Property: Which property should an entity go to?  Resolve by expected, then ask.

        // Combine entity values and $instance meta-data
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(SequenceContext context)
        {
            var dcState = context.GetState();
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();
            var text = dcState.GetValue<string>(TurnPath.RECOGNIZED + ".text");
            if (dcState.TryGetValue<dynamic>(TurnPath.RECOGNIZED + ".entities", out var entities))
            {
                var turn = dcState.GetValue<uint>(DialogPath.EventCounter);
                var metaData = entities["$instance"];
                foreach (var entry in entities)
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
                                Value = val
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

        // Generate possible entity to property mappings
        private IEnumerable<EntityAssignment> Candidates(Dictionary<string, List<EntityInfo>> entities, string[] expected)
        {
            var globalExpectedOnly = dialogSchema.Schema["$expectedOnly"]?.ToObject<List<string>>() ?? new List<string>();
            foreach (var propSchema in dialogSchema.Property.Children)
            {
                var isExpected = expected.Contains(propSchema.Path);
                var expectedOnly = propSchema.ExpectedOnly;
                foreach (var entityName in propSchema.Entities)
                {
                    if (entities.TryGetValue(entityName, out var matches) && (isExpected || !(expectedOnly != null ? expectedOnly : globalExpectedOnly).Contains(entityName)))
                    {
                        foreach (var entity in matches)
                        {
                            yield return new EntityAssignment
                            {
                                Entity = entity,
                                Property = propSchema.Path,

                                // TODO: Eventually we should be able to pick up an add/remove composite here as an alternative
                                Operation = AssignEntityOperations.Add,
                                IsExpected = isExpected
                            };
                        }
                    }
                }
            }
        }

        private void AddMappingToQueue(EntityAssignment mapping, EntityEvents queues)
        {
            if (mapping.Entity.Value is JArray arr)
            {
                if (arr.Count > 1)
                {
                    queues.ChooseEntities.Add(mapping);
                }
                else
                {
                    mapping.Entity.Value = arr[0];
                    var i = 0;
                    for (; i < queues.AssignEntities.Count(); i++)
                    {
                        var assign = queues.AssignEntities[i];
                        if (assign.IsExpected)
                        {
                            // Assign in front of first expected so unexpected assignments can be confirmed
                            queues.AssignEntities.Insert(i, mapping);
                            break;
                        }
                    }

                    if (i == queues.AssignEntities.Count())
                    {
                        queues.AssignEntities.Add(mapping);
                    }
                }
            }
            else
            {
                queues.AssignEntities.Add(mapping);
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

        // Have each property pick which overlapping entity is the best one
        private IEnumerable<EntityAssignment> RemoveOverlappingPerProperty(IEnumerable<EntityAssignment> candidates)
        {
            var perProperty = from candidate in candidates
                              group candidate by candidate.Property;
            foreach (var propChoices in perProperty)
            {
                var schema = dialogSchema.PathToSchema(propChoices.Key);
                var choices = propChoices.ToList();

                // Assume preference by order listed in mappings
                // Alternatives would be to look at coverage or other metrices
                foreach (var entity in schema.Entities)
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

        private List<EntityInfo> AddToQueues(SequenceContext context, Dictionary<string, List<EntityInfo>> entities, string[] expected, EntityEvents queues, string lastEvent)
        {
            var dcState = context.GetState();
            var candidates = (from candidate in RemoveOverlappingPerProperty(Candidates(entities, expected))
                              orderby candidate.IsExpected descending
                              select candidate).ToList();
            var usedEntities = new HashSet<EntityInfo>(from candidate in candidates select candidate.Entity);
            var expectedChoices = new List<string>();
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
                if (lastEvent == AdaptiveEvents.ChooseEntity && candidate.Property == queues.ChooseEntities[0].Property)
                {
                    // Property has resolution so remove entity ambiguity
                    queues.ChooseEntities.Dequeue();
                    lastEvent = null;
                }
                else if (lastEvent == AdaptiveEvents.ChooseProperty && candidate.Entity.Name == "PROPERTYName")
                {
                    // NOTE: This assumes the existence of an entity named PROPERTYName for resolving this ambiguity
                    var choices = queues.ChooseProperties[0];
                    var entity = (candidate.Entity.Value as JArray)?[0]?.ToObject<string>();
                    var choice = choices.Find(p => p.Property == entity);
                    if (choice != null)
                    {
                        // Resolve choice, pretend it was expected and add to queues
                        choice.IsExpected = true;
                        expectedChoices.Add(choice.Property);
                        AddMappingToQueue(choice, queues);
                        choices.RemoveAll(c => c.Entity.Overlaps(choice.Entity));
                        mapped = true;
                    }
                }

                usedEntities.Add(candidate.Entity);
                foreach (var alternative in alternatives)
                {
                    usedEntities.Add(alternative.Entity);
                }

                if (!mapped)
                {
                    if (alternatives.Count() == 1)
                    {
                        AddMappingToQueue(candidate, queues);
                    }
                    else
                    {
                        queues.ChooseProperties.Add(alternatives);
                    }
                }
            }

            if (expectedChoices.Any())
            {
                // When choosing between property assignments, make the assignments be expected.
                dcState.SetValue(DialogPath.ExpectedProperties, expectedChoices);
                var choices = queues.ChooseProperties[0];

                // Add back in any non-overlapping choices
                while (choices.Any())
                {
                    var choice = choices.First();
                    AddMappingToQueue(choice, queues);
                    choices.RemoveAll(c => c.Entity.Overlaps(choice.Entity));
                }

                queues.ChooseProperties.Dequeue();
            }

            return (from entity in usedEntities orderby entity.Start ascending select entity).ToList();
        }

        private EntityEvents PropertyQueues(string path, Dictionary<PropertySchema, EntityEvents> propertyToQueues)
        {
            var prop = dialogSchema.PathToSchema(path);
            if (!propertyToQueues.TryGetValue(prop, out var propertyQueues))
            {
                propertyQueues = new EntityEvents();
                propertyToQueues[prop] = propertyQueues;
            }

            return propertyQueues;
        }

        // Create queues for each property
        private Dictionary<PropertySchema, EntityEvents> PerPropertyQueues(EntityEvents queues)
        {
            var propertyToQueues = new Dictionary<PropertySchema, EntityEvents>();
            foreach (var entry in queues.AssignEntities)
            {
                PropertyQueues(entry.Property, propertyToQueues).AssignEntities.Add(entry);
            }

            foreach (var entry in queues.ChooseEntities)
            {
                PropertyQueues(entry.Property, propertyToQueues).ChooseEntities.Add(entry);
            }

            foreach (var entry in queues.ClearProperties)
            {
                PropertyQueues(entry, propertyToQueues).ClearProperties.Add(entry);
            }

            foreach (var entry in queues.ChooseProperties)
            {
                foreach (var choice in entry)
                {
                    PropertyQueues(choice.Property, propertyToQueues).ChooseProperties.Add(entry);
                }
            }

            return propertyToQueues;
        }

        private void CombineNewEntityProperties(EntityEvents queues)
        {
            var propertyToQueues = PerPropertyQueues(queues);
            foreach (var entry in propertyToQueues)
            {
                var property = entry.Key;
                var propertyQueues = entry.Value;
                if (!property.IsArray && propertyQueues.AssignEntities.Count() + propertyQueues.ChooseEntities.Count() > 1)
                {
                    // Singleton with multiple operations
                    var mappings = from mapping in propertyQueues.AssignEntities.Union(propertyQueues.ChooseEntities) where mapping.Operation != AssignEntityOperations.Remove select mapping;
                    switch (mappings.Count())
                    {
                        case 0:
                            queues.ClearProperties.Add(property.Path);
                            break;
                        case 1:
                            AddMappingToQueue(mappings.First(), queues);
                            break;
                        default:
                            // TODO: Map to multiple entity to property
                            /* queues.ChooseProperties.Add(new EntitiesToProperty
                            {
                                Entities = (from mapping in mappings select mapping.Entity).ToList(),
                                Property = mappings.First().Change
                            }); */
                            break;
                    }
                }
            }

            // TODO: There is a lot more we can do here
        }

        private void CombineOldEntityToProperties(EntityEvents queues, uint turn)
        {
            var propertyToQueues = PerPropertyQueues(queues);
            foreach (var entry in propertyToQueues)
            {
                var property = entry.Key;
                var propertyQueues = entry.Value;
                if (!property.IsArray &&
                    (propertyQueues.AssignEntities.Any(e => e.Entity.WhenRecognized == turn)
                    || propertyQueues.ChooseEntities.Any(e => e.Entity.WhenRecognized == turn)
                    || propertyQueues.ChooseProperties.Any(c => c.First().Entity.WhenRecognized == turn)))
                {
                    // Remove all old operations on property because there is a new one
                    foreach (var mapping in propertyQueues.AssignEntities)
                    {
                        if (mapping.Entity.WhenRecognized != turn)
                        {
                            queues.AssignEntities.Remove(mapping);
                        }
                    }

                    foreach (var mapping in propertyQueues.ChooseEntities)
                    {
                        if (mapping.Entity.WhenRecognized != turn)
                        {
                            queues.ChooseEntities.Remove(mapping);
                        }
                    }

                    foreach (var mapping in propertyQueues.ChooseProperties)
                    {
                        if (mapping.First().Entity.WhenRecognized != turn)
                        {
                            queues.ChooseProperties.Remove(mapping);
                        }
                    }
                }
            }
        }

        // Assign entities to queues
        private List<EntityInfo> AssignEntities(SequenceContext context, Dictionary<string, List<EntityInfo>> entities, string[] expected, EntityEvents queues, string lastEvent)
        {
            var recognized = AddToQueues(context, entities, expected, queues, lastEvent);
            CombineNewEntityProperties(queues);
            return recognized;
        }
    }
}
