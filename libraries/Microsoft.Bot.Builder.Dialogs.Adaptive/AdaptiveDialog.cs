// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
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

        private const string AdaptiveKey = "_adaptive";

        // unique key for language generator turn property, (TURN STATE ONLY)
        private readonly string generatorTurnKey = Guid.NewGuid().ToString();

        // unique key for change tracking of the turn state (TURN STATE ONLY)
        private readonly string changeTurnKey = Guid.NewGuid().ToString();

        private bool installedDependencies;

        private bool needsTracker = false;

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
        public IRecognizer Recognizer { get; set; }

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
        public DialogSchema Schema { get; set; }

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

            if (needsTracker)
            {
                if (!dc.GetState().ContainsKey(DialogPath.EventCounter))
                {
                    dc.GetState().SetValue(DialogPath.EventCounter, 0u);
                }

                if (!dc.GetState().ContainsKey(DialogPath.ConditionTracker))
                {
                    var parser = Selector.Parser;
                    foreach (var trigger in Triggers)
                    {
                        if (trigger.RunOnce)
                        {
                            // TODO: Should probably use the full expression, but wrap things like event processing in ignore
                            var paths = dc.GetState().Track(parser.Parse(trigger.Condition).References());
                            var triggerPath = DialogPath.ConditionTracker + "." + trigger.Id + ".";
                            dc.GetState().SetValue(triggerPath + "paths", paths);
                            dc.GetState().SetValue(triggerPath + "lastRun", 0u);
                        }
                    }
                }
            }

            SetLocalGenerator(dc.Context);

            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            activeDialogState[AdaptiveKey] = new AdaptiveDialogState();
            var state = activeDialogState[AdaptiveKey] as AdaptiveDialogState;

            // Persist options to dialog state
            dc.GetState().SetValue(ThisPath.OPTIONS, options);

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
            // Save into turn
            sequenceContext.GetState().SetValue(TurnPath.DIALOGEVENT, dialogEvent);

            EnsureDependenciesInstalled();

            // Count of events processed
            var count = sequenceContext.GetState().GetValue<uint>(DialogPath.EventCounter);
            sequenceContext.GetState().SetValue(DialogPath.EventCounter, ++count);

            // Save schema information
            if (this.Schema != null)
            {
                sequenceContext.GetState().SetValue(TurnPath.SCHEMA, this.Schema.Schema);
                if (!sequenceContext.GetState().ContainsKey(DialogPath.RequiredProperties))
                {
                    // All properties required by default unless specified.
                    sequenceContext.GetState().SetValue(DialogPath.RequiredProperties, this.Schema.Required());
                }
            }

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
                        if (sequenceContext.GetState().GetBoolValue(TurnPath.ACTIVITYPROCESSED) == false)
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
                            var recognized = sequenceContext.GetState().GetValue<RecognizerResult>(TurnPath.RECOGNIZED);
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
                            sequenceContext.GetState().SetValue(TurnPath.INTERRUPTED, true);
                        }

                        break;

                    case AdaptiveEvents.RecognizeUtterance:

                        if (sequenceContext.Context.Activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance
                            var recognized = await OnRecognize(sequenceContext, cancellationToken).ConfigureAwait(false);

                            sequenceContext.GetState().SetValue(TurnPath.RECOGNIZED, recognized);

                            var (name, score) = recognized.GetTopScoringIntent();
                            sequenceContext.GetState().SetValue(TurnPath.TOPINTENT, name);                            
                            sequenceContext.GetState().SetValue(DialogPath.LastIntent, name);
                            sequenceContext.GetState().SetValue(TurnPath.TOPSCORE, score);

                            if (Recognizer != null)
                            {
                                await sequenceContext.DebuggerStepAsync(Recognizer, AdaptiveEvents.RecognizeUtterance, cancellationToken).ConfigureAwait(false);
                            }

                            handled = true;
                        }

                        break;

                    case AdaptiveEvents.EndOfActions:
                        // Completed actions so continue processing form queues
                        handled = await ProcessFormAsync(sequenceContext, cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
            else
            {
                switch (dialogEvent.Name)
                {
                    case AdaptiveEvents.BeginDialog:
                        if (sequenceContext.GetState().GetBoolValue(TurnPath.ACTIVITYPROCESSED) == false)
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
                            sequenceContext.GetState().SetValue(TurnPath.INTERRUPTED, true);
                        }

                        break;
                }
            }

            return handled;
        }

        protected async Task<bool> ProcessFormAsync(SequenceContext sequenceContext, CancellationToken cancellationToken)
        {
            DialogEvent evt;
            var queues = EventQueues.Read(sequenceContext);
            var changed = queues.DequeueEvent(sequenceContext.GetState().GetValue<string>(DialogPath.LastEvent));
            if (queues.ClearProperty.Any())
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.ClearProperty, Value = queues.ClearProperty[0], Bubble = false };
            }
            else if (queues.SetProperty.Any())
            {
                var val = queues.SetProperty[0];
                evt = new DialogEvent() { Name = AdaptiveEvents.SetProperty, Value = val, Bubble = false };

                // TODO: For now, I'm going to dereference to a one-level array value.  There is a bug in the current code in the distinction between
                // @ which is supposed to unwrap down to non-array and @@ which returns the whole thing. @ in the curent code works by doing [0] which
                // is not enough.
                var entity = val.Entity.Value;
                if (!(entity is JArray))
                {
                    entity = new object[] { entity };
                }

                sequenceContext.GetState().SetValue($"{TurnPath.RECOGNIZED}.entities.{val.Entity.Name}", entity);
            }
            else if (queues.ChooseProperty.Any())
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.ChooseProperty, Value = queues.ChooseProperty[0], Bubble = false };
            }
            else if (queues.ClarifyEntity.Any())
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.ClarifyEntity, Value = queues.ClarifyEntity[0], Bubble = false };
            }
            else
            {
                evt = new DialogEvent() { Name = AdaptiveEvents.Ask, Bubble = false };
            }

            if (changed)
            {
                queues.Write(sequenceContext);
            }

            sequenceContext.GetState().SetValue(DialogPath.LastEvent, evt.Name);
            var handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!handled)
            {
                // If event wasn't handled, remove it from queues and keep going if things changed
                if (queues.DequeueEvent(evt.Name))
                {
                    queues.Write(sequenceContext);
                    handled = await this.ProcessFormAsync(sequenceContext, cancellationToken);
                }
            }

            return handled;
        }

        protected override string OnComputeId()
        {
            if (DebugSupport.SourceMap.TryGetValue(this, out var range))
            {
                return $"{GetType().Name}({Path.GetFileName(range.Path)}:{range.StartPoint.LineIndex})";
            }

            return $"{GetType().Name}[]";
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
                    // Waiting in next step
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
                // Raise EndOfActions event
                var endOfActionsEvent = new DialogEvent() { Name = AdaptiveEvents.EndOfActions, Bubble = false };
                var handled = await OnDialogEventAsync(sequenceContext, endOfActionsEvent, cancellationToken).ConfigureAwait(false);

                if (handled)
                {
                    // EndOfActions event was handled
                    return await ContinueActionsAsync(sequenceContext, null, cancellationToken).ConfigureAwait(false);
                }
                else if (ShouldEnd(sequenceContext))
                {
                    RestoreParentGenerator(sequenceContext.Context);
                    sequenceContext.GetState().TryGetValue<object>(DefaultResultProperty, out var result);
                    return await sequenceContext.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                }

                return EndOfTurn;
            }

            return new DialogTurnResult(DialogTurnStatus.Cancelled);
        }

        protected async Task<RecognizerResult> OnRecognize(SequenceContext sequenceContext, CancellationToken cancellationToken = default)
        {
            var context = sequenceContext.Context;
            var noneIntent = new RecognizerResult
            {
                Text = context.Activity.Text ?? string.Empty,
                Intents = new Dictionary<string, IntentScore> { { "None", new IntentScore { Score = 0.0 } } },
                Entities = JObject.Parse("{}")
            };
            var text = context.Activity.Text;
            if (context.Activity.Value != null)
            {
                var value = JObject.FromObject(context.Activity.Value);

                // Check for submission of an adaptive card
                if (string.IsNullOrEmpty(text) && value.Property("intent") != null)
                {
                    // Map submitted values to a recognizer result
                    var recognized = new RecognizerResult { Text = string.Empty };

                    foreach (var property in value.Properties())
                    {
                        if (property.Name.ToLower() == "intent")
                        {
                            recognized.Intents[property.Value.ToString()] = new IntentScore { Score = 1.0 };
                        }
                        else
                        {
                            if (recognized.Entities.Property(property.Name) == null)
                            {
                                recognized.Entities[property.Name] = new JArray(property.Value);
                            }
                            else
                            {
                                ((JArray)recognized.Entities[property.Name]).Add(property.Value);
                            }
                        }
                    }

                    return recognized;
                }
            }

            if (Recognizer != null)
            {
                var result = await Recognizer.RecognizeAsync(context, cancellationToken).ConfigureAwait(false);

                // only allow one intent
                var topIntent = result.GetTopScoringIntent();
                result.Intents.Clear();
                result.Intents.Add(topIntent.intent, new IntentScore { Score = topIntent.score });
                return result;
            }

            return noneIntent;
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
                var evt = (from conditional in selection orderby conditional.Priority ascending select conditional).First();
                await sequenceContext.DebuggerStepAsync(evt, dialogEvent, cancellationToken).ConfigureAwait(false);
                Trace.TraceInformation($"Executing Dialog: {Id} Rule[{selection}]: {evt.GetType().Name}: {evt.GetExpression(new ExpressionEngine())}");
                var changes = await evt.ExecuteAsync(sequenceContext).ConfigureAwait(false);

                if (changes != null && changes.Count() > 0)
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

                    var id = 0u;
                    var noActivity = 0;
                    var activity = 1000;
                    var input = 2000;
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

                        if (!trigger.Priority.HasValue)
                        {
                            // Analyze actions to set default priorities
                            // 0-999 Non-activity
                            // 1000-1999 Sends activity
                            // 2000+ Contains input action
                            var foundActivity = false;
                            var foundInput = false;
                            foreach (var action in trigger.Actions)
                            {
                                if (action is InputDialog || action is Ask)
                                {
                                    foundInput = true;
                                }
                                else if (action is SendActivity)
                                {
                                    foundActivity = true;
                                }
                            }

                            if (foundInput)
                            {
                                trigger.Priority = input++;
                            }
                            else if (foundActivity)
                            {
                                trigger.Priority = activity++;
                            }
                            else
                            {
                                trigger.Priority = noActivity++;
                            }
                        }

                        trigger.Id = id++;
                    }

                    // Wire up selector
                    if (Selector == null)
                    {
                        // Default to most specific then first
                        Selector = new MostSpecificSelector { Selector = new FirstSelector() };
                    }

                    this.Selector.Initialize(Triggers, true);
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

        private void ProcessEntities(SequenceContext context)
        {
            if (Schema != null)
            {
                var queues = EventQueues.Read(context);
                var entities = NormalizeEntities(context);
                var utterance = context.Context.Activity?.AsMessageActivity()?.Text;
                if (!context.GetState().TryGetValue<string[]>("$expectedProperties", out var expected))
                {
                    expected = new string[0];
                }

                if (expected.Contains("utterance"))
                {
                    entities["utterance"] = new List<EntityInfo> { new EntityInfo { Priority = int.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = "utterance", Score = 0.0, Type = "string", Value = utterance, Text = utterance } };
                }

                var updated = UpdateLastEvent(context, queues, entities);
                var newQueues = new EventQueues();
                var recognized = AssignEntities(entities, expected, newQueues);
                var unrecognized = SplitUtterance(utterance, recognized);
                recognized.AddRange(updated);

                context.GetState().SetValue(TurnPath.UNRECOGNIZEDTEXT, unrecognized);
                context.GetState().SetValue(TurnPath.RECOGNIZEDENTITIES, recognized);

                // turn.unrecognizedText = [<text not consumed by entities>]
                // turn.consumedEntities = [entityInfo] 
                queues.Merge(newQueues);
                var turn = context.GetState().GetValue<uint>(DialogPath.EventCounter);
                CombineOldEntityToPropertys(queues, turn);
                queues.Write(context);
            }
        }

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

        private List<EntityInfo> UpdateLastEvent(SequenceContext context, EventQueues queues, Dictionary<string, List<EntityInfo>> entities)
        {
            var recognized = new List<EntityInfo>();
            if (context.GetState().TryGetValue<string>(DialogPath.LastEvent, out var evt))
            {
                switch (evt)
                {
                    case AdaptiveEvents.ClarifyEntity:
                        {
                            context.GetState().RemoveValue(DialogPath.LastEvent);
                            var entityToProperty = queues.ClarifyEntity[0];
                            var ambiguousEntity = entityToProperty.Entity;
                            var choices = ambiguousEntity.Value as JArray;

                            // TODO: There could be no way to resolve the ambiguity, i.e. wheat has synonym wheat and
                            // honeywheat has synonym wheat.  For now rely on the model to not have that issue.
                            if (entities.TryGetValue(ambiguousEntity.Name, out var infos) && infos.Count() == 1)
                            {
                                var info = infos.First();
                                var foundValues = info.Value as JArray;
                                var common = choices.Intersect(foundValues);
                                if (common.Count() == 1)
                                {
                                    // Resolve and move to SetProperty
                                    recognized.Add(info);
                                    infos.Clear();
                                    entityToProperty.Entity = info;
                                    entityToProperty.Expected = true;
                                    queues.ClarifyEntity.Dequeue();
                                    queues.SetProperty.Add(entityToProperty);
                                }
                            }

                            break;
                        }

                    case AdaptiveEvents.ChooseProperty:
                        {
                            context.GetState().RemoveValue(DialogPath.LastEvent);

                            // NOTE: This assumes the existance of a property entity which contains the normalized
                            // names of the properties.
                            if (entities.TryGetValue("PROPERTYName", out var infos) && infos.Count() == 1)
                            {
                                var info = infos[0];
                                var choices = queues.ChooseProperty[0];
                                var choice = choices.Find(p => p.Property == (info.Value as JArray)[0].ToObject<string>());
                                if (choice != null)
                                {
                                    // Resolve and move to SetProperty
                                    recognized.Add(info);
                                    infos.Clear();
                                    queues.ChooseProperty.Dequeue();
                                    choice.Expected = true;
                                    queues.SetProperty.Add(choice);

                                    // TODO: This seems a little draconian, but we don't want property names to trigger help
                                    context.GetState().SetValue("turn.recognized.intent", "None");
                                    context.GetState().SetValue("turn.recognized.score", 1.0);
                                }
                            }

                            break;
                        }
                }
            }

            return recognized;
        }

        // A big issue is that we want multiple firings.  We can get this from quantification, but not arrays.
        // If we have a rule for value ambiguity we would want it to fire for each value ambiguity.
        // Possibly:
        // * Iterate through ambiguous text and run rule?
        // * Iterate through each ambiguous entity and collect firing rules.
        // * Run rules on remaining
        // Prefer handlers by:
        // * Set & Expected propertys
        // * Set & Coverage
        // * Set & Priority
        // * Disambiguation & expected
        // * Disambiguation & coverage
        // * Disambiguation & priority
        // * Prompt

        // We have four kinds of ambiguity to deal with:
        // * Value: Ambiguous interpretation of entity value: (peppers -> [green peppers, red peppers]  Tell this by entity value is array.  Doesn't matter if property singleton or not. Ask.
        // * Text: Ambiguous interpretion of text: (3 -> age or number) Identify by overlapping entities. Resolve by greater coverage, expected entity, ask.
        // * Singelton: two different entities which could fill property singleton.  Could be same type or different types.  Resolve by rule priority.
        // * Slot: Which property should an entity go to?  Resolve by expected, then ask.
        // Should rules by over entities directly or should we process them first into these forms?
        // This is also complicated by singleton vs. array
        // It would be nice if multiple entities were rolled up into a single entity, i.e. a toppings composite with topping inside of it.
        // Rule for value ambiguity: foreach(entity in @entity) entity is array.    
        // Rule for text ambiguity: info overlaps...
        // Rule for singleton ambiguity: multiple rules fire over different entities
        // Rule for property ambiguity: multiple rules fire for same entity
        // Preference is for expected properties
        // Want to write rules that:
        // * Allow mapping a property through steps.
        // * Allow disambiguation
        // * More specific win from trigger tree
        // * Easy to understand
        // How to deal with multiple entities.
        // * Rules are over them all--some of which have ambiguity
        // * Rules are specific to individual entity.  Easier to write, but less powerful and lots of machinery for singleton/array
        //
        // Key assumptions:
        // * A single entity type maps to a single property.  Otherwise we have to figure out how to name different entity instances.
        //
        // Need to figure out how to handle operations.  They could be done in LUIS as composites which allow putting together multiples ones. 
        // You can imagine doing add/remove, but another scenario would be to do "change seattle to dallas" where you are referring to where 
        // a specific value is found independent of which property has the value.
        //
        // 1) @@entity to entities array
        // 2) Use schema information + expected to assign each entity to one of: choice(property), clarify(property), unknown, properties and remove any overlapping entities.
        // 3) Run rules to pick one rule for doing next.  They are in terms of the processing queues and other memory.
        // On the next cycle go ahead and add to process queues
        // Implied in this is that mapping information consists of simple paths to entities.
        // Choice[property] = [[entity, ...]]
        // Clarify[property] = [entity, ...]
        // Slots = [{entity, [properties]}]
        // Unknown = [entity, ...]
        // Set = [{entity, property, op}]
        // For rules, prefer non-forminput, then forminput.

        // Combine all the information we have about entities
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(SequenceContext context)
        {
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();
            var text = context.GetState().GetValue<string>(TurnPath.RECOGNIZED + ".text");
            if (context.GetState().TryGetValue<dynamic>(TurnPath.RECOGNIZED + ".entities", out var entities))
            {
                // TODO: We should have RegexRecognizer return $instance or make this robust to it missing, i.e. assume no entities overlap
                var turn = context.GetState().GetValue<uint>(DialogPath.EventCounter);
                var metaData = entities["$instance"];
                foreach (var entry in entities)
                {
                    var name = entry.Name;
                    if (!name.StartsWith("$"))
                    {
                        var values = entry.Value;
                        var instances = metaData[name];
                        for (var i = 0; i < values.Count; ++i)
                        {
                            var val = values[i];
                            var instance = instances[i];
                            if (!entityToInfo.TryGetValue(name, out List<EntityInfo> infos))
                            {
                                infos = new List<EntityInfo>();
                                entityToInfo[name] = infos;
                            }

                            var info = new EntityInfo
                            {
                                Turn = turn,
                                Name = name,
                                Value = val,
                                Start = (int)instance.startIndex,
                                End = (int)instance.endIndex,
                                Text = (string)instance.text,
                                Type = (string)instance.type,
                                Role = (string)instance.role,
                                Score = (double)(instance.score ?? 0.0d),
                            };

                            // Eventually this could be passed in
                            info.Priority = info.Role == null ? 1 : 0;
                            info.Coverage = (info.End - info.Start) / (double)text.Length;
                            infos.Add(info);
                        }
                    }
                }
            }

            // TODO: This should not be necessary--LUIS should be picking the maximal match
            foreach (var infos in entityToInfo.Values)
            {
                infos.Sort((e1, e2) =>
                {
                    var val = 0;
                    if (e1.Start == e2.Start)
                    {
                        if (e1.End > e2.End)
                        {
                            val = -1;
                        }
                        else if (e1.End < e2.End)
                        {
                            val = +1;
                        }
                    }
                    else if (e1.Start < e2.Start)
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
        private IEnumerable<EntityToProperty> Candidates(Dictionary<string, List<EntityInfo>> entities, string[] expected)
        {
            var expectedOnly = Schema.Schema["$expectedOnly"]?.ToObject<List<string>>() ?? new List<string>();
            foreach (var propSchema in Schema.Property.Children)
            {
                var isExpected = expected.Contains(propSchema.Path);
                if (isExpected || !expectedOnly.Contains(propSchema.Path))
                {
                    foreach (var entityName in propSchema.Mappings)
                    {
                        if (entities.TryGetValue(entityName, out var matches))
                        {
                            foreach (var entity in matches)
                            {
                                yield return new EntityToProperty
                                {
                                    Entity = entity,
                                    Schema = propSchema,
                                    Property = propSchema.Path,

                                    // TODO: Eventually we should be able to pick up an add/remove composite here as an alternative
                                    Operation = Operations.Add,
                                    Expected = isExpected
                                };
                            }
                        }
                    }
                }
            }
        }

        private void AddMappingToQueue(EntityToProperty mapping, EventQueues queues)
        {
            if (mapping.Entity.Value is JArray arr)
            {
                if (arr.Count > 1)
                {
                    queues.ClarifyEntity.Add(mapping);
                }
                else
                {
                    mapping.Entity.Value = arr[0];
                    queues.SetProperty.Add(mapping);
                }
            }
            else
            {
                queues.SetProperty.Add(mapping);
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
        private IEnumerable<EntityToProperty> RemoveOverlappingPerProperty(IEnumerable<EntityToProperty> candidates)
        {
            var perProperty = from candidate in candidates
                              group candidate by candidate.Schema;
            foreach (var propChoices in perProperty)
            {
                var schema = propChoices.Key;
                var choices = propChoices.ToList();

                // Assume preference by order listed in mappings
                // Alternatives would be to look at coverage or other metrices
                foreach (var entity in schema.Mappings)
                {
                    EntityToProperty candidate;
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

        private List<EntityInfo> AddToQueues(Dictionary<string, List<EntityInfo>> entities, string[] expected, EventQueues queues)
        {
            var candidates = (from candidate in RemoveOverlappingPerProperty(Candidates(entities, expected))
                              orderby candidate.Expected descending
                              select candidate).ToList();
            var usedEntities = new HashSet<EntityInfo>(from candidate in candidates select candidate.Entity);
            while (candidates.Any())
            {
                var candidate = candidates.First();
                var alternatives = (from alt in candidates where candidate.Entity.Overlaps(alt.Entity) select alt).ToList();
                candidates = candidates.Except(alternatives).ToList();
                if (candidate.Expected)
                {
                    // If expected binds entity, drop alternatives
                    alternatives.RemoveAll(a => !a.Expected);
                }

                foreach (var alternative in alternatives)
                {
                    usedEntities.Add(alternative.Entity);
                }

                if (alternatives.Count() == 1)
                {
                    AddMappingToQueue(candidate, queues);
                }
                else
                {
                    queues.ChooseProperty.Add(alternatives);
                }
            }

            return (from entity in usedEntities orderby entity.Start ascending select entity).ToList();
        }

        private EventQueues PropertyQueues(string path, Dictionary<PropertySchema, EventQueues> slotToQueues)
        {
            var prop = Schema.PathToSchema(path);
            if (!slotToQueues.TryGetValue(prop, out var slotQueues))
            {
                slotQueues = new EventQueues();
                slotToQueues[prop] = slotQueues;
            }

            return slotQueues;
        }

        // Create queues for each property
        private Dictionary<PropertySchema, EventQueues> PerPropertyQueues(EventQueues queues)
        {
            var propertyToQueues = new Dictionary<PropertySchema, EventQueues>();
            foreach (var entry in queues.SetProperty)
            {
                PropertyQueues(entry.Property, propertyToQueues).SetProperty.Add(entry);
            }

            foreach (var entry in queues.ClarifyEntity)
            {
                PropertyQueues(entry.Property, propertyToQueues).ClarifyEntity.Add(entry);
            }

            foreach (var entry in queues.ClearProperty)
            {
                PropertyQueues(entry, propertyToQueues).ClearProperty.Add(entry);
            }

            foreach (var entry in queues.ChooseProperty)
            {
                foreach (var choice in entry)
                {
                    PropertyQueues(choice.Property, propertyToQueues).ChooseProperty.Add(entry);
                }
            }

            return propertyToQueues;
        }

        private void CombineNewEntityToPropertys(EventQueues queues)
        {
            var slotToQueues = PerPropertyQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var property = entry.Key;
                var slotQueues = entry.Value;
                if (!property.IsArray && slotQueues.SetProperty.Count() + slotQueues.ClarifyEntity.Count() > 1)
                {
                    // Singleton with multiple operations
                    var mappings = from mapping in slotQueues.SetProperty.Union(slotQueues.ClarifyEntity) where mapping.Operation != Operations.Remove select mapping;
                    switch (mappings.Count())
                    {
                        case 0:
                            queues.ClearProperty.Add(property.Path);
                            break;
                        case 1:
                            AddMappingToQueue(mappings.First(), queues);
                            break;
                        default:
                            // TODO: Map to multiple entity to property
                            /* queues.ChooseProperty.Add(new EntitiesToProperty
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

        private void CombineOldEntityToPropertys(EventQueues queues, uint turn)
        {
            var slotToQueues = PerPropertyQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var property = entry.Key;
                var slotQueues = entry.Value;
                if (!property.IsArray &&
                    (slotQueues.SetProperty.Any(e => e.Entity.Turn == turn)
                    || slotQueues.ClarifyEntity.Any(e => e.Entity.Turn == turn)
                    || slotQueues.ChooseProperty.Any(c => c.First().Entity.Turn == turn)))
                {
                    // Remove all old operations on property because there is a new one
                    foreach (var mapping in slotQueues.SetProperty)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.SetProperty.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.ClarifyEntity)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.ClarifyEntity.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.ChooseProperty)
                    {
                        if (mapping.First().Entity.Turn != turn)
                        {
                            queues.ChooseProperty.Remove(mapping);
                        }
                    }
                }
            }
        }

        // Assign entities to queues
        private List<EntityInfo> AssignEntities(Dictionary<string, List<EntityInfo>> entities, string[] expected, EventQueues queues)
        {
            var recognized = AddToQueues(entities, expected, queues);
            CombineNewEntityToPropertys(queues);
            return recognized;
        }

        // For simple singleton property:
        //  Set values
        //      count(@@foo) == 1 -> foo == @foo
        //      count(@@foo) > 1 -> "Which {@@foo} do you want for {slotName}"
        //  Constraints (which are more specific)
        //      count(@@foo) == 1 && @foo < 0 -> "{@foo} is too small for {slotname}"
        //      count(@@foo) > 1 && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        // For simple array property:
        //  Set values:
        //      @@foo -> foo = @@foo
        //  Constraints: (which are more specific)
        //      @@foo && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0) are too small for {slotname}"
        //  Modification--based on intent?
        //      add: @@foo && @intent == add -> Append(@@foo, foo)
        //      // This is to make this more specific than both the simple constraint and the intent
        //      add: @@foo && count(where(@@foo, foo, foo < 0)) > 0 && @intent == add -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        //      delete: @@foo @intent == delete -> Delete(@@foo, foo)
        // For structured singleton property
        //  count(@@foo) == 1 -> Apply child constraints, i.e. make a new singleton object to apply child property rule sets to it.
        //  count(@@foo) > 1 -> "Which one did you want?" with replacing @@foo with the singleton selection
        //
        // Children properties can either:
        // * Refer to parent structure which turns into count(first(parent).child) == 1
        // * Refer to independent entity, i.e. count(datetime) > 1
        //
        // Assumptions:
        // * In order to map structured entities to structured properties, parent structures must be singletons before child can map them.
        // * We will only generate a single instance of the form.  (Although there can be multiple ones inside.)
        // * You can map directly, but then must deal with that complexity of structures.  For example if you have multiple flight(origin, destination) and you want to map to hotel(location)
        //   you have to figure out how to deal with multiple flight structures and the underlying entity structures.
        // * For now only leaves can be arrays.  If you need more, I think it is a subform, but we could probably automatically generate a foreach step on top.
        //
        // 1) Find all most specific matches
        // 2) Identify any properties that compete for the same entity.  Select by in expected, then keep as property ambiguous.
        // 3) For each entity either: a) Do its set, b) queue up clarification, c) surface as unhandled
        // 
        // Two cases:
        // 1) Flat entity resolution, treat properties as independent.
        // 2) Hierarchical, the first level you get to count(@@flight) == 1, then for count(first(@@flight).origin) == 1
        // We know which is which by entity path, i.e. flight.origin -> hierarchical whereas origin is flat.
        //
        // In order to robustly handle we need a progression of transformations, i.e. to map @meat to meatSlot singleton:
        // @meat -> meatSlot_choice (m->1) ->
        //                          (1->1) -> foreach meatslot_clarify -> set meat property (clears others)
        // If we get a new @meat, then it would reset them all.
        // Should this be a flat set of rules?

        // If one @@entity then goes to foreach
    }
}
