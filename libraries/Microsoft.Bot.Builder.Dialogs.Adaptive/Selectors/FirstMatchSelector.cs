using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    public class FirstMatchSelector : IRuleSelector
    {
        private IRule[] _rules;

        public Task<IReadOnlyList<int>> Candidates(DialogContext context, CancellationToken cancel)
        {
            var rules = new List<int>();
            for (var i = 0; i < _rules.Length; i++)
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
            return Task.FromResult((IReadOnlyList<int>) rules);
        }

        public Task Initialize(DialogContext context, IEnumerable<IRule> rules, CancellationToken cancel)
        {
            _rules = rules.ToArray();
            return Task.CompletedTask;
        }

        public Task<int> Select(DialogContext context, CancellationToken cancel)
        {
            var selection = -1;
            for (var i = 0; i < _rules.Length; i++)
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
            return Task.FromResult(selection);
        }
    }
}
