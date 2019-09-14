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

        public Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = new List<int>();
            var parser = _evaluate ? new ExpressionEngine() : null;
            for (var i = 0; i < _triggerHandlers.Count; ++i)
            {
                if (_evaluate)
                {
                    var triggerHandler = _triggerHandlers[i];
                    var expression = triggerHandler.GetExpression(parser);
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
