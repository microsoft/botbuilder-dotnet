using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select a random true rule implementation of IRuleSelector.
    /// </summary>
    public class RandomSelector : IRuleSelector
    {
        private List<IRule> _rules;
        private bool _evaluate;
        private Random _rand;
        private int _seed = -1;

        /// <summary>
        /// Optional seed for random number generator.
        /// </summary>
        /// <remarks>If not specified a random seed will be used.</remarks>
        public int Seed
        {
            get => _seed;
            set
            {
                _seed = value;
                _rand = new Random(_seed);
            }
        }

        public Task<IReadOnlyList<int>> Candidates(DialogContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = new List<int>();
            for (var i = 0; i < _rules.Count; ++i)
            {
                if (_evaluate)
                {
                    var rule = _rules[i];
                    var expression = _rules[i].GetExpression();
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

        public Task Initialize(DialogContext context, IEnumerable<IRule> rules, bool evaluate, CancellationToken cancel = default(CancellationToken))
        {
            _rules = rules.ToList();
            _evaluate = evaluate;
            if (_rand == null)
            {
                _rand = _seed == -1 ? new Random() : new Random(_seed);
            }
            return Task.CompletedTask;
        }

        public Task<int> Select(DialogContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = Candidates(context, cancel).Result;
            var selection = _rand.Next(candidates.Count);
            return Task.FromResult(candidates[selection]);
        }
    }
}
