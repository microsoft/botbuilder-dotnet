using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select between two rule selectors based on a condition.
    /// </summary>
    public class ConditionalSelector : ITriggerSelector
    {
        private IReadOnlyList<OnCondition> _conditionals;
        private bool _evaluate;
        private Expression condition;

        /// <summary>
        /// Gets or sets expression that determines which selector to use.
        /// </summary>
        /// <value>
        /// Expression that determines which selector to use.
        /// </value>
        public string Condition
        {
            get { return condition?.ToString(); }
            set { this.condition = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets selector if <see cref="Condition"/> is true.
        /// </summary>
        /// <value>
        /// Selector if <see cref="Condition"/> is true.
        /// </value>
        public ITriggerSelector IfTrue { get; set; }

        /// <summary>
        /// Gets or sets selector if <see cref="Condition"/> is false.
        /// </summary>
        /// <value>
        /// Selector if <see cref="Condition"/> is false.
        /// </value>
        public ITriggerSelector IfFalse { get; set; }

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate = true)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        public async Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var (value, error) = condition.TryEvaluate(context.State);
            var eval = error == null && (bool)value;
            ITriggerSelector selector;
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

            return await selector.Select(context, cancel).ConfigureAwait(false);
        }
    }
}
