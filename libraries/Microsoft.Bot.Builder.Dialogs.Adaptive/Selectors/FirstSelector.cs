using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the first true rule implementation of <see cref="IEventSelector"/>.
    /// </summary>
    public class FirstSelector : IEventSelector
    {
        private List<IOnEvent> _rules;
        private bool _evaluate;
        private readonly IExpressionParser _parser = new ExpressionEngine();

        public void Initialize(IEnumerable<IOnEvent> rules, bool evaluate)
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
        }

        public Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel)
        {
            var selection = -1;
            if (_evaluate)
            {
                for (var i = 0; i < _rules.Count; i++)
                {
                    var rule = _rules[i];
                    var expression = rule.GetExpression(_parser);
                    var (value, error) = expression.TryEvaluate(context.State);
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        selection = i;
                        break;
                    }
                }
            }
            else
            {
                if (_rules.Count > 0)
                {
                    selection = 0;
                }
            }
            var result = new List<int>();
            if (selection != -1)
            {
                result.Add(selection);
            }
            return Task.FromResult((IReadOnlyList<int>)result);
        }
    }
}
