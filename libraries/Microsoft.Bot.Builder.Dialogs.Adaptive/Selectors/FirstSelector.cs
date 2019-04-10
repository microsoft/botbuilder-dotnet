using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the first true rule implementation of <see cref="IRuleSelector"/>.
    /// </summary>
    public class FirstSelector : IRuleSelector
    {
        private List<IRule> _rules;
        private bool _evaluate;

        public Task<IReadOnlyList<int>> Candidates(DialogContext context, CancellationToken cancel)
        {
            var rules = new List<int>();
            for (var i = 0; i < _rules.Count; i++)
            {
                if (_evaluate)
                {
                    var rule = _rules[i];
                    var expression = _rules[i].GetExpression();
                    var (value, error) = expression.TryEvaluate(context.State);
                    var result = error == null && (bool)value;
                    if (result == true)
                    {
                        rules.Add(i);
                    }
                }
                else
                {
                    rules.Add(i);
                }
            }
            return Task.FromResult((IReadOnlyList<int>)rules);
        }

        public Task Initialize(DialogContext context, IEnumerable<IRule> rules, bool evaluate, CancellationToken cancel)
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
            return Task.CompletedTask;
        }

        public Task<int> Select(DialogContext context, CancellationToken cancel)
        {
            var selection = -1;
            if (_evaluate)
            {
                for (var i = 0; i < _rules.Count; i++)
                {
                    var rule = _rules[i];
                    var expression = _rules[i].GetExpression();
                    var (value, error) = expression.TryEvaluate(context.State);
                    var result = error == null && (bool)value;
                    if (result == true)
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
            return Task.FromResult(selection);
        }
    }
}
