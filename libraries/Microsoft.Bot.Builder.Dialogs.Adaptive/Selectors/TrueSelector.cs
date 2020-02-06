// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select all rules which evaluate to true.
    /// </summary>
    public class TrueSelector : ITriggerSelector
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.TrueSelector";

        private List<OnCondition> _conditionals;
        private bool _evaluate;

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate = true)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        public Task<IReadOnlyList<OnCondition>> Select(SequenceContext context, CancellationToken cancel = default)
        {
            var candidates = _conditionals;
            if (_evaluate)
            {
                candidates = new List<OnCondition>();
                foreach (var conditional in _conditionals)
                {
                    var expression = conditional.GetExpression();
                    var (value, error) = expression.TryEvaluate(context.GetState());
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
