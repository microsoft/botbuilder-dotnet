// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Conditional branch with multiple cases
    /// </summary>
    public class SwitchCondition : DialogCommand, IDialogDependencies
    {
        private Dictionary<Expression, List<IDialog>> cases = null;

        /// <summary>
        /// Condition expression against memory Example: "user.age"
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Cases
        /// </summary>
        public Dictionary<string, List<IDialog>> Cases = new Dictionary<string, List<IDialog>>();

        /// <summary>
        /// Default case
        /// </summary>
        public List<IDialog> Default { get; set; } = new List<IDialog>();

        [JsonConstructor]
        public SwitchCondition([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0) : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                lock (this.Condition)
                {
                    var engine = new ExpressionEngine();
                    Expression condition = engine.Parse(this.Condition);
                    if (this.cases == null)
                    {
                        this.cases = new Dictionary<Expression, List<IDialog>>();
                        foreach (var c in this.Cases)
                        {
                            var caseCondition = Expression.EqualsExpression(condition, engine.Parse(c.Key));
                            // map of expression to steps
                            this.cases[caseCondition] = c.Value;
                        }
                    }
                }

                List<IDialog> stepsToRun = this.Default;

                foreach (var caseCondition in this.cases)
                {

                    var (value, error) = caseCondition.Key.TryEvaluate(dc.State);
                    if (error != null)
                    {
                        // Do what? 
                    }
                    else if (((bool)value) == true)
                    {
                        stepsToRun = caseCondition.Value;
                        break;
                    }
                }

                // run condition or default steps
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
                foreach (var steps in this.Cases.Values)
                {
                    dialogs.AddRange(steps);
                }
            }

            return dialogs;
        }
    }
}
