// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Select the trigger to execute in a given state.
    /// </summary>
    public abstract class TriggerSelector
    {
        /// <summary>
        /// Initialize the selector with the set of rules.
        /// </summary>
        /// <param name="conditionHandlers">Possible rules to match.</param>
        /// <param name="evaluate">True if rules should be evaluated on select.</param>
        public abstract void Initialize(IEnumerable<OnCondition> conditionHandlers, bool evaluate = true);

        /// <summary>
        /// Select the best rule to execute.
        /// </summary>
        /// <param name="actionContext">Dialog context for evaluation.</param>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        public abstract Task<IReadOnlyList<OnCondition>> Select(ActionContext actionContext, CancellationToken cancel = default);
    }
}
