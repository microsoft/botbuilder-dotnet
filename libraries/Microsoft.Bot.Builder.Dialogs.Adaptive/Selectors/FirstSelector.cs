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
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.FirstSelector";

        private List<OnCondition> _conditionals;
        private bool _evaluate;

        public override void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        public override Task<IReadOnlyList<OnCondition>> Select(ActionContext context, CancellationToken cancel)
        {
            OnCondition selection = null;
            var lowestPriority = int.MaxValue;
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
