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
    /// Select the first ordered by priority true OnCondition.
    /// </summary>
    public class FirstSelector : TriggerSelector
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.FirstSelector";

        private List<OnCondition> _conditionals;
        private bool _evaluate;

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
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        public override Task<IReadOnlyList<OnCondition>> SelectAsync(ActionContext context, CancellationToken cancellationToken)
        {
            OnCondition selection = null;
            var lowestPriority = double.MaxValue;
            if (_evaluate)
            {
                for (var i = 0; i < _conditionals.Count; i++)
                {
                    var conditional = _conditionals[i];
                    var expression = conditional.GetExpression();
                    var (value, error) = expression.TryEvaluate(context.State);
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        var priority = conditional.CurrentPriority(context);
                        if (priority >= 0 && priority < lowestPriority)
                        {
                            selection = conditional;
                            lowestPriority = priority;
                        }
                    }
                }
            }
            else
            {
                foreach (var conditional in _conditionals)
                {
                    var priority = conditional.CurrentPriority(context);
                    if (priority >= 0 && priority < lowestPriority)
                    {
                        selection = conditional;
                        lowestPriority = priority;
                    }
                }
            }

            var result = new List<OnCondition>();
            if (selection != null)
            {
                result.Add(selection);
            }

            return Task.FromResult((IReadOnlyList<OnCondition>)result);
        }
    }
}
