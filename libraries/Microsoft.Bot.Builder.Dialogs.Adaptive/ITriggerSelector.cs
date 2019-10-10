using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Select the trigger to execute in a given state.
    /// </summary>
    public interface ITriggerSelector
    {
        /// <summary>
        /// Initialize the selector with the set of rules.
        /// </summary>
        /// <param name="conditionHandlers">Possible rules to match.</param>
        /// <param name="evaluate">True if rules should be evaluated on select.</param>
        void Initialize(IEnumerable<OnCondition> conditionHandlers, bool evaluate = true);

        /// <summary>
        /// Select the best rule to execute.
        /// </summary>
        /// <param name="context">Dialog context for evaluation.</param>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        Task<IReadOnlyList<int>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken));
    }
}
