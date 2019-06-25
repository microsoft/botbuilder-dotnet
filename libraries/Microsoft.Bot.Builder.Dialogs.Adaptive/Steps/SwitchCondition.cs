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
    public class Case
    {
        public Case(string value = null, IEnumerable<IDialog> steps = null)
        {
            this.Value = value;
            this.Steps = steps?.ToList() ?? this.Steps;
        }

        /// <summary>
        /// Value expression to be compared against condition.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Set of steps to be executed given that the condition of the switch matches the value of this case.
        /// </summary>
        [JsonProperty("steps")]
        public List<IDialog> Steps { get; set; } = new List<IDialog>();
    }

    /// <summary>
    /// Conditional branch with multiple cases
    /// </summary>
    public class SwitchCondition : DialogCommand, IDialogDependencies
    {
        private Dictionary<string, Expression> caseExpressions = null;
        private Expression conditionExpression;

        /// <summary>
        /// Condition expression against memory Example: "user.age"
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Cases
        /// </summary>
        public List<Case> Cases = new List<Case>();

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
            if (dc is SequenceContext planning)
            {
                lock (this.Condition)
                {
                    var engine = new ExpressionEngine();
                    conditionExpression = engine.Parse(this.Condition);
                    if (this.caseExpressions == null)
                    {
                        this.caseExpressions = new Dictionary<string, Expression>();
                        foreach (var c in this.Cases)
                        {
                            // Values for cases are always coerced to string
                            var caseCondition = Expression.ConstantExpression(c.Value);
                            
                            // Map of expression to steps
                            this.caseExpressions[c.Value] = caseCondition;
                        }
                    }
                }

                List<IDialog> stepsToRun = this.Default;

                // Evaluate the condition expression, i.e. the left side of the switch equality
                var (conditionEvaluation, conditionError) = this.conditionExpression.TryEvaluate(dc.State);

                if (conditionError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {this.conditionExpression.ToString()}. Error: {conditionError}");
                }

                foreach (var caseCondition in this.Cases)
                {
                    var caseExpression = this.caseExpressions[caseCondition.Value];

                    // Evaluate the constant expression for the right side of the equality
                    var (value, error) = caseExpression.TryEvaluate(dc.State);

                    if (conditionError != null)
                    {
                        throw new Exception($"Expression evaluation resulted in an error. Expression: {caseExpression.ToString()}. Error: {error}");
                    }

                    // Compare both expression results. The current switch case triggers if the comparison is true.
                    if (conditionEvaluation.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        stepsToRun = caseCondition.Steps;
                        break;
                    }
                }

                // run condition or default steps
                var planSteps = stepsToRun.Select(s => new StepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                // Queue up steps that should run after current step
                planning.QueueChanges(new StepChangeList()
                {
                    ChangeType = StepChangeTypes.InsertSteps,
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
            return $"Switch({this.Condition})";
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
                foreach (var conidtionalCase in this.Cases)
                {
                    dialogs.AddRange(conidtionalCase.Steps);
                }
            }

            return dialogs;
        }
    }
}
