using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select between two rule selectors based on a condition.
    /// </summary>
    public class ConditionalSelector : IEventSelector
    {
        private IReadOnlyList<IOnEvent> _rules;
        private bool _evaluate;
        private Expression condition;

        /// <summary>
        /// Expression that determines which selector to use.
        /// </summary>
        public string Condition
        {
            get { return condition?.ToString(); }
            set {this.condition = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Selector if <see cref="Condition"/> is true.
        /// </summary>
        public IEventSelector IfTrue { get; set; }

        /// <summary>
        /// Selector if <see cref="Condition"/> is false.
        /// </summary>
        public IEventSelector IfFalse { get; set; }

        public void Initialize(IEnumerable<IOnEvent> rules, bool evaluate = true)
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
        }

        public async Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var (value, error) = condition.TryEvaluate(context.State);
            var eval = error == null && (bool)value;
            IEventSelector selector;
            if (eval)
            {
                selector = IfTrue;
                IfTrue.Initialize(_rules, _evaluate);
            }
            else
            {
                selector = IfFalse;
                IfFalse.Initialize(_rules, _evaluate);
            }
            return await selector.Select(context, cancel).ConfigureAwait(false);
        }
    }
}
