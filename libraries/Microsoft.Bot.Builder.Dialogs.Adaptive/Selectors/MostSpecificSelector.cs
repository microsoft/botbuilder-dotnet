using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.TriggerTrees;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the most specific true rule implementation of <see cref="IEventSelector"/>.
    /// </summary>
    public class MostSpecificSelector : IEventSelector
    {
        private readonly TriggerTree _tree = new TriggerTree();

        /// <summary>
        /// Gets or sets optional rule selector to use when more than one most specific rule is true.
        /// </summary>
        /// <value>
        /// Optional rule selector to use when more than one most specific rule is true.
        /// </value>
        public IEventSelector Selector { get; set; }

        public void Initialize(IEnumerable<IOnEvent> rules, bool evaluate)
        {
            var i = 0;
            var parser = new ExpressionEngine(TriggerTree.LookupFunction);
            foreach (var rule in rules)
            {
                _tree.AddTrigger(rule.GetExpression(parser), (i, rule));
                ++i;
            }
        }

        public async Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel)
        {
            var nodes = _tree.Matches(context.State);
            IReadOnlyList<int> selections;
            if (Selector == null)
            {
                // Return all matches
                var matches = new List<int>();
                foreach (var node in nodes)
                {
                    foreach (var trigger in node.AllTriggers)
                    {
                        var (pos, rule) = (ValueTuple<int, IOnEvent>)trigger.Action;
                        matches.Add(pos);
                    }
                }

                selections = matches;
            }
            else
            {
                var matches = new List<ValueTuple<int, IOnEvent>>();
                foreach (var node in nodes)
                {
                    foreach (var trigger in node.AllTriggers)
                    {
                        matches.Add((ValueTuple<int, IOnEvent>)trigger.Action);
                    }
                }

                // Sort rules by original order and then pass to child selector
                matches = (from candidate in matches orderby candidate.Item1 ascending select candidate).ToList();
                Selector.Initialize(matches.Select(m => m.Item2), false);
                selections = (from match in await Selector.Select(context, cancel).ConfigureAwait(false) select matches[match].Item1).ToList();
            }

            return selections;
        }
    }
}
