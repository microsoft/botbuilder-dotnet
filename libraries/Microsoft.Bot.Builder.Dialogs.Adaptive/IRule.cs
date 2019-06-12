// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public interface IRule
    {
        /// <summary>
        /// Get the expression for this rule.
        /// </summary>
        /// <param name="parser">Expression parser to use.</param>
        Expression GetExpression(IExpressionParser parser);

        /// <summary>
        /// Execute the action for this rule
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        Task<List<StepChangeList>> ExecuteAsync(SequenceContext context);

        /// <summary>
        /// Steps to add to the plan when the rule is activated
        /// </summary>
        List<IDialog> Steps { get; }
    }
}
