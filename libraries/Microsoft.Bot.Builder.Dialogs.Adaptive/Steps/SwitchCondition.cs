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
        /// value expression to be compared against condition
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("steps")]
        public List<IDialog> Steps { get; set; } = new List<IDialog>();
    }

    /// <summary>
    /// Conditional branch with multiple cases
    /// </summary>
    public class SwitchCondition : DialogCommand, IDialogDependencies
    {
        private Dictionary<string, Expression> caseExpressions = null;

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
                    Expression condition = engine.Parse(this.Condition);
                    if (this.caseExpressions == null)
                    {
                        this.caseExpressions = new Dictionary<string, Expression>();
                        foreach (var c in this.Cases)
                        {
                            var caseCondition = Expression.EqualsExpression(condition, engine.Parse(c.Value));
                            // map of expression to steps
                            this.caseExpressions[c.Value] = caseCondition;
                        }
                    }
                }

                List<IDialog> stepsToRun = this.Default;

                foreach (var caseCondition in this.Cases)
                {

                    var (value, error) = this.caseExpressions[caseCondition.Value].TryEvaluate(dc.State);
                    if (error != null)
                    {
                        // Do what? 
                    }
                    else if (((bool)value) == true)
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
