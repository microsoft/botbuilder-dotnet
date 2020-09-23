// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select all rules which evaluate to true.
    /// </summary>
    public class TrueSelector : TriggerSelector
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TrueSelector";

        private List<OnCondition> _conditionals;
        private bool _evaluate;

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
        /// <param name="context">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        public override Task<IReadOnlyList<OnCondition>> SelectAsync(ActionContext context, CancellationToken cancellationToken = default)
        {
            var candidates = _conditionals;
            if (_evaluate)
            {
                candidates = new List<OnCondition>();
                foreach (var conditional in _conditionals)
                {
                    var expression = conditional.GetExpression();
                    var (value, error) = expression.TryEvaluate(context.State);
                    var result = error == null && (bool)value;
                    if (result == true)
                    {
                        candidates.Add(conditional);
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<OnCondition>)candidates);
        }
    }
}
