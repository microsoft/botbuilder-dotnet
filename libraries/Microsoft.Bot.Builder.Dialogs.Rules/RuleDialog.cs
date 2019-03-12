using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public class PlanningDialogRunOptions
    {
        public BotState BotState { get; set; }

        public object DialogOptions { get; set; }

        public int? ExpireAfter { get; set; }

        public StateMap UserState { get; set; }
    }

    public class StoredBotState
    {
        public StateMap UserState { get; set; }
        public StateMap ConversationState { get; set; }
        public List<DialogInstance> DialogStack { get; set; }
    }

    public class BotStateStorageKeys
    {
        public string UserState { get; set; }
        public string ConversationState { get; set; }
        public string DialogState { get; set; }
    }

    public class BotTurnResult
    {
        public DialogTurnResult TurnResult { get; set; }
        public List<Activity> Activities { get; set; }
        public StoredBotState NewState { get; set; }
    }

    public class RuleDialog : Dialog
    {
        private bool installedDependencies = false;
        protected readonly DialogSet dialogs = new DialogSet();
        protected DialogSet runDialogs = new DialogSet(); // Used by the Run method

        public virtual List<IRule> Rules { get; set; } = new List<IRule>();

        public IStatePropertyAccessor<BotState> BotState { get; set; }

        public IStatePropertyAccessor<StateMap> UserState { get; set; }

        public IRecognizer Recognizer { get; set; }

        public IStorage Storage { get; set; }

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

        public RuleDialog(string dialogId = null) 
            : base(dialogId)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!installedDependencies)
            {
                installedDependencies = true;

                Rules.ForEach(r => AddDialog(r.Steps.ToArray()));
            }

            var activeDialogState = dc.ActiveDialog.State as StateMap;
            activeDialogState["planningState"] = new PlanningState();
            var state = activeDialogState["planningState"] as PlanningState;

            // Persist options to dialog state
            state.Options = options ?? new StateMap();

            // Initialize 'result' with any initial value
            if (state.Options.GetType() == typeof(StateMap) && (state.Options as StateMap).ContainsKey("value"))
            {
                state.Result = state.Options["value"];
            }

            // Create a new planning context
            var planning = PlanningContext.Create(dc, state);

            // Evaluate rules and queue up plan changes
            await EvaluateRulesAsync(planning, new DialogEvent() { Name = PlanningEvents.BeginDialog.ToString(), Value = options, Bubble = false }).ConfigureAwait(false);

            // Run plan
            return await ContinuePlanAsync(planning).ConfigureAwait(false);
        }

        public override async Task<DialogConsultation> ConsultDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Create a new planning context
            var activeStateMap = (dc.ActiveDialog.State as StateMap);
            var state = activeStateMap["planningState"] as PlanningState;

            var planning = PlanningContext.Create(dc, state);

            // First consult plan
            var consultation = await ConsultPlanAsync(planning).ConfigureAwait(false);

            if (consultation == null || consultation.Desire != DialogConsultationDesires.ShouldPrcess)
            {
                // Next evaluate rules
                var changesQueued = await EvaluateRulesAsync(planning, new DialogEvent() { Name = PlanningEvents.ConsultDialog.ToString(), Value = null, Bubble = false }).ConfigureAwait(false);

                if (changesQueued)
                {
                    consultation = new DialogConsultation()
                    {
                        Desire = DialogConsultationDesires.ShouldPrcess,
                        Processor = (ctx) => this.ContinuePlanAsync(planning)
                    };
                }

                // Fallback to just continuing the plan
                if (consultation == null)
                {
                    consultation = new DialogConsultation()
                    {
                        Desire = DialogConsultationDesires.CanProcess,
                        Processor = (ctx) => this.ContinuePlanAsync(planning)
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
                UserState = data.ContainsKey(keys.UserState) ? data[keys.UserState] as StateMap : new StateMap(),
                ConversationState = data.ContainsKey(keys.ConversationState) ? data[keys.ConversationState] as StateMap : new StateMap(),
                DialogStack = data.ContainsKey(keys.DialogState) ? data[keys.DialogState] as List<DialogInstance> : new List<DialogInstance>(),
            };
        }

        private static async Task SaveBotState(IStorage storage, StoredBotState newState, BotStateStorageKeys keys)
        {
            await storage.WriteAsync(new Dictionary<string, object>()
            {
                { keys.UserState, newState.UserState},
                { keys.ConversationState, newState.ConversationState},
                { keys.DialogState, newState.DialogStack}
            });
        }

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

        public override async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e)
        {
            // Create a new planning context
            var state = (dc.ActiveDialog.State as StateMap)["planningState"] as PlanningState;
            var planning = PlanningContext.Create(dc, state);

            // Evaluate rules and queue up any potential changes
            return await EvaluateRulesAsync(planning, e).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
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
            var state = instance.State as PlanningState;
            var plan = state.Plan;

            if (plan?.Steps.Count > 0)
            {
                // We need to mockup a DialogContext so that we can call RepromptDialog
                // for the active step
                var stepDc = new DialogContext(dialogs, turnContext, plan.Steps[0], new StateMap(), new StateMap());
                await stepDc.RepromptDialogAsync().ConfigureAwait(false);
            }
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forwards cancellation to sequences
            if (reason == DialogReason.CancelCalled)
            {
                var state = instance.State as PlanningState;

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
                var stepDc = new DialogContext(dialogs, context, plan.Steps[i], new StateMap(), new StateMap());
                await stepDc.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public void AddRule(IEnumerable<IRule> rules)
        {
            foreach (var rule in rules)
            {
                rule.Steps.ForEach(s => dialogs.Add(s));
                this.Rules.Add(rule);
            }
        }

        public IDialog FindDialog(string dialogId)
        {
            return dialogs.Find(dialogId);
        }

        public void AddDialog(IEnumerable<IDialog> dialogs)
        {
            foreach (var dialog in dialogs)
            {
                this.dialogs.Add(dialog);
            }
        }

        protected override string OnComputeId()
        {
            return $"planning[{this.BindingPath()}]";
        }

        public async Task<BotTurnResult> OnTurnAsync(ITurnContext context, StoredBotState storedState, CancellationToken cancellationToken = default(CancellationToken))
        {
            var saveState = false;
            var keys = ComputeKeys(context);

            if (storedState == null)
            {
                if (Storage != null)
                {
                    storedState = await LoadBotState(Storage, keys).ConfigureAwait(false);
                    saveState = true;
                }
            }

            if (runDialogs.GetDialogs().Count() == 0)
            {
                // Create DialogContext
                this.runDialogs.Add(this);

                foreach (var rule in Rules)
                {
                    rule.Steps.ForEach(s => dialogs.Add(s));
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
                storedState.ConversationState,
                storedState.UserState);

            // Execute component
            var result = await dc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dc.BeginDialogAsync(this.Id, cancellationToken).ConfigureAwait(false);
            }

            if (saveState)
            {
                await SaveBotState(Storage, storedState, keys).ConfigureAwait(false);
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
                    botState.ConversationState = new StateMap();
                }
            }
            else if (this.BotState != null)
            {
                botState = await this.BotState.GetAsync(context, () => new BotState { ConversationState = new StateMap(), DialogStack = new List<DialogInstance>() }).ConfigureAwait(false);
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
                    userState = await this.UserState.GetAsync(context, () => new StateMap()).ConfigureAwait(false);
                }
                else if (botState.UserState == null)
                {
                    botState.UserState = new StateMap();
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
                    botState.ConversationState = new StateMap();
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

        protected async Task<DialogConsultation> ConsultPlanAsync(PlanningContext planning)
        {
            // Apply any queued up changes
            await planning.ApplyChangesAsync().ConfigureAwait(false);

            // Delegate consultation to any active planning step
            var step = PlanningContext.CreateForStep(planning, dialogs);
            var consultation = step != null ? await step.ConsultDialogAsync() : null;

            return new DialogConsultation()
            {
                Desire = consultation?.Desire ?? DialogConsultationDesires.CanProcess,
                Processor = async (dc) =>
                {
                    if (step != null)
                    {
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
                            // Is step waiting?
                            if (result.Status == DialogTurnStatus.Waiting)
                            {
                                return result;
                            }

                            // Was step cancelled?
                            if (result.Status == DialogTurnStatus.Cancelled)
                            {
                                // End the current plan
                                await planning.EndPlanAsync().ConfigureAwait(false);
                            }
                            else
                            {
                                // End the current step
                                await planning.EndStepAsync().ConfigureAwait(false);
                            }

                            // Continue plan execution
                            var plan = planning.Plan;

                            if (plan?.Steps[0].DialogStack?.Count > 0)
                            {
                                // Tell step to re-prompt
                                await RepromptDialogAsync(dc.Context, dc.ActiveDialog).ConfigureAwait(false);
                                return new DialogTurnResult(DialogTurnStatus.Waiting);
                            }
                            else
                            {
                                return await ContinuePlanAsync(planning).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            // Remove parent ended flag and return result
                            result.ParentEnded = false;
                            return result;
                        }
                    }
                    else
                    {
                        // End dialog and return default result
                        var state = planning.ActiveDialog.State as PlanningState;
                        return await planning.EndDialogAsync(state?.Result).ConfigureAwait(false);
                    }
                }
            };
        }

        protected async Task<DialogTurnResult> ContinuePlanAsync(PlanningContext planning)
        {
            // Consult plan and execute returned processor
            var consultation = await this.ConsultPlanAsync(planning).ConfigureAwait(false);
            return await consultation.Processor(planning).ConfigureAwait(false);
        }

        protected async Task<RecognizerResult> OnRecognize(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var noneIntent = new RecognizerResult()
            {
                Text = context.Activity.Text ?? string.Empty,
                Intents = new Dictionary<string, IntentScore>()
                {
                    { "None", new IntentScore() { Score = 0.0} }
                },
                Entities = JObject.Parse("{}")
            };

            return Recognizer != null ? await Recognizer.RecognizeAsync(context, cancellationToken) : noneIntent; 
        }

        protected virtual async Task<bool> EvaluateRulesAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            var handled = false;

            if (!handled)
            {
                if (dialogEvent.Name == PlanningEvents.BeginDialog.ToString() ||
                    dialogEvent.Name == PlanningEvents.ConsultDialog.ToString())
                {
                    // Emit event
                    handled = await QueueFirstMatchAsync(planning, dialogEvent).ConfigureAwait(false);

                    if (!handled)
                    {
                        // Process activityReceived event
                        handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = PlanningEvents.ActivityReceived.ToString(), Value = null, Bubble = false }).ConfigureAwait(false);
                    }
                }
                else if (dialogEvent.Name == PlanningEvents.ActivityReceived.ToString())
                {
                    // Emit event
                    handled = await QueueFirstMatchAsync(planning, dialogEvent).ConfigureAwait(false);

                    if (!handled)
                    {
                        var activity = planning.Context.Activity;

                        if (activity.Type == ActivityTypes.Message)
                        {
                            // Recognize utterance
                            var recognized = await this.OnRecognize(planning.Context).ConfigureAwait(false);

                            // Emit UtteranceRecognized evnet
                            handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = PlanningEvents.UtteranceRecognized.ToString(), Value = recognized, Bubble = false }).ConfigureAwait(false);
                        }
                        else if (activity.Type == ActivityTypes.Event)
                        {
                            // Dispatch named event that was received
                            handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = activity.Name, Value = activity.Value, Bubble = false }).ConfigureAwait(false);
                        }
                    }
                    return handled;
                }
                else if (dialogEvent.Name == PlanningEvents.UtteranceRecognized.ToString())
                {
                    handled = await QueueBestMatches(planning, dialogEvent).ConfigureAwait(false);

                    if (!handled)
                    {
                        // Dispatch fallback event
                        handled = await EvaluateRulesAsync(planning, new DialogEvent() { Name = PlanningEvents.Fallback.ToString(), Value = dialogEvent.Value, Bubble = false }).ConfigureAwait(false);
                    }
                }
                else if (dialogEvent.Name == PlanningEvents.Fallback.ToString())
                {
                    if (planning.Plan == null)
                    {
                        handled = await QueueFirstMatchAsync(planning, dialogEvent).ConfigureAwait(false);
                    }
                }
                else
                {
                    handled = await QueueFirstMatchAsync(planning, dialogEvent).ConfigureAwait(false);
                }
            }

            return handled;
        }

        private async Task<bool> QueueFirstMatchAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                var changes = await Rules[i].EvaluateAsync(planning, dialogEvent).ConfigureAwait(false);

                if (changes != null && changes.Count > 0)
                {
                    planning.QueueChanges(changes[0]);
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> QueueBestMatches(PlanningContext planning, DialogEvent dialogEvent)
        {
            // Get list of proposed changes
            var allChanges = new List<PlanChangeList>();

            for (int i = 0; i < Rules.Count; i++)
            {
                var changes = await Rules[i].EvaluateAsync(planning, dialogEvent).ConfigureAwait(false);

                if (changes != null)
                {
                    changes.ForEach(c => allChanges.Add(c));
                }
            }

            // Find changes with most coverage
            var appliedChanges = new List<Tuple<int, PlanChangeList>>();

            if (allChanges.Count > 0)
            {
                while (true)
                {
                    // Find the change that has the most intents and entities covered
                    var index = FindBestChange(allChanges);

                    if (index == -1)
                    {
                        break;
                    }
                    else
                    {
                        // Add change to apply list
                        var change = allChanges[index];
                        appliedChanges.Add(new Tuple<int, PlanChangeList>(index, change));

                        // Remove applied changes
                        allChanges.RemoveAt(index);


                        // Remove changes with overlapping intents
                        for (int i = allChanges.Count - 1; i >= 0; i--)
                        {
                            if (IntentsOverlap(change, allChanges[i]))
                            {
                                allChanges.RemoveAt(i);
                            }
                        }
                    }
                }
            }

            // Queue changes
            if (appliedChanges.Count > 0)
            {
                var sorted = appliedChanges.OrderBy(c => c.Item1).ToList();

                if (sorted.Count > 0)
                {
                    // Look for the first change that starts a new plan
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var changeType = sorted[i].Item2.ChangeType;

                        if (changeType == PlanChangeTypes.NewPlan || changeType == PlanChangeTypes.ReplacePlan)
                        {
                            // Queue change and remove from list
                            planning.QueueChanges(sorted[i].Item2);
                            sorted.RemoveAt(i);
                            break;
                        }
                    }

                    // Queue additional changes
                    // Additional NewPlan or ReplacePlan steps will be changed to a DoStepsLater
                    // change type so that they're appended to the new plan
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var change = sorted[i].Item2;

                        switch (change.ChangeType)
                        {
                            case PlanChangeTypes.DoSteps:
                            case PlanChangeTypes.DoStepsBeforeTags:
                            case PlanChangeTypes.DoStepsLater:
                                planning.QueueChanges(change);
                                break;
                            case PlanChangeTypes.NewPlan:
                            case PlanChangeTypes.ReplacePlan:
                                change.ChangeType = PlanChangeTypes.DoStepsLater;
                                planning.QueueChanges(change);
                                break;
                        }
                    }
                }
                else
                {
                    // Just queue the change
                    planning.QueueChanges(sorted[0].Item2);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private int FindBestChange(List<PlanChangeList> changes)
        {
            PlanChangeList top = null;
            var topIndex = -1;

            for (int i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                var better = false;

                if (top == null)
                {
                    better = true;
                }
                else
                {
                    var topIntents = top.IntentsMatched ?? new List<string>();
                    var intents = change.IntentsMatched ?? new List<string>();

                    if (intents.Count > topIntents.Count)
                    {
                        better = true;
                    }
                    else if (intents.Count == topIntents.Count)
                    {
                        var topEntities = top.EntitiesMatched ?? new List<string>();
                        var entities = change.EntitiesMatched ?? new List<string>();
                        better = entities.Count > topEntities.Count;
                    }
                }

                if (better)
                {
                    top = change;
                    topIndex = i;
                }
            }

            return topIndex;
        }

        private bool IntentsOverlap(PlanChangeList c1, PlanChangeList c2)
        {
            var i1 = c1.IntentsMatched ?? new List<string>();
            var i2 = c2.IntentsMatched ?? new List<string>();

            if (i2.Count > 0 && i1.Count > 0)
            {
                for (int i = 0; i < i2.Count; i++)
                {
                    if (i1.IndexOf(i2[i]) >= 0)
                    {
                        return true;
                    }
                }
            }
            else if (i2.Count == i1.Count)
            {
                return true;
            }

            return false;
        }
    }
}
