using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class IfPropertyCondition
    {
        public Expression Expression { get; set; }

        public List<IDialog> Steps { get; set; }
    }

    public class IfProperty : DialogCommand, IDialogDependencies
    {
        public Expression Expression { get; set; }

        public List<IDialog> IfTrue { get; set; } = new List<IDialog>();

        public List<IDialog> IfFalse { get; set; } = new List<IDialog>();

        public IfProperty()
            : base()
        {
        }
        
        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                // TODO: Handle errors from expression evaluation
                var (result, error) = Expression.TryEvaluate(dc.State);
                var conditionResult = (bool)result;

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

                return await planning.EndDialogAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`IfProperty` should only be used in the context of a planning or sequence dialog.");
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
