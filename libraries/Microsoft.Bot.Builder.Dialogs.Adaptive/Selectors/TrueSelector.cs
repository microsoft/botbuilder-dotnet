using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select all rules which evaluate to true.
    /// </summary>
    public class TrueSelector : IRuleSelector
    {
        private List<IRule> _rules;
        private bool _evaluate;

        public Task Initialize(PlanningContext context, IEnumerable<IRule> rules, bool evaluate = true, CancellationToken cancel = default(CancellationToken))
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<int>> Select(PlanningContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = new List<int>();
            var parser = _evaluate ? new ExpressionEngine() : null;
            for (var i = 0; i < _rules.Count; ++i)
            {
                if (_evaluate)
                {
                    var rule = _rules[i];
                    var expression = rule.GetExpression(parser);
                    var (value, error) = expression.TryEvaluate(context.State);
                    var result = error == null && (bool)value;
                    if (result == true)
                    {
                        candidates.Add(i);
                    }
                }
                else
                {
                    candidates.Add(i);
                }
            }
            return Task.FromResult((IReadOnlyList<int>)candidates);
        }
    }
}
