// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class PlanningContext : DialogContext
    {
        public PlanningState Plans { get; private set; }

        /// <summary>
        /// The current plan being executed (if any.)
        /// </summary>
        public PlanState Plan => Plans.Plan;

        public List<PlanChangeList> Changes { get { return this.Plans.Changes; } }

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

            if (this.Plans.Changes.Count > 0 && this.Plans.Changes[0].Desire != changes.Desire)
            {
                // A shouldProcess outweighs any canProcess changes
                if (changes.Desire == DialogConsultationDesire.ShouldProcess)
                {
                    this.Plans.Changes = new List<PlanChangeList>() { changes };
                }
                else
                {

                }
            }
            else
            {
                this.Plans.Changes.Add(changes);
            }
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
        public async Task<bool> ApplyChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var changes = Plans.Changes;

            if (changes != null)
            {
                // Reset plan changes
                Plans.Changes = null;

                // Apply each queued set of changes
                foreach (var change in changes)
                {
                    // apply memory changes to turn state
                    if (change.Turn != null)
                    {
                        foreach (var keyValue in change.Turn)
                        {
                            this.State.SetValue($"turn.{keyValue.Key}", keyValue.Value);
                        }
                    }

                    switch (change.ChangeType)
                    {
                        case PlanChangeTypes.NewPlan:
                            await NewPlanAsync(change.Steps, cancellationToken).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.DoSteps:
                            await DoStepsAsync(change.Steps, cancellationToken).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.DoStepsBeforeTags:
                            await DoStepsBeforeTagsAsync(change.Tags, change.Steps, cancellationToken).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.DoStepsLater:
                            await DoStepsLaterAsync(change.Steps, cancellationToken).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.EndPlan:
                            await EndPlanAsync(change.Steps, cancellationToken).ConfigureAwait(false);
                            break;
                        case PlanChangeTypes.ReplacePlan:
                            await ReplacePlanAsync(change.Steps, cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }

                // Apply any queued up changes
                await ApplyChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Inserts steps at the beginning of the plan to be executed immediately.
        /// </summary>
        /// <param name="steps">Steps to insert at the beginning of the plan.</param>
        /// <returns>True if a new plan had to be started.</returns>
        public async Task<bool> DoStepsAsync(List<PlanStepState> steps, CancellationToken cancellationToken = default(CancellationToken))
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
                await this.EmitEventAsync(AdaptiveEvents.StepsStarted.ToString(), null, false, cancellationToken).ConfigureAwait(false);
            }

            return isNewPlan;
        }

        public async Task<bool> DoStepsBeforeTagsAsync(List<string> tags, List<PlanStepState> steps, CancellationToken cancellationToken = default(CancellationToken))
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
                await this.EmitEventAsync(AdaptiveEvents.StepsStarted.ToString(), null, false, cancellationToken).ConfigureAwait(false);
            }

            return isNewPlan;
        }

        public async Task<bool> DoStepsLaterAsync(List<PlanStepState> steps, CancellationToken cancellationToken = default(CancellationToken))
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
                await this.EmitEventAsync(AdaptiveEvents.StepsStarted.ToString(), null, false, cancellationToken);
            }

            return isNewPlan;
        }

        public async Task<bool> EndPlanAsync(List<PlanStepState> steps = null, CancellationToken cancellationToken = default(CancellationToken))
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
                await this.EmitEventAsync(AdaptiveEvents.StepsResumed.ToString(), null, true, cancellationToken).ConfigureAwait(false);
            }
            else if (Plans.Plan != null)
            {
                this.Plans.Plan = null;

                // Emit planning ended event
                await this.EmitEventAsync(AdaptiveEvents.StepsEnded.ToString(), null, false, cancellationToken).ConfigureAwait(false);
            }

            return resumePlan;
        }

        public async Task<bool> EndStepAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Plans.Plan != null && Plans.Plan.Steps.Count > 0)
            {
                if (Plans.Plan.Steps.Count == 1)
                {
                    return await this.EndPlanAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Plans.Plan.Steps.RemoveAt(0);
                }
            }

            return false;
        }

        public async Task<bool> NewPlanAsync(List<PlanStepState> steps, CancellationToken cancellationToken = default(CancellationToken))
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
                await this.EmitEventAsync(AdaptiveEvents.StepsSaved.ToString(), null, false, cancellationToken).ConfigureAwait(false);
            }

            await this.EmitEventAsync(AdaptiveEvents.StepsStarted.ToString(), null, false, cancellationToken).ConfigureAwait(false);

            return savePlan;
        }

        public async Task<bool> ReplacePlanAsync(List<PlanStepState> steps, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Update plan
            var planReplaced = Plans.Plan != null && Plans.Plan.Steps.Count > 0;

            Plans.Plan = new PlanState()
            {
                Steps = steps
            };

            // Emit plan started event
            await this.EmitEventAsync(AdaptiveEvents.StepsStarted.ToString(), null, false, cancellationToken);

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

    public class AdaptiveEvents : DialogContext.DialogEvents
    {
        public const string ActivityReceived = "activityReceived";
        public const string RecognizedIntent = "recognizedIntent";
        public const string UnknownIntent = "unknownIntent";
        public const string StepsStarted = "stepsStarted";
        public const string StepsSaved = "stepsSaved";
        public const string StepsEnded = "stepsEnded";
        public const string StepsResumed = "stepsResumed";
    }

    public class PlanningState
    {
        public PlanningState()
        {
        }

        [JsonProperty(PropertyName = "options")]
        public dynamic Options { get; set; }

        [JsonProperty(PropertyName = "plan")]
        public PlanState Plan { get; set; }

        [JsonProperty(PropertyName = "savedPlans")]
        public List<PlanState> SavedPlans { get; set; } = new List<PlanState>();

        [JsonProperty(PropertyName = "changes")]
        public List<PlanChangeList> Changes { get; set; } = new List<PlanChangeList>();

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }

    [DebuggerDisplay("{DialogId}")]
    public class PlanStepState : DialogState
    {
        public PlanStepState()
        {
        }

        public PlanStepState(string dialogId = null, object options = null)
        {
            DialogId = dialogId;
            Options = options;
        }

        [JsonProperty(PropertyName = "dialogId")]
        public string DialogId { get; set; }

        [JsonProperty(PropertyName = "options")]
        public object Options { get; set; }
    }

    [DebuggerDisplay("{Title}")]
    public class PlanState
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "steps")]
        public List<PlanStepState> Steps { get; set; } = new List<PlanStepState>();
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

    [DebuggerDisplay("{ChangeType}:{Desire}")]
    public class PlanChangeList
    {
        [JsonProperty(PropertyName = "changeType")]
        public PlanChangeTypes ChangeType { get; set; } = PlanChangeTypes.DoSteps;

        [JsonProperty(PropertyName = "steps")]
        public List<PlanStepState> Steps { get; set; } = new List<PlanStepState>();

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets turn state associated with the plan change list (it will be applied to turn state when plan is applied)
        /// </summary>
        [JsonProperty(PropertyName = "turn")]
        public Dictionary<string, object> Turn { get; set; }

        [JsonProperty(PropertyName = "desire")]
        public DialogConsultationDesire Desire { get; set; } = DialogConsultationDesire.ShouldProcess;

    }
}
