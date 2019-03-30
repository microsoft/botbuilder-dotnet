using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    /// <summary>
    /// Conditional branch with multiple cases
    /// </summary>
    public class SwitchCondition : DialogCommand, IDialogDependencies
    {
        /// <summary>
        /// Condition expression against memory Example: "user.age"
        /// </summary>
        public Expression Condition { get; set; }

        /// <summary>
        /// Cases
        /// </summary>
        public Dictionary<string, List<IDialog>> Cases = new Dictionary<string, List<IDialog>>();

        /// <summary>
        /// Default case
        /// </summary>
        public List<IDialog> Default { get; set; } = new List<IDialog>();

        public SwitchCondition() : base()
        {
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                var (value, error) = Condition.TryEvaluate(dc.State);

                List<IDialog> stepsToRun = this.Default;

                if (this.Cases.TryGetValue(value?.ToString(), out List<IDialog> steps))
                {
                    stepsToRun = steps;
                }

                var planSteps = stepsToRun.Select(s => new PlanStepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                // Queue up steps that should run after current step
                planning.QueueChanges(new PlanChangeList()
                {
                    ChangeType = PlanChangeTypes.DoSteps,
                    Steps = planSteps.ToList()
                });

                return await planning.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`IfCondition` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            return $"conditional({this.Condition})";
        }

        public override List<IDialog> ListDependencies()
        {
            var dialogs = new List<IDialog>();
            if (this.Default != null)
            {
                dialogs.AddRange(this.Default);
            }

            if (this.Cases != null)
            {
                foreach(var steps in this.Cases.Values)
                {
                    dialogs.AddRange(steps);
                }
            }

            return dialogs;
        }
    }
}
