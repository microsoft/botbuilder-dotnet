// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Rules;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Dialogs.Debugging.DebugSupport;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{

    /// <summary>
    /// The Adaptive Dialog models conversation using events and rules to adapt dynamicaly to changing conversation flow
    /// </summary>
    public class AdaptiveDialog : DialogContainer
    {
        private string changeKey = Guid.NewGuid().ToString();
        private const string ADAPTIVE_KEY = "adaptiveDialogState";

        private bool installedDependencies = false;

        protected DialogSet runDialogs = new DialogSet(); // Used by the Run method

        public IStatePropertyAccessor<BotState> BotState { get; set; }

        public IStatePropertyAccessor<Dictionary<string, object>> UserState { get; set; }

        /// <summary>
        /// Recognizer for processing incoming user input 
        /// </summary>
        public IRecognizer Recognizer { get; set; }

        /// <summary>
        /// Language Generator override
        /// </summary>
        public ILanguageGenerator Generator { get; set; }

        /// <summary>
        /// Gets or sets the steps to execute when the dialog begins
        /// </summary>
        public List<IDialog> Steps { get; set; } = new List<IDialog>();

        /// <summary>
        /// Rules for handling events to dynamic modifying the executing plan 
        /// </summary>
        public virtual List<IRule> Rules { get; set; } = new List<IRule>();

        /// <summary>
        /// Gets or sets the policty to Automatically end the dialog when there are no steps to execute
        /// </summary>
        /// <remarks>
        /// If true, when there are no steps to execute the current dialog will end
        /// If false, when there are no steps to execute the current dialog will simply end the turn and still be active
        /// </remarks>
        public bool AutoEndDialog { get; set; } = true;

        /// <summary>
        /// Gets or sets the selector for picking the possible rules to execute.
        /// </summary>
        public IRuleSelector Selector { get; set; }

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

        public AdaptiveDialog(string dialogId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogId)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {

            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} should not ever be a cancellation token");
            }

            lock (this)
            {
                if (!installedDependencies)
                {
                    installedDependencies = true;

                    AddDialog(this.Steps.ToArray());

                    foreach (var rule in this.Rules)
                    {
                        AddDialog(rule.Steps.ToArray());
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
                    this.Selector.Initialize(this.Rules, true);
                }
            }

            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            activeDialogState[ADAPTIVE_KEY] = new AdaptiveDialogState();
            var state = activeDialogState[ADAPTIVE_KEY] as AdaptiveDialogState;

            // Persist options to dialog state
            state.Options = options ?? new Dictionary<string, object>();

            // Initialize 'result' with any initial value
            if (state.Options.GetType() == typeof(Dictionary<string, object>) && (state.Options as Dictionary<string, object>).ContainsKey("value"))
            {
                state.Result = state.Options["value"];
            }

            // Evaluate rules and queue up step changes
            var dialogEvent = new DialogEvent()
            {
                Name = AdaptiveEvents.BeginDialog,
                Value = options,
                Bubble = false
            };

            await this.OnDialogEventAsync(dc, dialogEvent, cancellationToken).ConfigureAwait(false);

            // Continue step execution
            return await this.ContinueStepsAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Continue step execution
            return await ContinueStepsAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<bool> OnPreBubbleEvent(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sequence = this.ToSequenceContext(dc);

            // Process event and queue up any potential interruptions
            return await this.ProcessEventAsync(sequence, dialogEvent, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<bool> OnPostBubbleEvent(DialogContext dc, DialogEvent dialogEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sequence = this.ToSequenceContext(dc);

            // Process event and queue up any potential interruptions
            return await this.ProcessEventAsync(sequence, dialogEvent, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected async Task<bool> ProcessEventAsync(SequenceContext sequence, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Save into turn
            sequence.State.SetValue($"turn.dialogEvent", dialogEvent);
            sequence.State.SetValue($"turn.dialogEvents.{dialogEvent.Name}", dialogEvent.Value);

            // Look for triggered rule
            var handled = await this.QueueFirstMatchAsync(sequence, dialogEvent, preBubble, cancellationToken).ConfigureAwait(false);

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
                        if (this.Steps.Any())
                        {
                            // Initialize plan with steps
                            var changes = new StepChangeList()
                            {
                                ChangeType = StepChangeTypes.InsertSteps,
                                Steps = new List<StepState>()
                            };

                            this.Steps.ForEach(
                                s => changes.Steps.Add(
                                    new StepState()
                                    {
                                        DialogId = s.Id,
                                        DialogStack = new List<DialogInstance>()
                                    }));

                            sequence.QueueChanges(changes);
                            handled = true;
                        }
                        else
                        {
                            // Emit leading ActivityReceived event
                            var e = new DialogEvent() { Name = AdaptiveEvents.ActivityReceived, Bubble = false };
                            handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case AdaptiveEvents.ActivityReceived:

                        var activity = sequence.Context.Activity;

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Clear any recognizer results
                            sequence.State.SetValue("turn.recognized", null);

                            // Recognize utterance
                            var recognized = await this.OnRecognize(sequence, cancellationToken).ConfigureAwait(false);

                            sequence.State.SetValue("turn.recognized", recognized);

                            // Emit leading RecognizedIntent event
                            var e = new DialogEvent() { Name = AdaptiveEvents.RecognizedIntent, Value = recognized, Bubble = false };
                            handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }
                        else if (activity.Type == ActivityTypes.Event)
                        {
                            // Emit trailing edge of named event that was received
                            var e = new DialogEvent() { Name = activity.Name, Value = activity.Value, Bubble = false };
                            handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        else if (activity.Type == ActivityTypes.ConversationUpdate)
                        {
                            var membersAdded = sequence.State.GetValue<List<ChannelAccount>>("turn.membersAdded", null);
                            if (membersAdded != null && membersAdded.Any())
                            {
                                // Emit trailing ConversationMembersAdded event
                                var e = new DialogEvent() { Name = AdaptiveEvents.ConversationMembersAdded, Value = membersAdded, Bubble = false };
                                handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
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
                        var e = new DialogEvent() { Name = AdaptiveEvents.ActivityReceived, Bubble = false };
                        handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);

                        break;

                    case AdaptiveEvents.ActivityReceived:

                        var activity = sequence.Context.Activity;
                        var membersAdded = sequence.State.GetValue<List<ChannelAccount>>("turn.membersAdded", null);

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Clear recognizer results
                            sequence.State.SetValue("turn.recognized", null);


                            // Empty sequence?
                            if (!sequence.Steps.Any())
                            {
                                // Emit trailing unknownIntent event
                                e = new DialogEvent() { Name = AdaptiveEvents.UnknownIntent, Bubble = false };
                                handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                handled = false;
                            }
                        }
                        else if (activity.Type == ActivityTypes.Event)
                        {
                            // Emit trailing edge of named event that was received
                            e = new DialogEvent() { Name = activity.Name, Value = activity.Value, Bubble = false };
                            handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }
                        else if (activity.Type == ActivityTypes.ConversationUpdate && membersAdded != null && membersAdded.Any())
                        {
                            // Emit trailing conversation members added event
                            e = new DialogEvent() { Name = activity.Name, Value = activity.Value, Bubble = false };
                            handled = await this.ProcessEventAsync(sequence, dialogEvent: e, preBubble: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                        }

                        break;
                }
            }

            return handled;
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

            if (state.Steps.Any())
            {
                // We need to mockup a DialogContext so that we can call RepromptDialog
                // for the active step
                var stepDc = new DialogContext(_dialogs, turnContext, state.Steps[0], new Dictionary<string, object>(), new Dictionary<string, object>());
                await stepDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public void AddRule(IRule rule)
        {
            rule.Steps.ForEach(s => _dialogs.Add(s));
            this.Rules.Add(rule);
        }

        public void AddRules(IEnumerable<IRule> rules)
        {
            foreach (var rule in rules)
            {
                this.AddRule(rule);
            }
        }

        public void AddDialog(IEnumerable<IDialog> dialogs)
        {
            foreach (var dialog in dialogs)
            {
                this._dialogs.Add(dialog);
            }
        }

        protected override string OnComputeId()
        {
            if (DebugSupport.SourceRegistry.TryGetValue(this, out var range))
            {
                return $"AdaptiveDialog({Path.GetFileName(range.Path)}:{range.Start.LineIndex})";
            }
            return $"AdaptiveDialog[{this.BindingPath()}]";
        }

        public async Task<BotTurnResult> OnTurnAsync(ITurnContext context, StoredBotState storedState, CancellationToken cancellationToken = default(CancellationToken))
        {
            var saveState = false;
            var keys = ComputeKeys(context);
            var storage = context.TurnState.Get<IStorage>();

            if (storedState == null)
            {
                storedState = await LoadBotState(storage, keys).ConfigureAwait(false);
                saveState = true;
            }

            lock (runDialogs)
            {
                if (runDialogs.GetDialogs().Count() == 0)
                {
                    // Create DialogContext
                    this.runDialogs.Add(this);
                }
            }

            var dc = new DialogContext(runDialogs,
                context,
                new DialogState()
                {
                    ConversationState = storedState.ConversationState,
                    UserState = storedState.UserState,
                    DialogStack = storedState.DialogStack
                },
                conversationState: storedState.ConversationState,
                userState: storedState.UserState);

            // Dispatch ActivityReceived event
            // This will queue up any interruptions
            await dc.EmitEventAsync(AdaptiveEvents.ActivityReceived, value: null, bubble: true, fromLeaf: true, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Continue execution
            // This will apply any queued up interruptions and execute the current / next step(s)
            var result = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dc.BeginDialogAsync(this.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (saveState)
            {
                await SaveBotState(storage, storedState, keys).ConfigureAwait(false);
                return new BotTurnResult()
                {
                    TurnResult = result,
                };
            }
            else
            {
                return new BotTurnResult()
                {
                    TurnResult = result,
                    NewState = storedState,
                };
            }
        }

        private static async Task<StoredBotState> LoadBotState(IStorage storage, BotStateStorageKeys keys)
        {
            var data = await storage.ReadAsync(new[] { keys.UserState, keys.ConversationState, keys.DialogState }).ConfigureAwait(false);

            return new StoredBotState()
            {
                UserState = data.ContainsKey(keys.UserState) ? data[keys.UserState] as Dictionary<string, object> : new Dictionary<string, object>(),
                ConversationState = data.ContainsKey(keys.ConversationState) ? data[keys.ConversationState] as Dictionary<string, object> : new Dictionary<string, object>(),
                DialogStack = data.ContainsKey(keys.DialogState) ? data[keys.DialogState] as List<DialogInstance> : new List<DialogInstance>(),
            };
        }

        private static async Task SaveBotState(IStorage storage, StoredBotState newState, BotStateStorageKeys keys) => await storage.WriteAsync(new Dictionary<string, object>()
            {
                { keys.UserState, newState.UserState},
                { keys.ConversationState, newState.ConversationState},
                { keys.DialogState, newState.DialogStack}
            });

        private static BotStateStorageKeys ComputeKeys(ITurnContext context)
        {
            // Get channel, user and conversation ids
            var activity = context.Activity;
            var channelId = activity.ChannelId;
            var userId = activity.From?.Id;
            var conversationId = activity.Conversation?.Id;

            // Patch user id if needed
            if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                var members = activity.MembersAdded ?? activity.MembersRemoved ?? new List<ChannelAccount>();
                var nonRecipients = members.Where(m => m.Id != activity.Recipient.Id);
                var found = userId != null ? nonRecipients.FirstOrDefault(r => r.Id == userId) : null;

                if (found == null && members.Count > 0)
                {
                    userId = nonRecipients.FirstOrDefault()?.Id ?? userId;
                }
            }

            // Verify ids were found
            if (userId == null)
            {
                throw new Exception("PlanningDialog: unable to load the bots state.The users ID couldn't be found.");
            }

            if (conversationId == null)
            {
                throw new Exception("PlanningDialog: unable to load the bots state. The conversations ID couldn't be found.");
            }

            // Return storage keys
            return new BotStateStorageKeys()
            {
                UserState = $"{channelId}/users/{userId}",
                ConversationState = $"{channelId}/conversations/{conversationId}",
                DialogState = $"{channelId}/dialog/{conversationId}",
            };
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

            if (state.Steps != null && state.Steps.Any())
            {
                var ctx = new SequenceContext(this._dialogs, dc, state.Steps.First(), state.Steps, changeKey);
                ctx.Parent = dc;
                return ctx;
            }
            
            return null;
        }

        private string GetUniqueInstanceId(DialogContext dc)
        {
            return dc.Stack.Count > 0 ? $"{dc.Stack.Count}:{dc.ActiveDialog.Id}" : string.Empty;
        }

        protected async Task<DialogTurnResult> ContinueStepsAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // Apply any queued up changes
            var sequence = this.ToSequenceContext(dc);
            await sequence.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);

            if (this.Generator != null)
            {
                dc.Context.TurnState.Set<ILanguageGenerator>(this.Generator);
            }

            // Get a unique instance ID for the current stack entry.
            // We need to do this because things like cancellation can cause us to be removed
            // from the stack and we want to detect this so we can stop processing steps.
            var instanceId = this.GetUniqueInstanceId(sequence);

            var step = this.CreateChildContext(sequence) as SequenceContext;

            if (step != null)
            {
                // Continue current step
                var result = await step.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

                // Start step if not continued
                if (result.Status == DialogTurnStatus.Empty && GetUniqueInstanceId(sequence) == instanceId)
                {
                    var nextStep = step.Steps.First();
                    result = await step.BeginDialogAsync(nextStep.DialogId, nextStep.Options, cancellationToken).ConfigureAwait(false);
                }

                // Increment turns step count
                // This helps dialogs being resumed from an interruption to determine if they
                // should re-prompt or not.
                var stepCount = sequence.State.GetValue<int>("turn.stepCount", 0);
                sequence.State.SetValue("turn.stepCount", stepCount + 1);

                // Is the step waiting for input or were we cancelled?
                if (result.Status == DialogTurnStatus.Waiting || this.GetUniqueInstanceId(sequence) != instanceId)
                {
                    return result;
                }

                // End current step
                await this.EndCurrentStepAsync(sequence, cancellationToken).ConfigureAwait(false);

                // Execute next step
                // We call continueDialog() on the root dialog to ensure any changes queued up
                // by the previous steps are applied.
                DialogContext root = sequence;
                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                return await root.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await this.OnEndOfStepsAsync(sequence, cancellationToken).ConfigureAwait(false);
            }
        }

        protected async Task<bool> EndCurrentStepAsync(SequenceContext sequence, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequence.Steps.Any())
            {
                sequence.Steps.RemoveAt(0);

                if (!sequence.Steps.Any())
                {
                    await sequence.EmitEventAsync(AdaptiveEvents.SequenceEnded, value: null, bubble: false, fromLeaf: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            return false;
        }

        protected async Task<DialogTurnResult> OnEndOfStepsAsync(SequenceContext sequence, CancellationToken cancellationToken = default(CancellationToken))
        {
            // End dialog and return result
            if (sequence.ActiveDialog != null)
            {
                if (this.ShouldEnd(sequence))
                {
                    var state = (sequence.ActiveDialog.State as Dictionary<string, object>)[ADAPTIVE_KEY] as AdaptiveDialogState;
                    return await sequence.EndDialogAsync(state.Result, cancellationToken).ConfigureAwait(false);
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

        protected async Task<RecognizerResult> OnRecognize(SequenceContext planning, CancellationToken cancellationToken = default(CancellationToken))
        {
            var context = planning.Context;
            if (Recognizer != null)
            {
                await planning.DebuggerStepAsync(Recognizer, DialogContext.DialogEvents.OnRecognize, cancellationToken).ConfigureAwait(false);
                var result = await Recognizer.RecognizeAsync(context, cancellationToken).ConfigureAwait(false);
                // only allow one intent 
                var topIntent = result.GetTopScoringIntent();
                result.Intents.Clear();
                result.Intents.Add(topIntent.intent, new IntentScore() { Score = topIntent.score });
                return result;
            }
            else
            {
                return new RecognizerResult()
                {
                    Text = context.Activity.Text ?? string.Empty,
                    Intents = new Dictionary<string, IntentScore>()
                    {
                        { "None", new IntentScore() { Score = 0.0} }
                    },
                    Entities = JObject.Parse("{}")
                };

            }
        }

        private async Task<bool> QueueFirstMatchAsync(SequenceContext sequence, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken)
        {
            var selection = await Selector.Select(sequence, cancellationToken).ConfigureAwait(false);
            if (selection.Any())
            {
                var rule = Rules[selection.First()];
                await sequence.DebuggerStepAsync(rule, dialogEvent, cancellationToken).ConfigureAwait(false);
                System.Diagnostics.Trace.TraceInformation($"Executing Dialog: {this.Id} Rule[{selection}]: {rule.GetType().Name}: {rule.GetExpression(null)}");
                var changes = await rule.ExecuteAsync(sequence).ConfigureAwait(false);

                if (changes != null && changes.Count > 0)
                {
                    sequence.QueueChanges(changes[0]);
                    return true;
                }
            }
            return false;
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

            if (state.Steps == null)
            {
                state.Steps = new List<StepState>();
            }

            var sequenceContext = new SequenceContext(dc.Dialogs, dc, new DialogState() { DialogStack = dc.Stack }, state.Steps, changeKey);
            sequenceContext.Parent = dc.Parent;
            return sequenceContext;
        }
    }
}
