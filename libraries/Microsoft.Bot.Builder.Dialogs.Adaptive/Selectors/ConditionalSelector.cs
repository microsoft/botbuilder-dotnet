using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select between two rule selectors based on a condition.
    /// </summary>
    public class ConditionalSelector : IRuleSelector
    {
        private IReadOnlyList<IRule> _rules;
        private bool _evaluate;

        /// <summary>
        /// Expression that determines which selector to use.
        /// </summary>
        public Expression Condition { get; set; }

        /// <summary>
        /// Selector if <see cref="Condition"/> is true.
        /// </summary>
        public IRuleSelector IfTrue { get; set; }

        /// <summary>
        /// Selector if <see cref="Condition"/> is false.
        /// </summary>
        public IRuleSelector IfFalse { get; set; }

        public Task Initialize(PlanningContext context, IEnumerable<IRule> rules, bool evaluate = true, CancellationToken cancel = default(CancellationToken))
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<int>> Select(PlanningContext context, CancellationToken cancel = default(CancellationToken))
        {
            var (value, error) = Condition.TryEvaluate(context.State);
            var eval = error == null && (bool)value;
            IRuleSelector selector;
            if (eval)
            {
                selector = IfTrue;
                await IfTrue.Initialize(context, _rules, _evaluate, cancel).ConfigureAwait(false);
            }
            else
            {
                selector = IfFalse;
                await IfFalse.Initialize(context, _rules, _evaluate, cancel).ConfigureAwait(false);
            }
            return await selector.Select(context, cancel).ConfigureAwait(false);
        }
    }
}
