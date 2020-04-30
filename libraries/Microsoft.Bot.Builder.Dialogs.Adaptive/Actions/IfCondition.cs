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
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.IfCondition";

        private ActionScope trueScope;
        private ActionScope falseScope;

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

        [JsonProperty("actions")]
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        [JsonProperty("elseActions")]
        public List<Dialog> ElseActions { get; set; } = new List<Dialog>();

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

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            yield return this.TrueScope;
            yield return this.FalseScope;
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

        protected override string OnComputeId()
        {
            var idList = Actions.Select(s => s.Id);
            return $"{this.GetType().Name}({this.Condition?.ToString()}|{StringUtils.Ellipsis(string.Join(",", idList), 50)})";
        }
    }
}
