// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select a random true triggerHandler implementation of IRuleSelector.
    /// </summary>
    public class RandomSelector : TriggerSelector
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RandomSelector";

        private List<OnCondition> _conditionals;
        private bool _evaluate;

        /// <summary>
        /// Gets or sets optional seed for random number generator.
        /// </summary>
        /// <remarks>If not specified a random seed will be used.</remarks>
        /// <value>
        /// Optional seed for random number generator.
        /// </value>
        [JsonProperty("seed")]
        public int Seed { get; set; } = -1;

        /// <summary>
        /// Initializes the selector with the set of rules.
        /// </summary>
        /// <param name="conditionals">Possible rules to match.</param>
        /// <param name="evaluate">Optional, true by default if rules should be evaluated on select.</param>
        public override void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        /// <summary>
        /// Selects the best rule to execute.
        /// </summary>
        /// <param name="context">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of the task.</param>
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
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        candidates.Add(conditional);
                    }
                }
            }

            var result = new List<OnCondition>();
            if (candidates.Count > 0)
            {
                var memory = MemoryFactory.Create(context.State);
                int? customizedSeed = null;
                if (Seed != -1)
                {
                    customizedSeed = Seed;
                }

                var selection = memory.RandomNext(0, candidates.Count, customizedSeed);

                result.Add(candidates[selection]);
            }

            return Task.FromResult((IReadOnlyList<OnCondition>)result);
        }
    }
}
