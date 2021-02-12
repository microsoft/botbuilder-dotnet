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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.SwitchCondition";

        private Dictionary<string, Expression> caseExpressions = null;

        private ActionScope defaultScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchCondition"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Dialog> Default { get; set; } = new List<Dialog>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets Cases.
        /// </summary>
        /// <value>
        /// Cases.
        /// </value>
        [JsonProperty("cases")]
#pragma warning disable CA2227 // Collection properties should be read only
        public List<Case> Cases { get; set; } = new List<Case>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets the default scope.
        /// </summary>
        /// <value>An <see cref="ActionScope"/> with the scope.</value>
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

        /// <summary>
        /// Enumerates child dialog dependencies so they can be added to the containers dialog set.
        /// </summary>
        /// <returns>Dialog enumeration.</returns>
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

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            lock (this.Condition)
            {
                if (this.caseExpressions == null)
                {
                    this.caseExpressions = new Dictionary<string, Expression>();

                    foreach (var @case in this.Cases)
                    {
                        if (long.TryParse(@case.Value, out long intVal))
                        {
                            // you don't have to put quotes around numbers, "23" => 23 OR "23"
                            this.caseExpressions[@case.Value] = Expression.OrExpression(
                                Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(intVal)),
                                Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(@case.Value)));
                        }
                        else if (float.TryParse(@case.Value, out float floatVal))
                        {
                            // you don't have to put quotes around numbers, "23" => 23 OR "23"
                            this.caseExpressions[@case.Value] = Expression.OrExpression(
                                Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(floatVal)),
                                Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(@case.Value)));
                        }
                        else if (bool.TryParse(@case.Value, out bool boolVal))
                        {
                            // you don't have to put quotes around bools, "true" => true OR "true"
                            this.caseExpressions[@case.Value] = Expression.OrExpression(
                                Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(boolVal)),
                                Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(@case.Value)));
                        }
                        else
                        {
                            // if someone does "=23" that will be numeric comparison or "='23'" that will be string comparison, or it can be a 
                            // real expression bound to memory.
                            var (value, _) = new ValueExpression(@case.Value).TryGetValue(dc.State);
                            this.caseExpressions[@case.Value] = Expression.EqualsExpression(this.Condition, Expression.ConstantExpression(value));
                        }
                    }
                }
            }

            ActionScope actionScope = this.DefaultScope;

            foreach (var caseScope in this.Cases)
            {
                var (value, error) = this.caseExpressions[caseScope.Value].TryEvaluate(dc.State);

                // Compare both expression results. The current switch case triggers if the comparison is true.
                if (value != null && ((bool)value) == true)
                {
                    actionScope = caseScope;
                    break;
                }
            }

            return await dc.ReplaceDialogAsync(actionScope.Id, null, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({this.Condition?.ToString()})";
        }
    }
}
