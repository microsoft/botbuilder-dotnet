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
    public class AdaptiveDialog : Dialog
    {
        private bool installedDependencies = false;
        protected readonly DialogSet dialogs = new DialogSet();
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
                dialogs.TelemetryClient = client;
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
            activeDialogState["planningState"] = new PlanningState();
            var state = activeDialogState["planningState"] as PlanningState;

            // Persist options to dialog state
            state.Options = options ?? new Dictionary<string, object>();

            // Initialize 'result' with any initial value
            if (state.Options.GetType() == typeof(Dictionary<string, object>) && (state.Options as Dictionary<string, object>).ContainsKey("value"))
            {
                state.Result = state.Options["value"];
            }

            // Create a new planning context
            var planning = PlanningContext.Create(dc, state);


            if (this.Steps.Any())
            {
                // use this.Steps as initial plan
                var changeList = new PlanChangeList();
                foreach (var step in this.Steps)
                {
                    changeList.Steps.Add(new PlanStepState(step.Id, options));
                }

                planning.QueueChanges(changeList);
            }
            else
            {
                // Evaluate rules and queue up plan changes
                await EvaluateRulesAsync(planning, new DialogEvent() { Name = AdaptiveEvents.BeginDialog.ToString(), Value = options, Bubble = false }, cancellationToken).ConfigureAwait(false);
            }

            // Run plan
            return await ContinuePlanAsync(planning, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogConsultation> ConsultDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Create a new planning context
            var activeStateMap = (dc.ActiveDialog.State as Dictionary<string, object>);
            var state = activeStateMap["planningState"] as PlanningState;
            state.Changes = new List<PlanChangeList>();

            var planning = PlanningContext.Create(dc, state);

            // First consult plan
            var consultation = await ConsultPlanAsync(planning, cancellationToken).ConfigureAwait(false);

            if (consultation == null || consultation.Desire != DialogConsultationDesire.ShouldProcess)
            {
                // Next evaluate rules
                var changesQueued = await EvaluateRulesAsync(planning, new DialogEvent() { Name = AdaptiveEvents.ConsultDialog.ToString(), Value = null, Bubble = false }, cancellationToken).ConfigureAwait(false);
                if (changesQueued)
                {
                    if (consultation == null || planning.Changes[0].Desire == DialogConsultationDesire.ShouldProcess)
                    {
                        consultation = new DialogConsultation()
                        {
                            Desire = planning.Changes[0].Desire,
                            Processor = (ctx) => this.ContinuePlanAsync(planning, cancellationToken)
                        };
                    }
                    else
                    {
                        state.Changes = new List<PlanChangeList>();
                    }
                }

                // Fallback to just continuing the plan
                if (consultation == null)
                {
                    consultation = new DialogConsultation()
                    {
                        Desire = DialogConsultationDesire.CanProcess,
                        Processor = (ctx) => this.ContinuePlanAsync(planning, cancellationToken)
                    };
                }
            }

            return consultation;
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

        public override async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            // Create a new planning context
            var state = (dc.ActiveDialog.State as Dictionary<string, object>)["planningState"] as PlanningState;
            var planning = PlanningContext.Create(dc, state);

            // Evaluate rules and queue up any potential changes
            return await EvaluateRulesAsync(planning, e, cancellationToken).ConfigureAwait(false);
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
            var state = (instance.State as Dictionary<string, object>)["planningState"] as PlanningState;
            var plan = state.Plan;

            if (plan?.Steps.Count > 0)
            {
                // We need to mockup a DialogContext so that we can call RepromptDialog
                // for the active step
                var stepDc = new DialogContext(dialogs, turnContext, plan.Steps[0], new Dictionary<string, object>(), new Dictionary<string, object>());
                await stepDc.RepromptDialogAsync().ConfigureAwait(false);
            }
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forwards cancellation to sequences
            if (reason == DialogReason.CancelCalled)
            {
                var state = (instance.State as Dictionary<string, object>)["planningState"] as PlanningState;

                if (state.Plan != null)
                {
                    await CancelPlanAsync(turnContext, state.Plan);
                    state.Plan = null;
                }

                if (state.SavedPlans != null)
                {
                    for (int i = 0; i < state.SavedPlans.Count; i++)
                    {
                        await CancelPlanAsync(turnContext, state.SavedPlans[i]).ConfigureAwait(false);
                    }

                    state.SavedPlans = null;

                }
            }
        }

        private async Task CancelPlanAsync(ITurnContext context, PlanState plan, CancellationToken cancellationToken = default(CancellationToken))
        {
            for (int i = 0; i < plan.Steps.Count; i++)
            {
                // We need to mock up a dialog context so that EndDialogAsync() can be called on any active steps
                var stepDc = new DialogContext(dialogs, context, plan.Steps[i], new Dictionary<string, object>(), new Dictionary<string, object>());
                await stepDc.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public void AddRule(IRule rule)
        {
            rule.Steps.ForEach(s => dialogs.Add(s));
            this.Rules.Add(rule);
        }

        public void AddRules(IEnumerable<IRule> rules)
        {
            foreach (var rule in rules)
            {
                this.AddRule(rule);
            }
        }

        public IDialog FindDialog(string dialogId) => dialogs.Find(dialogId);

        public void AddDialog(IEnumerable<IDialog> dialogs)
        {
            foreach (var dialog in dialogs)
            {
                this.dialogs.Add(dialog);
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

            // Execute component
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

        public async Task<DialogTurnResult> Run(ITurnContext context, PlanningDialogRunOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            options = options ?? new PlanningDialogRunOptions();

            // Initialize bot state
            var botState = options.BotState;

            if (botState != null)
            {
                if (botState.DialogStack == null)
                {
                    botState.DialogStack = new List<DialogInstance>();
                }

                if (botState.ConversationState == null)
                {
                    botState.ConversationState = new Dictionary<string, object>();
                }
            }
            else if (this.BotState != null)
            {
                botState = await this.BotState.GetAsync(context, () => new BotState { ConversationState = new Dictionary<string, object>(), DialogStack = new List<DialogInstance>() }).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("PlanningDialog.Run(): method called without a 'botState'. Set the 'PlanningDialog.BotState' property or pass in the state to use.");
            }

            // Initialize user state
            var userState = options.UserState;

            if (userState == null)
            {
                if (this.UserState != null)
                {
                    userState = await this.UserState.GetAsync(context, () => new Dictionary<string, object>()).ConfigureAwait(false);
                }
                else if (botState.UserState == null)
                {
                    botState.UserState = new Dictionary<string, object>();
                    userState = botState.UserState;
                }
            }

            // Check for expiration
            var now = DateTime.UtcNow;

            if (options.ExpireAfter.HasValue && !string.IsNullOrEmpty(botState.LastAccess))
            {
                var lastAccess = DateTime.Parse(botState.LastAccess);

                if (now - lastAccess >= TimeSpan.FromMilliseconds(options.ExpireAfter.Value))
                {
                    // Clear stack and conversation state
                    botState.DialogStack = new List<DialogInstance>();
                    botState.ConversationState = new Dictionary<string, object>();
                }
            }
            botState.LastAccess = now.ToString();

            // Create dialog context
            var dc = new DialogContext(this.runDialogs, context, botState, botState.ConversationState, userState);

            // Attempt to continue execution of the component's current dialog
            var result = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

            // Start the component if it wasn't already running
            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dc.BeginDialogAsync(this.Id, options.DialogOptions).ConfigureAwait(false);
            }

            return result;
        }

        protected async Task<DialogConsultation> ConsultPlanAsync(PlanningContext planning, CancellationToken cancellationToken)
        {
            // Apply any queued up changes
            await planning.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);

            // Delegate consultation to any active planning step
            var step = PlanningContext.CreateForStep(planning, dialogs);
            if (step != null)
            {
                var consultation = await step.ConsultDialogAsync();
                return new DialogConsultation()
                {
                    Desire = consultation?.Desire ?? DialogConsultationDesire.CanProcess,
                    Processor = async (dc) =>
                    {
                        // set context
                        if (this.Generator != null)
                        {
                            dc.Context.TurnState.Set<ILanguageGenerator>(this.Generator);
                        }

                        // Continue current step
                        var result = consultation != null ? await consultation.Processor(step).ConfigureAwait(false) : new DialogTurnResult(DialogTurnStatus.Empty);

                        if (result.Status == DialogTurnStatus.Empty && !result.ParentEnded)
                        {
                            var nextStep = step.Plan.Steps[0];
                            result = await step.BeginDialogAsync(nextStep.DialogId, nextStep.Options).ConfigureAwait(false);
                        }

                        // Process step results
                        if (!result.ParentEnded)
                        {
                            // end the current step
                            if (result.Status != DialogTurnStatus.Waiting)
                            {
                                await planning.EndStepAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                            }

                            // do we have any queued up changes
                            if (planning.Changes?.Count > 0)
                            {
                                // apply changes and continue execution
                                return await this.ContinuePlanAsync(planning, cancellationToken: cancellationToken).ConfigureAwait(false);
                            }

                            // if step is waiting?
                            if (result.Status == DialogTurnStatus.Waiting)
                            {
                                return result;
                            }

                            // Continue plan execution
                            var plan = planning.Plan;

                            if (plan != null && plan.Steps.Count > 0 && plan?.Steps[0].DialogStack?.Count > 0)
                            {
                                // Tell step to re-prompt
                                await RepromptDialogAsync(dc.Context, dc.ActiveDialog).ConfigureAwait(false);
                                return new DialogTurnResult(DialogTurnStatus.Waiting);
                            }
                            else
                            {
                                return await ContinuePlanAsync(planning, cancellationToken: cancellationToken).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            // Remove parent ended flag and return result
                            result.ParentEnded = false;
                            return result;
                        }
                    }
                };

            }
            else
            {
                return null;
            }
        }

        protected async Task<DialogTurnResult> ContinuePlanAsync(PlanningContext planning, CancellationToken cancellationToken)
        {
            // Consult plan and execute returned processor
            var consultation = await this.ConsultPlanAsync(planning, cancellationToken).ConfigureAwait(false);
            if (consultation != null)
            {
                return await consultation.Processor(planning).ConfigureAwait(false);
            }
            else
            {
                return await OnEndOfPlan(planning);
            }
        }

        protected virtual async Task<DialogTurnResult> OnEndOfPlan(PlanningContext planning)
        {
            // End dialog and return default result
            if (planning.ActiveDialog != null)
            {
                if (this.AutoEndDialog)
                {
                    var state = (planning.ActiveDialog.State as Dictionary<string, object>)["planningState"] as PlanningState;
                    return await planning.EndDialogAsync(state?.Result).ConfigureAwait(false);
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

        protected async Task<RecognizerResult> OnRecognize(PlanningContext planning, CancellationToken cancellationToken = default(CancellationToken))
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

        protected virtual async Task<bool> EvaluateRulesAsync(PlanningContext planning, DialogEvent dialogEvent, CancellationToken cancellationToken)
        {
            // save into turn
            planning.State.SetValue($"turn.dialogEvent", dialogEvent);
            planning.State.SetValue($"turn.dialogEvents.{dialogEvent.Name}", dialogEvent.Value);

            var handled = false;

            if (!handled)
            {
                if (dialogEvent.Name == AdaptiveEvents.BeginDialog.ToString())
                {
                    // Emit event
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);

                    if (!handled)
                    {
                        // Process activityReceived event
                        handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = AdaptiveEvents.ActivityReceived.ToString(), Value = planning.Context.Activity, Bubble = false }, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (dialogEvent.Name == AdaptiveEvents.ConsultDialog.ToString())
                {
                    // Emit event
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);

                    if (!handled)
                    {
                        // Process activityReceived event
                        handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = AdaptiveEvents.ActivityReceived.ToString(), Value = planning.Context.Activity, Bubble = false }, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (dialogEvent.Name == AdaptiveEvents.ActivityReceived.ToString())
                {
                    // Emit event
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);

                    if (!handled)
                    {
                        var activity = planning.Context.Activity;

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance
                            var recognized = await this.OnRecognize(planning).ConfigureAwait(false);

                            // Emit UtteranceRecognized evnet
                            handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = AdaptiveEvents.RecognizedIntent.ToString(), Value = recognized, Bubble = false }, cancellationToken).ConfigureAwait(false);
                        }
                        else if (activity.Type == ActivityTypes.Event)
                        {
                            // Dispatch named event that was received
                            handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = activity.Name, Value = activity.Value, Bubble = false }, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    return handled;
                }
                else if (dialogEvent.Name == AdaptiveEvents.RecognizedIntent.ToString())
                {
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);

                    if (!handled)
                    {
                        // Dispatch fallback event
                        handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = AdaptiveEvents.UnknownIntent.ToString(), Value = dialogEvent.Value, Bubble = false }, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (dialogEvent.Name == AdaptiveEvents.StepsStarted.ToString())
                {
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);
                }
                else if (dialogEvent.Name == AdaptiveEvents.StepsEnded.ToString())
                {
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);
                }
                else if (dialogEvent.Name == AdaptiveEvents.UnknownIntent.ToString())
                {
                    if (planning.Plan == null)
                    {
                        handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    handled = await QueueFirstMatchAsync(planning, dialogEvent, cancellationToken).ConfigureAwait(false);
                }
            }

            return handled;
        }

        private async Task<bool> QueueFirstMatchAsync(PlanningContext planning, DialogEvent dialogEvent, CancellationToken cancellationToken)
        {
            var selection = await Selector.Select(planning, cancellationToken).ConfigureAwait(false);
            if (selection.Any())
            {
                var rule = Rules[selection.First()];
                await planning.DebuggerStepAsync(rule, dialogEvent, cancellationToken).ConfigureAwait(false);
                System.Diagnostics.Trace.TraceInformation($"Executing Dialog: {this.Id} Rule[{selection}]: {rule.GetType().Name}: {rule.GetExpression(null)}");
                var changes = await rule.ExecuteAsync(planning).ConfigureAwait(false);

                if (changes != null && changes.Count > 0)
                {
                    planning.QueueChanges(changes[0]);
                    return true;
                }
            }
            return false;
        }
    }
}
