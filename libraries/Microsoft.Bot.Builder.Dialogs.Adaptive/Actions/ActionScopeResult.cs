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
        /// <value>See ActionScopeCommands constants (BreakCommand|GotoCommand|ContinueCommand).</value>
        public string ActionScopeCommand { get; set; }

        /// <summary>
        /// Gets or sets the action Id.
        /// </summary>
        /// <value>actionId to target (for commands like GotoCommand).</value>
        public string ActionId { get; set; }
    }
}
