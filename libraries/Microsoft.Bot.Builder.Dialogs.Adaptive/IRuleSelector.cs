using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Select the rule to execute in a given state.
    /// </summary>
    public interface IRuleSelector
    {
        /// <summary>
        /// Initialize the selector with the set of rules.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="cancel">Cancellation token.</param>
        /// <param name="rules">Possible rules to match.</param>
        Task Initialize(DialogContext context, IEnumerable<IRule> rules, bool evaluate = true, CancellationToken cancel = default(CancellationToken));

        /// <summary>
        /// Return the set of rules that are candidates to run.
        /// </summary>
        /// <remarks>This is mainly useful for cascading rule selectors, i.e. return all active rules for something like conversation learner.</remarks>
        /// <param name="context">Dialog context for evaluation.</param>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Possible rule candidates.</returns>
        Task<IReadOnlyList<int>> Candidates(DialogContext context, CancellationToken cancel = default(CancellationToken));

        /// <summary>
        /// Select the best rule to execute.
        /// </summary>
        /// <param name="context">Dialog context for evaluation.</param>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Best rule in original list to execute or -1 if none.</returns>
        Task<int> Select(DialogContext context, CancellationToken cancel = default(CancellationToken));
    }
}
