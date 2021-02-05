// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.ActionPolicies
{
    /// <summary>
    /// Policy type for an <see cref="ActionPolicy"/>.
    /// </summary>
    public enum ActionPolicyType
    {
        /// <summary>
        /// Last action in the list of actions. Nothing after this action will execute.
        /// </summary>
        LastAction,

        /// <summary>
        /// Action is only a valid type for this trigger (can be in a list with others).
        /// </summary>
        AllowedTrigger,

        /// <summary>
        /// Action is an 'interactive' type, and will expect input from the user on the next turn.
        /// </summary>
        Interactive,

        /// <summary>
        /// The trigger does not allow interactive actions (no input dialogs).
        /// </summary>
        TriggerNotInteractive,

        /// <summary>
        /// Action must be present for this trigger (can be in a list of options)
        /// </summary>
        TriggerRequiresAction,
    }
}
