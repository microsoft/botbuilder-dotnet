using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select all rules which evaluate to true.
    /// </summary>
    public class TrueSelector : ITriggerSelector
    {
        private List<TriggerHandler> _triggerHandlers;
        private bool _evaluate;

        public void Initialize(IEnumerable<TriggerHandler> triggerHandlers, bool evaluate = true)
        {
            _triggerHandlers = triggerHandlers.ToList();
            _evaluate = evaluate;
        }

        public Task<IReadOnlyList<TriggerHandler>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = _triggerHandlers;
            if (_evaluate)
            {
                var parser = new ExpressionEngine();
                candidates = new List<TriggerHandler>();
                foreach (var rule in _triggerHandlers)
                {
                    var expression = rule.GetExpression(parser);
                    var (value, error) = expression.TryEvaluate(context.State);
                    var result = error == null && (bool)value;
                    if (result == true)
                    {
                        candidates.Add(rule);
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<TriggerHandler>)candidates);
        }
    }
}
