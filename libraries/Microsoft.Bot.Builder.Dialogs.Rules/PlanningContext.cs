using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public class PlanningContext : DialogContext
    {
        public PlanningState Plans { get; private set; }

        /// <summary>
        /// The current plan being executed (if any.)
        /// </summary>
        public PlanState Plan => Plans.Plan;

        /// <summary>
        /// Returns true if there are saved plans.
        /// </summary>
        public bool HasSavedPlans => Plans.SavedPlans?.Count > 0;

        public PlanningContext(DialogContext dc, DialogContext parentDc, DialogSet dialogs, DialogState state, PlanningState plans)
            : base(dialogs, dc.Context, state, conversationState: dc.State.Conversation, userState: dc.State.User, settings: dc.State.Settings)
        {
            this.Parent = parentDc;
            this.Plans = plans ?? throw new ArgumentNullException(nameof(plans));
        }

        /// <summary>
        /// Queues up a set of plan changes that will be applied when ApplyChanges is called.
        /// </summary>
        /// <param name="changes">Plan changes to queue up</param>
        public void QueueChanges(PlanChangeList changes)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            if (Plans.Changes == null)
            {
                Plans.Changes = new List<PlanChangeList>();
            }

            Plans.Changes.Add(changes);
        }

        /// <summary>
        /// Applies any queued up changes.
        /// </summary>
        /// <remarks>
        /// Applying a set of changes can result in additional plan changes being queued. The method
        /// will loop and apply any additional plan changes until there are no more changes left to 
        /// apply.
        /// </remarks>
        /// <returns>True if there were any changes to apply. </returns>
        public async Task<bool> ApplyChangesAsync()
        {
            var changes = Plans.Changes;

            if (changes != null)
            {
                // Reset plan changes
                Plans.Changes = null;

                // Apply each queued set of changes
                foreach (var change in changes)
                {
                    if (change.EntitiesRecognized != null && change.EntitiesRecognized.Count > 0)
                    {
                        var entities = this.State.Entities;
                        foreach(var name in change.EntitiesRecognized.Keys)
                        {
                            if (!entities.ContainsKey(name))
                            {
                                entities.Add(name, change.EntitiesRecognized[name]);
                            }
                            else
                            {
                                entities[name] = change.EntitiesRecognized[name];
                            }
                        }
                    }

                    switch (change.ChangeType)
                    {
                        case PlanChangeTypes.NewPlan:
                            await NewPlanAsync(change.Steps).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.DoSteps:
                            await DoStepsAsync(change.Steps).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.DoStepsBeforeTags:
                            await DoStepsBeforeTagsAsync(change.Tags, change.Steps).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.DoStepsLater:
                            await DoStepsLaterAsync(change.Steps).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.EndPlan:
                            await EndPlanAsync(change.Steps).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.ReplacePlan:
                            await ReplacePlanAsync(change.Steps).ConfigureAwait(false);
                            break;
                    }
                }

                // Apply any queued up changes
                await ApplyChangesAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Inserts steps at the beginning of the plan to be executed immediately.
        /// </summary>
        /// <param name="steps">Steps to insert at the beginning of the plan.</param>
        /// <returns>True if a new plan had to be started.</returns>
        public async Task<bool> DoStepsAsync(List<PlanStepState> steps)
        {
            // Initialize new plan if needed
            bool isNewPlan = Plans.Plan == null;

            if (isNewPlan)
            {
                Plans.Plan = new PlanState() { Steps = new List<PlanStepState>() };
            }

            // Insert steps
            Plans.Plan.Steps.InsertRange(0, steps);


            // Emit new plan event
            if (isNewPlan)
            {
                await this.EmitEventAsync(PlanningEvents.PlanStarted.ToString(), null, false);
            }

            return isNewPlan;
        }

        public async Task<bool> DoStepsBeforeTagsAsync(List<string> tags, List<PlanStepState> steps)
        {
            // Initialize new plan if needed
            bool isNewPlan = Plans.Plan == null;

            if (isNewPlan)
            {
                Plans.Plan = new PlanState() { Steps = new List<PlanStepState>() };
            }

            // Search for tag to insert steps before
            var found = false;
            var idx = 0;

            for (int i = 0; i < Plans.Plan.Steps.Count && !found; i++)
            {
                var dialogId = Plans.Plan.Steps[i].DialogId;
                var dialog = FindDialog(dialogId);

                if (dialog != null && dialog.Tags.Count > 0)
                {
                    for (int j = 0; j < dialog.Tags.Count; j++)
                    {
                        if (tags.Contains(dialog.Tags[j]))
                        {
                            // Insert steps before current index
                            found = true;
                            idx = j;
                            break;
                        }
                    }
                }
            }

            // Insert steps
            if (found)
            {
                Plans.Plan.Steps.InsertRange(idx, steps);
            }
            else
            {
                Plans.Plan.Steps.InsertRange(0, steps);
            }
            


            // Emit new plan event
            if (isNewPlan)
            {
                await this.EmitEventAsync(PlanningEvents.PlanStarted.ToString(), null, false);
            }

            return isNewPlan;
        }

        public async Task<bool> DoStepsLaterAsync(List<PlanStepState> steps)
        {
            // Initialize new plan if needed
            bool isNewPlan = Plans.Plan == null;

            if (isNewPlan)
            {
                Plans.Plan = new PlanState() { Steps = new List<PlanStepState>() };
            }

            // Insert steps
            Plans.Plan.Steps.AddRange(steps);


            // Emit new plan event
            if (isNewPlan)
            {
                await this.EmitEventAsync(PlanningEvents.PlanStarted.ToString(), null, false);
            }

            return isNewPlan;
        }

        public async Task<bool> EndPlanAsync(List<PlanStepState> steps = null)
        {
            var resumePlan = Plans.SavedPlans != null && Plans.SavedPlans.Count > 0;

            if (resumePlan)
            {
                // Resume the plan
                var nextPlan = Plans.SavedPlans[0];
                Plans.SavedPlans.RemoveAt(0);
                Plans.Plan = nextPlan;

                if (Plans.SavedPlans.Count == 0)
                {
                    Plans.SavedPlans = null;
                }

                // Insert optional steps
                if (steps != null && steps.Count > 0)
                {
                    Plans.Plan.Steps.AddRange(steps);
                }

                // Emit resumption event
                await this.EmitEventAsync(PlanningEvents.PlanResumed.ToString(), null, true).ConfigureAwait(false);
            }
            else if (Plans.Plan != null)
            {
                this.Plans.Plan = null;

                // Emit planning ended event
                await this.EmitEventAsync(PlanningEvents.PlanEnded.ToString(), null, false).ConfigureAwait(false);
            }

            return resumePlan;
        }

        public async Task<bool> EndStepAsync()
        {
            if (Plans.Plan != null && Plans.Plan.Steps.Count > 0)
            {
                if (Plans.Plan.Steps.Count == 1)
                {
                    return await this.EndPlanAsync().ConfigureAwait(false);
                }
                else
                {
                    Plans.Plan.Steps.RemoveAt(0);
                }
            }

            return false;
        }

        public async Task<bool> NewPlanAsync(List<PlanStepState> steps)
        {
            // Save existing plan
            var savePlan = Plans.Plan != null && Plans.Plan.Steps.Count > 0;

            if (savePlan)
            {
                if (Plans.SavedPlans != null)
                {
                    Plans.SavedPlans = new List<PlanState>()
                    {
                        Plans.Plan
                    };
                }
            }

            // Initialize plan

            Plans.Plan = new PlanState()
            {
                Steps = steps
            };

            if (savePlan)
            {
                await this.EmitEventAsync(PlanningEvents.PlanSaved.ToString(), null, false).ConfigureAwait(false);
            }

            await this.EmitEventAsync(PlanningEvents.PlanStarted.ToString(), null, false).ConfigureAwait(false);

            return savePlan;
        }

        public async Task<bool> ReplacePlanAsync(List<PlanStepState> steps)
        {
            // Update plan
            var planReplaced = Plans.Plan != null && Plans.Plan.Steps.Count > 0;

            Plans.Plan = new PlanState()
            {
                Steps = steps
            };

            // Emit plan started event
            await this.EmitEventAsync(PlanningEvents.PlanStarted.ToString(), null, false);

            return planReplaced;
        }

        public void UpdatePlanTitle(string title)
        {
            if (Plans.Plan != null)
            {
                throw new Exception("PlanningContext.UpdatePlanTitle(): No plan found to update.");
            }

            Plans.Plan.Title = title;
        }

        public static PlanningContext Create(DialogContext dc, PlanningState plans)
        {
            return new PlanningContext(dc, dc.Parent, dc.Dialogs, new DialogState() { DialogStack = dc.Stack }, plans);
        }

        public static PlanningContext CreateForStep(PlanningContext planning, DialogSet dialogs)
        {
            var plans = planning.Plans;

            if (plans.Plan != null && plans.Plan.Steps.Count > 0)
            {
                var state = plans.Plan.Steps[0];
                return new PlanningContext(planning, planning, dialogs, state, plans);
            }
            else
            {
                return null;
            }
        }
    }

    public enum PlanningEvents
    {
        BeginDialog,
        ConsultDialog,
        ActivityReceived,
        UtteranceRecognized,
        Fallback,
        PlanStarted,
        PlanSaved,
        PlanEnded,
        PlanResumed
    }

    public class PlanningState
    {
        public dynamic Options { get; set; }
        public PlanState Plan { get; set; }
        public List<PlanState> SavedPlans { get; set; }
        public List<PlanChangeList> Changes { get; set; }
        public object Result { get; set; }
    }

    public class PlanStepState : DialogState
    {
        public string DialogId { get; set; }
        public object Options{ get; set; }
    }

    public class PlanState
    {
        public string Title { get; set; }
        public List<PlanStepState> Steps { get; set; }
    }

    public enum PlanChangeTypes
    {
        NewPlan,
        DoSteps,
        DoStepsBeforeTags,
        DoStepsLater,
        EndPlan,
        ReplacePlan
    }

    public class PlanChangeList
    {
        public PlanChangeTypes ChangeType { get; set; }
        public List<PlanStepState> Steps { get; set; }
        public List<string> Tags { get; set; }
        public List<string> EntitiesMatched { get; set; }
        public List<string> IntentsMatched { get; set; }
        public Dictionary<string, object> EntitiesRecognized { get; set; }
    }
}
