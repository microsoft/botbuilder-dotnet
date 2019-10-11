using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select all rules which evaluate to true.
    /// </summary>
    public class TrueSelector : ITriggerSelector
    {
        private List<OnCondition> _conditionals;
        private bool _evaluate;

        /// <summary>
        /// Gets or sets the expression parser to use.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IExpressionParser Parser { get; set; } = new ExpressionEngine();

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
                    var expression = conditional.GetExpression(Parser);
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
