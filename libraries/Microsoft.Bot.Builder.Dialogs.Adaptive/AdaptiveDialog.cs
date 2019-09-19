// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static Microsoft.Bot.Builder.Dialogs.Debugging.DebugSupport;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// The Adaptive Dialog models conversation using events and events to adapt dynamicaly to changing conversation flow.
    /// </summary>
    public class AdaptiveDialog : DialogContainer
    {
#pragma warning disable SA1310 // Field should not contain underscore.
        private const string ADAPTIVE_KEY = "adaptiveDialogState";
#pragma warning restore SA1310 // Field should not contain underscore.

        private readonly string changeKey = Guid.NewGuid().ToString();

        private bool installedDependencies = false;

        public AdaptiveDialog(string dialogId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogId)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public IStatePropertyAccessor<BotState> BotState { get; set; }

        public IStatePropertyAccessor<Dictionary<string, object>> UserState { get; set; }

        /// <summary>
        /// Gets or sets recognizer for processing incoming user input.
        /// </summary>
        public IRecognizer Recognizer { get; set; }

        /// <summary>
        /// Gets or sets language Generator override.
        /// </summary>
        public ILanguageGenerator Generator { get; set; }

        /// <summary>
        /// Gets or sets trigger handlers to respond to conditions which modifying the executing plan. 
        /// </summary>
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
        public bool AutoEndDialog { get; set; } = true;

        /// <summary>
        /// Gets or sets the selector for picking the possible events to execute.
        /// </summary>
        /// <value>
        /// The selector for picking the possible events to execute.
        /// </value>
        public ITriggerSelector Selector { get; set; }

        /// <summary>
        /// Gets or sets the property to return as the result when the dialog ends when there are no more Actions and AutoEndDialog = true.
        /// </value>
        public string DefaultResultProperty { get; set; } = "dialog.result";

        /// <summary>
        /// Gets the dialogs which make up the AdaptiveDialog 
        /// </summary>
        public DialogSet Dialogs => this._dialogs;

        public override IBotTelemetryClient TelemetryClient
        {
            get
            {
                return base.TelemetryClient;
            }

            set
            {
                var client = value ?? new NullBotTelemetryClient();
                _dialogs.TelemetryClient = client;
                base.TelemetryClient = client;
            }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} should not ever be a cancellation token");
            }

            EnsureDependenciesInstalled();

            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            activeDialogState[ADAPTIVE_KEY] = new AdaptiveDialogState();
            var state = activeDialogState[ADAPTIVE_KEY] as AdaptiveDialogState;

            // Persist options to dialog state
            dc.State.SetValue(ThisPath.OPTIONS, options);

            // Evaluate events and queue up step changes
            var dialogEvent = new DialogEvent()
            {
                Name = AdaptiveEvents.BeginDialog,
                Value = options,
                Bubble = false
            };

            await this.OnDialogEventAsync(dc, dialogEvent, cancellationToken).ConfigureAwait(false);

            // Continue step execution
            return await this.ContinueActionsAsync(dc, options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureDependenciesInstalled();

            // Continue step execution
            return await ContinueActionsAsync(dc, null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
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

            return Dialog.EndOfTurn;
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forward to current sequence step
            var state = (instance.State as Dictionary<string, object>)[ADAPTIVE_KEY] as AdaptiveDialogState;

            if (state.Actions.Any())
            {
                // We need to mockup a DialogContext so that we can call RepromptDialog
                // for the active step
                var stepDc = new DialogContext(_dialogs, turnContext, state.Actions[0]);
                await stepDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public override DialogContext CreateChildContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            var state = activeDialogState[ADAPTIVE_KEY] as AdaptiveDialogState;

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[ADAPTIVE_KEY] = state;
            }

            if (state.Actions != null && state.Actions.Any())
            {
                var ctx = new SequenceContext(this._dialogs, dc, state.Actions.First(), state.Actions, changeKey, this._dialogs);
                ctx.Parent = dc;
                return ctx;
            }

            return null;
        }

        protected override async Task<bool> OnPreBubbleEvent(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sequenceContext = this.ToSequenceContext(dc);

            // Process event and queue up any potential interruptions
            return await this.ProcessEventAsync(sequenceContext, dialogEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<bool> OnPostBubbleEvent(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sequenceContext = this.ToSequenceContext(dc);

            // Process event and queue up any potential interruptions
            return await this.ProcessEventAsync(sequenceContext, dialogEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected async Task<bool> ProcessEventAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Save into turn
            sequenceContext.State.SetValue(TurnPath.DIALOGEVENT, dialogEvent);

            // Look for triggered evt
            var handled = await this.QueueFirstMatchAsync(sequenceContext, dialogEvent, preBubble, cancellationToken).ConfigureAwait(false);

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
                        // Emit leading ActivityReceived event
                        var activityReceivedEvent = new DialogEvent() { Name = AdaptiveEvents.ActivityReceived, Value = sequenceContext.Context.Activity, Bubble = false };
                        handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: activityReceivedEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        break;

                    case AdaptiveEvents.ActivityReceived:

                        var activity = sequenceContext.Context.Activity;

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance
                            var recognized = await this.OnRecognize(sequenceContext, cancellationToken).ConfigureAwait(false);

                            sequenceContext.State.SetValue(TurnPath.RECOGNIZED, recognized);

                            var (name, score) = recognized.GetTopScoringIntent();
                            sequenceContext.State.SetValue(TurnPath.TOPINTENT, name);
                            sequenceContext.State.SetValue(TurnPath.TOPSCORE, score);

                            if (this.Recognizer != null)
                            {
                                await sequenceContext.DebuggerStepAsync(Recognizer, AdaptiveEvents.RecognizedIntent, cancellationToken).ConfigureAwait(false);
                            }

                            // Emit leading RecognizedIntent event
                            var recognizedIntentEvent = new DialogEvent() { Name = AdaptiveEvents.RecognizedIntent, Value = recognized, Bubble = false };
                            handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: recognizedIntentEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;
                }
            }
            else
            {
                switch (dialogEvent.Name)
                {
                    case AdaptiveEvents.BeginDialog:
                        var activityReceivedEvent = new DialogEvent() { Name = AdaptiveEvents.ActivityReceived, Value = sequenceContext.Context.Activity, Bubble = false };
                        handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: activityReceivedEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);

                        break;

                    case AdaptiveEvents.ActivityReceived:

                        var activity = sequenceContext.Context.Activity;

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Empty sequence?
                            if (!sequenceContext.Actions.Any())
                            {
                                // Emit trailing unknownIntent event
                                var unknownIntentEvent = new DialogEvent() { Name = AdaptiveEvents.UnknownIntent, Bubble = false };
                                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: unknownIntentEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                handled = false;
                            }
                        }

                        break;
                }
            }

            return handled;
        }

        protected override string OnComputeId()
        {
            if (DebugSupport.SourceRegistry.TryGetValue(this, out var range))
            {
                return $"{this.GetType().Name}({Path.GetFileName(range.Path)}:{range.Start.LineIndex})";
            }

            return $"{this.GetType().Name}[]";
        }

        protected async Task<DialogTurnResult> ContinueActionsAsync(DialogContext dc, object options, CancellationToken cancellationToken)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException("You cannot pass a cancellation token as options");
            }

            // Apply any queued up changes
            var sequenceContext = this.ToSequenceContext(dc);
            await sequenceContext.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);

            if (this.Generator != null)
            {
                dc.Context.TurnState.Set<ILanguageGenerator>(this.Generator);
            }

            // Get a unique instance ID for the current stack entry.
            // We need to do this because things like cancellation can cause us to be removed
            // from the stack and we want to detect this so we can stop processing actions.
            var instanceId = this.GetUniqueInstanceId(sequenceContext);

            var action = this.CreateChildContext(sequenceContext) as SequenceContext;

            if (action != null)
            {
                // Continue current step
                var result = await action.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

                // Start step if not continued
                if (result.Status == DialogTurnStatus.Empty && GetUniqueInstanceId(sequenceContext) == instanceId)
                {
                    var nextAction = action.Actions.First();

                    // Compute options object for the step
                    object effectiveOptions = ComputeEffectiveOptions(options, nextAction.Options);

                    // Call begin dialog on our next step, passing the effective options we computed
                    result = await action.BeginDialogAsync(nextAction.DialogId, effectiveOptions, cancellationToken).ConfigureAwait(false);
                }

                // Increment turns step count
                // This helps dialogs being resumed from an interruption to determine if they
                // should re-prompt or not.
                var stepCount = sequenceContext.State.GetValue<int>(TurnPath.STEPCOUNT, () => 0);
                sequenceContext.State.SetValue(TurnPath.STEPCOUNT, stepCount + 1);

                // Is the step waiting for input or were we cancelled?
                if (result.Status == DialogTurnStatus.Waiting || this.GetUniqueInstanceId(sequenceContext) != instanceId)
                {
                    return result;
                }

                // End current step
                await this.EndCurrentActionAsync(sequenceContext, cancellationToken).ConfigureAwait(false);

                // Execute next step
                // We call continueDialog() on the root dialog to ensure any changes queued up
                // by the previous actions are applied.
                DialogContext root = sequenceContext;
                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                return await root.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await this.OnEndOfActionsAsync(sequenceContext, cancellationToken).ConfigureAwait(false);
            }
        }

        protected Task<bool> EndCurrentActionAsync(SequenceContext sequenceContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceContext.Actions.Any())
            {
                sequenceContext.Actions.RemoveAt(0);
            }

            return Task.FromResult(false);
        }

        protected async Task<DialogTurnResult> OnEndOfActionsAsync(SequenceContext sequenceContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // End dialog and return result
            if (sequenceContext.ActiveDialog != null)
            {
                if (this.ShouldEnd(sequenceContext))
                {
                    sequenceContext.State.TryGetValue<object>(DefaultResultProperty, out var result);
                    return await sequenceContext.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return Dialog.EndOfTurn;
                }
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Cancelled);
            }
        }

        protected async Task<RecognizerResult> OnRecognize(SequenceContext sequenceContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var context = sequenceContext.Context;
            var noneIntent = new RecognizerResult()
            {
                Text = context.Activity.Text ?? string.Empty,
                Intents = new Dictionary<string, IntentScore>()
                    {
                        { "None", new IntentScore() { Score = 0.0 } }
                    },
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
                    var recognized = new RecognizerResult() { Text = string.Empty };

                    foreach (var property in value.Properties())
                    {
                        if (property.Name.ToLower() == "intent")
                        {
                            recognized.Intents[property.Value.ToString()] = new IntentScore() { Score = 1.0 };
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
                result.Intents.Add(topIntent.intent, new IntentScore() { Score = topIntent.score });
                return result;
            }
            else
            {
                return noneIntent;
            }
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
                var evt = Triggers[selection.First()];
                await sequenceContext.DebuggerStepAsync(evt, dialogEvent, cancellationToken).ConfigureAwait(false);
                System.Diagnostics.Trace.TraceInformation($"Executing Dialog: {this.Id} Rule[{selection}]: {evt.GetType().Name}: {evt.GetExpression(new ExpressionEngine())}");
                var changes = await evt.ExecuteAsync(sequenceContext).ConfigureAwait(false);

                if (changes != null && changes.Count > 0)
                {
                    sequenceContext.QueueChanges(changes[0]);
                    return true;
                }
            }

            return false;
        }

        private object ComputeEffectiveOptions(object adaptiveOptions, object stepOptions)
        {
            var effectiveOptions = adaptiveOptions;

            if (effectiveOptions == null)
            {
                // If no options were passed in from the adaptive dialog, just use the step's option
                effectiveOptions = stepOptions;
            }
            else if (stepOptions != null)
            {
                // If we were passed in options and also have non-null options for the next step,
                // overlay the step options on top of the adaptive options 
                ObjectPath.Assign<object>(effectiveOptions, stepOptions);
            }

            return effectiveOptions;
        }

        private void EnsureDependenciesInstalled()
        {
            lock (this)
            {
                if (!installedDependencies)
                {
                    installedDependencies = true;

                    foreach (var @event in this.Triggers)
                    {
                        if (@event is IDialogDependencies depends)
                        {
                            foreach (var dlg in depends.GetDependencies())
                            {
                                this.Dialogs.Add(dlg);
                            }
                        }
                    }

                    // Wire up selector
                    if (this.Selector == null)
                    {
                        // Default to most specific then first
                        this.Selector = new MostSpecificSelector
                        {
                            Selector = new FirstSelector()
                        };
                    }

                    this.Selector.Initialize(this.Triggers, true);
                }
            }
        }

        private bool ShouldEnd(DialogContext dc)
        {
            return this.AutoEndDialog;
        }

        private SequenceContext ToSequenceContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            var state = activeDialogState[ADAPTIVE_KEY] as AdaptiveDialogState;

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[ADAPTIVE_KEY] = state;
            }

            if (state.Actions == null)
            {
                state.Actions = new List<ActionState>();
            }

            var sequenceContext = new SequenceContext(dc.Dialogs, dc, new DialogState() { DialogStack = dc.Stack }, state.Actions, changeKey, this._dialogs);
            sequenceContext.Parent = dc.Parent;
            return sequenceContext;
        }
    }
}
