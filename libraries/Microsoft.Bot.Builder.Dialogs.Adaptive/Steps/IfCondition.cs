// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Conditional branch
    /// </summary>
    public class IfCondition : DialogCommand, IDialogDependencies
    {
        /// <summary>
        /// Condition expression against memory Example: "user.age > 18"
        /// </summary>
        [JsonProperty("condition")]
        public Expression Condition { get; set; }

        [JsonProperty("steps")]
        public List<IDialog> Steps { get; set; } = new List<IDialog>();

        [JsonProperty("elseSteps")]
        public List<IDialog> ElseSteps { get; set; } = new List<IDialog>();

        public IfCondition()
            : base()
        {
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                var (value, error) = Condition.TryEvaluate(dc.State);
                var conditionResult = error == null && (bool)value;

                var steps = new List<IDialog>();
                if (conditionResult == true)
                {
                    steps = this.Steps;
                }
                else
                {
                    steps = this.ElseSteps;
                }

                var planSteps = steps.Select(s => new PlanStepState()
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
            var idList = Steps.Select(s => s.Id);
            return $"conditional({this.Condition}|{string.Join(",", idList)})";
        }

        public override List<IDialog> ListDependencies()
        {
            return Steps;
        }
    }
}
