// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Conditional branch with multiple cases.
    /// </summary>
    public class SwitchCondition : Dialog, IDialogDependencies
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.SwitchCondition";

        private Dictionary<string, Expression> caseExpressions = null;

        private Expression condition;

        [JsonConstructor]
        public SwitchCondition([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets condition expression against memory Example: "user.age > 18".
        /// </summary>
        /// <value>
        /// Condition expression against memory Example: "user.age > 18".
        /// </value>
        [JsonProperty("condition")]
        public string Condition
        {
            get { return condition?.ToString(); }
            set { condition = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets default case.
        /// </summary>
        /// <value>
        /// Default case.
        /// </value>
        [JsonProperty("default")]
        public List<Dialog> Default { get; set; } = new List<Dialog>();

        /// <summary>
        /// Gets or sets Cases.
        /// </summary>
        /// <value>
        /// Cases.
        /// </value>
        [JsonProperty("cases")]
        public List<Case> Cases { get; set; } = new List<Case>();

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            var dialogs = new List<Dialog>();
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

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
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

                List<Dialog> actionsToRun = this.Default;

                foreach (var caseCondition in this.Cases)
                {
                    var (value, error) = this.caseExpressions[caseCondition.Value].TryEvaluate(dc.GetState());

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
            return $"{this.GetType().Name}({this.Condition})";
        }
    }
}
