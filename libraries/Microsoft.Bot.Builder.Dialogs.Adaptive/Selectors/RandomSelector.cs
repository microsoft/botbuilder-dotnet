using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select a random true rule implementation of IRuleSelector.
    /// </summary>
    public class RandomSelector : ITriggerSelector
    {
        private List<TriggerHandler> _triggerHandlers;
        private bool _evaluate;
        private Random _rand;
        private int _seed = -1;
        private IExpressionParser _parser = new ExpressionEngine();

        /// <summary>
        /// Gets or sets optional seed for random number generator.
        /// </summary>
        /// <remarks>If not specified a random seed will be used.</remarks>
        /// <value>
        /// Optional seed for random number generator.
        /// </value>
        public int Seed
        {
            get => _seed;
            set
            {
                _seed = value;
                _rand = new Random(_seed);
            }
        }

        public void Initialize(IEnumerable<TriggerHandler> triggerHandlers, bool evaluate)
        {
            _triggerHandlers = triggerHandlers.ToList();
            _evaluate = evaluate;
            if (_rand == null)
            {
                _rand = _seed == -1 ? new Random() : new Random(_seed);
            }
        }

        public Task<IReadOnlyList<IOnEvent>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = _rules;
            if (_evaluate)
            {
                candidates = new List<IOnEvent>();
                foreach (var rule in _rules)
                {
                    var expression = rule.GetExpression(_parser);
                    var (value, error) = expression.TryEvaluate(context.State);
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        candidates.Add(rule);
                    }
                }
            }

            var result = new List<IOnEvent>();
            if (candidates.Count > 0)
            {
                var selection = _rand.Next(candidates.Count);
                result.Add(candidates[selection]);
            }

            return Task.FromResult((IReadOnlyList<IOnEvent>)result);
        }
    }
}
