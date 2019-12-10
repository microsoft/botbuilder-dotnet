// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select between two rule selectors based on a condition.
    /// </summary>
    public class ConditionalSelector : ITriggerSelector
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.ConditionalSelector";

        private IReadOnlyList<OnCondition> _conditionals;
        private bool _evaluate;
        private Expression condition;

        /// <summary>
        /// Gets or sets expression that determines which selector to use.
        /// </summary>
        /// <value>
        /// Expression that determines which selector to use.
        /// </value>
        [JsonProperty("condition")]
        public string Condition
        {
            get => condition?.ToString(); 
            set => this.condition = (value != null) ? Parser.Parse(value) : null;
        }

        /// <summary>
        /// Gets or sets selector if <see cref="Condition"/> is true.
        /// </summary>
        /// <value>
        /// Selector if <see cref="Condition"/> is true.
        /// </value>
        [JsonProperty("ifTrue")]
        public ITriggerSelector IfTrue { get; set; }

        /// <summary>
        /// Gets or sets selector if <see cref="Condition"/> is false.
        /// </summary>
        /// <value>
        /// Selector if <see cref="Condition"/> is false.
        /// </value>
        [JsonProperty("ifFalse")]
        public ITriggerSelector IfFalse { get; set; }

        /// <summary>
        /// Gets or sets the expression parser to use.
        /// </summary>
        /// <value>Expression parser.</value>
        [Newtonsoft.Json.JsonIgnore]
        public IExpressionParser Parser { get; set; } = new ExpressionEngine();

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate = true)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        public async Task<IReadOnlyList<OnCondition>> Select(SequenceContext context, CancellationToken cancel = default)
        {
            var (value, error) = condition.TryEvaluate(context.GetState());
            var eval = error == null && (bool)value;
            ITriggerSelector selector;
            if (eval)
            {
                selector = IfTrue;
                IfTrue.Initialize(_conditionals, _evaluate);
            }
            else
            {
                selector = IfFalse;
                IfFalse.Initialize(_conditionals, _evaluate);
            }

            return await selector.Select(context, cancel).ConfigureAwait(false);
        }
    }
}
