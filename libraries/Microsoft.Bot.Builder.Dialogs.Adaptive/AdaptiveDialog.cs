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

        private const string AdaptiveKey = "_adaptive";

        // unique key for language generator turn property, (TURN STATE ONLY)
        private readonly string generatorTurnKey = Guid.NewGuid().ToString();

        // unique key for change tracking of the turn state (TURN STATE ONLY)
        private readonly string changeTurnKey = Guid.NewGuid().ToString();

        private bool installedDependencies;

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

            SetLocalGenerator(dc.Context);

            // replace initial activeDialog.State with clone of options
            if (options != null)
            {
                dc.ActiveDialog.State = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(options));
            }

            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            activeDialogState[AdaptiveKey] = new AdaptiveDialogState();
            var state = activeDialogState[AdaptiveKey] as AdaptiveDialogState;

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

        protected async Task<bool> ProcessEventAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default)
        {
            // Save into turn
            sequenceContext.GetState().SetValue(TurnPath.DIALOGEVENT, dialogEvent);

            EnsureDependenciesInstalled();

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
                            sequenceContext.GetState().SetValue(TurnPath.TOPSCORE, score);

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

                // Check for changes to any of our parents
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
            // End dialog and return result
            if (sequenceContext.ActiveDialog != null)
            {
                if (ShouldEnd(sequenceContext))
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
                var evt = Triggers[selection.First()];
                await sequenceContext.DebuggerStepAsync(evt, dialogEvent, cancellationToken).ConfigureAwait(false);
                Trace.TraceInformation($"Executing Dialog: {Id} Rule[{selection}]: {evt.GetType().Name}: {evt.GetExpression(new ExpressionEngine())}");
                var changes = await evt.ExecuteAsync(sequenceContext).ConfigureAwait(false);

                if (changes != null && changes.Count > 0)
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

                    foreach (var trigger in Triggers)
                    {
                        if (trigger is IDialogDependencies depends)
                        {
                            foreach (var dlg in depends.GetDependencies())
                            {
                                Dialogs.Add(dlg);
                            }
                        }
                    }

                    // Wire up selector
                    if (Selector == null)
                    {
                        // Default to most specific then first
                        Selector = new MostSpecificSelector { Selector = new FirstSelector() };
                    }

                    Selector.Initialize(Triggers);
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
    }
}
