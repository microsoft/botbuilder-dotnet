// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action or Trigger which has <see cref="ActionPolicy"/>s.
    /// </summary>
    public interface IActionPolicies
    {
        /// <summary>
        /// Gets the action policies for this condition.
        /// </summary>
        /// <value>
        /// IEnumerable of <see cref="ActionPolicy"/>.
        /// </value>
        IEnumerable<ActionPolicy> ActionPolicies { get; }
    }
}
