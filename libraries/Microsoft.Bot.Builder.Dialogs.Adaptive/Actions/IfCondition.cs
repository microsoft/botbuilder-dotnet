// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Conditional branch.
    /// </summary>
    public class IfCondition : Dialog, IDialogDependencies
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.IfCondition";

        private ActionScope trueScope;
        private ActionScope falseScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfCondition"/> class.
        /// </summary>
        /// <param name="sourceFilePath">Optional, source file full path.</param>
        /// <param name="sourceLineNumber">Optional, line number in source file.</param>
        [JsonConstructor]
        public IfCondition([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets the memory expression. 
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// The memory expression. 
        /// </value>
        [JsonProperty("condition")]
        public BoolExpression Condition { get; set; } = false;

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
        /// Gets or sets the list of actions.
        /// </summary>
        /// <value>A <see cref="List{T}"/> of Dialog actions.</value>
        [JsonProperty("actions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Dialog> Actions { get; set; } = new List<Dialog>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets a list actions for false scope.
        /// </summary>
        /// <value>A <see cref="List{T}"/> of Dialog actions.</value>
        [JsonProperty("elseActions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Dialog> ElseActions { get; set; } = new List<Dialog>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets the true scope.
        /// </summary>
        /// <value>An <see cref="ActionScope"/> with the action scope.</value>
        protected ActionScope TrueScope
        {
            get
            {
                if (trueScope == null)
                {
                    trueScope = new ActionScope(this.Actions) { Id = $"True{this.Id}" };
                }

                return trueScope;
            }
        }

        /// <summary>
        /// Gets the false scope.
        /// </summary>
        /// <value>An <see cref="ActionScope"/> with the action scope.</value>
        protected ActionScope FalseScope
        {
            get
            {
                if (falseScope == null)
                {
                    falseScope = new ActionScope(this.ElseActions) { Id = $"False{this.Id}" };
                }

                return falseScope;
            }
        }

        /// <summary>
        /// Enumerates child dialog dependencies so they can be added to the containers dialog set.
        /// </summary>
        /// <returns>Dialog enumeration.</returns>
        public virtual IEnumerable<Dialog> GetDependencies()
        {
            yield return this.TrueScope;
            yield return this.FalseScope;
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

            if (string.IsNullOrEmpty(Condition.ExpressionText))
            {
                throw new InvalidOperationException("Adaptive Dialogs error: Missing predicate condition. Please add a valid predicate to the Condition property of IfCondition().");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var (conditionResult, error) = this.Condition.TryGetValue(dc.State);
            if (error == null && conditionResult == true && TrueScope.Actions.Any())
            {
                // replace dialog with If True Action Scope
                return await dc.ReplaceDialogAsync(this.TrueScope.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else if (conditionResult == false && FalseScope.Actions.Any())
            {
                return await dc.ReplaceDialogAsync(this.FalseScope.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // end dialog since no triggered actions
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            var idList = Actions.Select(s => s.Id);
            return $"{GetType().Name}({this.Condition?.ToString()}|{StringUtils.Ellipsis(string.Join(",", idList), 50)})";
        }
    }
}
