// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select between two rule selectors based on a condition.
    /// </summary>
    public class ConditionalSelector : TriggerSelector
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ConditionalSelector";

        private IReadOnlyList<OnCondition> _conditionals;
        private bool _evaluate;

        /// <summary>
        /// Gets or sets expression that determines which selector to use.
        /// </summary>
        /// <value>
        /// Expression that determines which selector to use.
        /// </value>
        [JsonProperty("condition")]
        public BoolExpression Condition { get; set; }

        /// <summary>
        /// Gets or sets selector if <see cref="Condition"/> is true.
        /// </summary>
        /// <value>
        /// Selector if <see cref="Condition"/> is true.
        /// </value>
        [JsonProperty("ifTrue")]
        public TriggerSelector IfTrue { get; set; }

        /// <summary>
        /// Gets or sets selector if <see cref="Condition"/> is false.
        /// </summary>
        /// <value>
        /// Selector if <see cref="Condition"/> is false.
        /// </value>
        [JsonProperty("ifFalse")]
        public TriggerSelector IfFalse { get; set; }

        /// <summary>
        /// Initializes the selector with the set of rules.
        /// </summary>
        /// <param name="conditionals">Possible rules to match.</param>
        /// <param name="evaluate">Optional, true by default if rules should be evaluated on select.</param>
        public override void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate = true)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        /// <summary>
        /// Selects the best rule to execute.
        /// </summary>
        /// <param name="actionContext">Dialog context for evaluation.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        public override async Task<IReadOnlyList<OnCondition>> SelectAsync(ActionContext actionContext, CancellationToken cancellationToken = default)
        {
            var (eval, _) = Condition.TryGetValue(actionContext.State);
            TriggerSelector selector;
            if (eval)
            {
                selector = IfTrue;
                IfTrue.Initialize(_conditionals, _evaluate);
            }
            else
            {
                selector = IfFalse;
                IfFalse.Initialize(_conditionals, _evaluate);
            }

            return await selector.SelectAsync(actionContext, cancellationToken).ConfigureAwait(false);
        }
    }
}
