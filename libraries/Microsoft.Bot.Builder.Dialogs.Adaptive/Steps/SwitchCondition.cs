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

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class Case
    {
        public Case(string value = null, IEnumerable<IDialog> actions = null)
        {
            this.Value = value;
            this.Actions = actions?.ToList() ?? this.Actions;
        }

        /// <summary>
        /// Value expression to be compared against condition.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Set of actions to be executed given that the condition of the switch matches the value of this case.
        /// </summary>
        [JsonProperty("actions")]
        public List<IDialog> Actions { get; set; } = new List<IDialog>();

        /// <summary>
        /// Creates an expression that returns the value in its primitive type. Still
        /// assumes that switch case values are compile time constants and not expressions
        /// that can be evaluated against state.
        /// </summary>
        /// <returns>An expression that reflects the constant case value</returns>
        public Expression CreateValueExpression()
        {
            Expression expression = null;

            if (Int64.TryParse(Value, out Int64 i))
            {
                expression = Expression.ConstantExpression(i);
            }
            else if (float.TryParse(Value, out float f))
            {
                expression = Expression.ConstantExpression(f);
            }
            else if (bool.TryParse(Value, out bool b))
            {
                expression = Expression.ConstantExpression(b);
            }
            else 
            {
                expression = Expression.ConstantExpression(Value);
            }

            return expression;
        }
    }

    /// <summary>
    /// Conditional branch with multiple cases
    /// </summary>
    public class SwitchCondition : DialogAction, IDialogDependencies
    {
        private Dictionary<string, Expression> caseExpressions = null;

        private Expression condition;

        /// <summary>
        /// Condition expression against memory Example: "user.age > 18"
        /// </summary>
        [JsonProperty("condition")]
        public string Condition
        {
            get { return condition?.ToString(); }
            set {condition = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

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
                    if (this.caseExpressions == null)
                    {
                        this.caseExpressions = new Dictionary<string, Expression>();

                        foreach (var c in this.Cases)
                        {
                            // Values for cases are always coerced to string
                            var caseCondition = Expression.EqualsExpression(this.condition, c.CreateValueExpression());

                            // Map of expression to actions
                            this.caseExpressions[c.Value] = caseCondition;
                        }
                    }
                }

                List<IDialog> actionsToRun = this.Default;

                foreach (var caseCondition in this.Cases)
                {

                    var (value, error) = this.caseExpressions[caseCondition.Value].TryEvaluate(dc.State);

                    if (error != null)
                    {
                        throw new Exception($"Expression evaluation resulted in an error. Expression: {caseExpressions[caseCondition.Value].ToString()}. Error: {error}");
                    }

                    // Compare both expression results. The current switch case triggers if the comparison is true.
                    if (((bool)value) == true)
                    {
                        actionsToRun = caseCondition.Actions;
                        break;
                    }
                }

                // run condition or default actions
                var planActions = actionsToRun.Select(s => new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = options
                });

                // Queue up actions that should run after current step
                planning.QueueChanges(new ActionChangeList()
                {
                    ChangeType = ActionChangeType.InsertActions,
                    Actions = planActions.ToList()
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
                    dialogs.AddRange(conidtionalCase.Actions);
                }
            }

            return dialogs;
        }
    }
}
