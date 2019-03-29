using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    /// <summary>
    /// Conditional branch
    /// </summary>
    public class IfCondition : DialogCommand, IDialogDependencies
    {
        /// <summary>
        /// Condition expression against memory Example: "user.age > 18"
        /// </summary>
        public IExpression Condition { get; set; }

        public List<IDialog> IfTrue { get; set; } = new List<IDialog>();

        public List<IDialog> IfFalse { get; set; } = new List<IDialog>();

        public IfCondition()
            : base()
        {
        }
        
        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                var conditionResult = (bool)await Condition.Evaluate(dc.State);

                var stepsToRun = conditionResult ? IfTrue : IfFalse;

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
            var trueIdList = IfTrue.Select(s => s.Id);
            var falseIdList = IfFalse.Select(s => s.Id);
            return $"conditional({string.Join(",", trueIdList)}|{string.Join(",", falseIdList)})";
        }

        public override List<IDialog> ListDependencies()
        {
            return IfTrue.Concat(IfFalse).ToList();
        }
    }
}
