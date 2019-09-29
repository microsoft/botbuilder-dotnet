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
    /// Select the first true triggerHandler implementation of <see cref="ITriggerSelector"/>.
    /// </summary>
    public class FirstSelector : ITriggerSelector
    {
        private List<OnCondition> _conditionals;
        private bool _evaluate;

        public IExpressionParser Parser { get; set;  } = new ExpressionEngine();

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            _conditionals = (from conditional in conditionals orderby conditional.Priority ascending select conditional).ToList();
            _evaluate = evaluate;
        }

        public Task<IReadOnlyList<OnCondition>> Select(SequenceContext context, CancellationToken cancel)
        {
            OnCondition selection = null;
            if (_evaluate)
            {
                for (var i = 0; i < _conditionals.Count; i++)
                {
                    var conditional = _conditionals[i];
                    var expression = conditional.GetExpression(Parser);
                    var (value, error) = expression.TryEvaluate(context.State);
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        selection = conditional;
                        break;
                    }
                }
            }
            else
            {
                if (_conditionals.Count > 0)
                {
                    selection = _conditionals[0];
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
