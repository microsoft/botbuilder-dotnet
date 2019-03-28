using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class OnCatchConditional
    {
        public string EventName { get; set; }
        public List<IDialog> Steps { get; set; }
    }

    public class OnCatch : DialogCommand, IDialogDependencies
    {
        public IDialog Step { get; set; }

        public List<OnCatchConditional> Conditionals { get; set; }

        public override async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e)
        {
            // Find the planning context to use
            // There is an issue created by consultation where our current DialogContext might
            // not have a reference to the actual plan os we need to search for the context to use
            PlanningContext planning = null;

            while (dc != null)
            {
                if (dc is PlanningContext candidatePlanningCtx && candidatePlanningCtx.Plan != null)
                {
                    var plan = candidatePlanningCtx.Plan;

                    if (plan.Steps.Count > 0 && plan.Steps[0].DialogId == this.Id)
                    {
                        planning = candidatePlanningCtx;
                        break;
                    }
                }
                dc = dc.Parent;
            }

            if (planning != null)
            {
                // Find conditional matching event
                foreach (var conditional in Conditionals)
                {
                    if (conditional.EventName == e.Name)
                    {
                        // Queue up the steps for the current plan
                        planning.QueueChanges(new PlanChangeList()
                        {
                            ChangeType = PlanChangeTypes.DoSteps,
                            Steps = conditional.Steps.Select(s => new PlanStepState()
                            {
                                DialogStack = new List<DialogInstance>(),
                                DialogId = s.Id
                            }).ToList()
                        });

                        //  Signal that we handled the event
                        return true;
                    }
                }
            }

            return false;
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Save initial options and start step
            return await dc.BeginDialogAsync(Step.Id, options, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"OnCatch[{Step?.Id}]";
        }

        public override List<IDialog> ListDependencies()
        {
            var steps = new List<IDialog>() { Step };
            steps.AddRange(Conditionals.SelectMany(c => c.Steps));
            return steps;
        }
    }
}
