// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public interface IOnEvent
    {
        /// <summary>
        /// Gets actions to add to the plan when the rule is activated.
        /// </summary>
        /// <value>
        /// Actions to add to the plan when the rule is activated.
        /// </value>
        List<IDialog> Actions { get; }

        /// <summary>
        /// Get the expression for this rule.
        /// </summary>
        /// <param name="parser">Expression parser to use.</param>
        /// <returns>The expression for the rule.</returns>
        Expression GetExpression(IExpressionParser parser);

        /// <summary>
        /// Execute the action for this rule.
        /// </summary>
        /// <param name="context">Dialog sequence context.</param>
        /// <returns>Task with plan change list.</returns>
        Task<List<ActionChangeList>> ExecuteAsync(SequenceContext context);
    }
}
