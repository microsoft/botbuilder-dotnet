using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.TriggerTrees;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the most specific true rule implementation of <see cref="ITriggerSelector"/>.
    /// </summary>
    public class MostSpecificSelector : ITriggerSelector
    {
        private readonly TriggerTree _tree = new TriggerTree();

        /// <summary>
        /// Gets or sets optional rule selector to use when more than one most specific rule is true.
        /// </summary>
        /// <value>
        /// Optional rule selector to use when more than one most specific rule is true.
        /// </value>
        public ITriggerSelector Selector { get; set; }

        /// <summary>
        /// Gets or sets the expression parser to use.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IExpressionParser Parser { get; set; } = new ExpressionEngine(TriggerTree.LookupFunction);

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            foreach (var conditional in conditionals)
            {
                _tree.AddTrigger(conditional.GetExpression(Parser), conditional);
            }
        }

        public virtual async Task<IReadOnlyList<OnCondition>> Select(SequenceContext context, CancellationToken cancel)
        {
            var nodes = _tree.Matches(context.State);
            var matches = new List<OnCondition>();
            foreach (var node in nodes)
            {
                foreach (var trigger in node.AllTriggers)
                {
                    matches.Add(trigger.Action as OnCondition);
                }
            }

            IReadOnlyList<OnCondition> selections = matches;
            if (Selector != null)
            {
                Selector.Initialize(matches, false);
                selections = await Selector.Select(context, cancel).ConfigureAwait(false);
            }

            return selections;
        }
    }
}
