using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.TriggerTrees;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the most specific true rule implementation of <see cref="IRuleSelector"/>.
    /// </summary>
    public class MostSpecificSelector : IRuleSelector
    {
        private readonly TriggerTree _tree = new TriggerTree();

        /// <summary>
        /// Optional rule selector to use when more than one most specific rule is true.
        /// </summary>
        public IRuleSelector Selector { get; set; }

        public Task<IReadOnlyList<int>> Candidates(DialogContext context, CancellationToken cancel)
        {
            var nodes = _tree.Matches(context.State);
            var rules = new List<int>();
            foreach(var node in nodes)
            {
                foreach(var trigger in node.AllTriggers)
                {
                    var (pos, _rule) = (ValueTuple<int, IRule>) trigger.Action;
                    rules.Add(pos);
                }
            }
            return Task.FromResult((IReadOnlyList<int>)rules);
        }

        public Task Initialize(DialogContext context, IEnumerable<IRule> rules, bool evaluate, CancellationToken cancel)
        {
            var i = 0;
            foreach (var rule in rules)
            {
                _tree.AddTrigger(rule.GetExpression(), (i, rule));
                ++i;
            }
            return Task.CompletedTask;
        }

        public async Task<int> Select(DialogContext context, CancellationToken cancel)
        {
            var selection = -1;
            var nodes = _tree.Matches(context.State);
            var candidates = new List<ValueTuple<int, IRule>>();
            foreach (var node in nodes)
            {
                foreach (var trigger in node.AllTriggers)
                {
                    candidates.Add((ValueTuple<int, IRule>)trigger.Action);
                }
            }
            if (Selector == null)
            {
                // Pick rules by order defined
                foreach(var candidate in candidates)
                {
                    var (pos, rule) = candidate;
                    if (pos < selection || selection == -1)
                    {
                        selection = pos;
                    }
                }
            }
            else
            {
                // Sort rules by original order and then pass to child selector
                var rules = (from candidate in candidates orderby candidate.Item1 ascending select candidate.Item2);
                await Selector.Initialize(context, rules, false, cancel).ConfigureAwait(false);
                selection = await Selector.Select(context, cancel).ConfigureAwait(false);
            }
            return selection;
        }
    }
}
