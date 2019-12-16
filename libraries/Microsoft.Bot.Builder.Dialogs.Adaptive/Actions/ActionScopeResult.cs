// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class ActionScopeResult
    {
        /// <summary>
        /// Gets or sets the action scope to take. 
        /// </summary>
        /// <value>See ActionScopeCommands constants (GotoAction|BreakLoop|ContinueLoop).</value>
        public string ActionScopeCommand { get; set; }

        /// <summary>
        /// Gets or sets the action Id.
        /// </summary>
        /// <value>actionId to target (for commands like GotoAction).</value>
        public string ActionId { get; set; }
    }
}
