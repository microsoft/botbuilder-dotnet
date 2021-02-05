// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.ActionPolicies
{
    /// <summary>
    /// Exception thrown during Action Policy validation.
    /// </summary>
#pragma warning disable CA1032 // Implement standard exception constructors
    public class ActionPolicyException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPolicyException"/> class.
        /// </summary>
        /// <param name="actionPolicy">The <see cref="ActionPolicy"/> which was violated.</param>
        /// <param name="dialog">The dialog which violated the ActionPolicy.</param>
        public ActionPolicyException(ActionPolicy actionPolicy, Dialog dialog = null)
        {
            this.ActionPolicy = actionPolicy;
            this.Dialog = dialog;
        }

        /// <summary>
        /// Gets or sets the Action Policy which was violated.
        /// </summary>
        /// <value>
        /// The Action Policy which was violated.
        /// </value>
        public ActionPolicy ActionPolicy { get; set; }

        /// <summary>
        /// Gets or sets the Dialog which had an invalid action policy.
        /// </summary>
        /// <value>
        /// The Dialog which had an invalid action policy.
        /// </value>
        public Dialog Dialog { get; set; }
    }
}
