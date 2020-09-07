// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.TriggerTrees;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the most specific true rule implementation of <see cref="TriggerSelector"/>.
    /// </summary>
    public class MostSpecificSelector : TriggerSelector
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.MostSpecificSelector";

        private readonly TriggerTree _tree = new TriggerTree();

        /// <summary>
        /// Gets or sets optional rule selector to use when more than one most specific rule is true.
        /// </summary>
        /// <value>
        /// Optional rule selector to use when more than one most specific rule is true.
        /// </value>
        [JsonProperty("selector")]
        public TriggerSelector Selector { get; set; }

        /// <summary>
        /// Initializes the selector with the set of rules.
        /// </summary>
        /// <param name="conditionals">Possible rules to match.</param>
        /// <param name="evaluate">Optional, true by default if rules should be evaluated on select.</param>
        public override void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            foreach (var conditional in conditionals)
            {
                _tree.AddTrigger(conditional.GetExpression(), conditional);
            }
        }

        /// <summary>
        /// Selects the best rule to execute.
        /// </summary>
        /// <param name="context">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        public override async Task<IReadOnlyList<OnCondition>> SelectAsync(ActionContext context, CancellationToken cancellationToken)
        {
            var triggers = _tree.Matches(context.State);
            var matches = new List<OnCondition>();
            foreach (var trigger in triggers)
            {
                matches.Add(trigger.Action as OnCondition);
            }

            IReadOnlyList<OnCondition> selections = matches;
            if (matches.Count > 0 && Selector != null)
            {
                Selector.Initialize(matches, false);
                selections = await Selector.SelectAsync(context, cancellationToken).ConfigureAwait(false);
            }

            return selections;
        }
    }
}
