// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
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

        private ActionScope defaultScope;

        [JsonConstructor]
        public SwitchCondition([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets value expression against memory Example: "user.age".
        /// </summary>
        /// <value>
        /// Value Expression against memory. This value expression will be combined with value expression in case statements to make a bool expression.
        /// </value>
        [JsonProperty("condition")]
        public Expression Condition { get; set; }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

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

        protected ActionScope DefaultScope
        {
            get
            {
                if (defaultScope == null)
                {
                    defaultScope = new ActionScope() { Actions = this.Default };
                }

                return defaultScope;
            }
        }

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            yield return this.DefaultScope;

            if (this.Cases != null)
            {
                foreach (var caseScope in this.Cases)
                {
                    yield return caseScope;
                }
            }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            lock (this.Condition)
            {
                if (this.caseExpressions == null)
                {
                    this.caseExpressions = new Dictionary<string, Expression>();

                    foreach (var cse in this.Cases)
                    {
                        Expression caseExpression = null;

                        // Values for cases are always coerced to constant
                        if (long.TryParse(cse.Value, out long i))
                        {
                            caseExpression = Expression.ConstantExpression(i);
                        }
                        else if (float.TryParse(cse.Value, out float f))
                        {
                            caseExpression = Expression.ConstantExpression(f);
                        }
                        else if (bool.TryParse(cse.Value, out bool b))
                        {
                            caseExpression = Expression.ConstantExpression(b);
                        }
                        else
                        {
                            caseExpression = Expression.ConstantExpression(cse.Value);
                        }

                        var caseCondition = Expression.EqualsExpression(this.Condition, caseExpression);

                        // Map of expression to actions
                        this.caseExpressions[cse.Value] = caseCondition;
                    }
                }
            }

            ActionScope actionScope = this.DefaultScope;

            foreach (var caseScope in this.Cases)
            {
                var (value, error) = this.caseExpressions[caseScope.Value].TryEvaluate(dc.State);

                if (error != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {caseExpressions[caseScope.Value].ToString()}. Error: {error}");
                }

                // Compare both expression results. The current switch case triggers if the comparison is true.
                if (((bool)value) == true)
                {
                    actionScope = caseScope;
                    break;
                }
            }

            return await dc.ReplaceDialogAsync(actionScope.Id, null, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.Condition?.ToString()})";
        }
    }
}
