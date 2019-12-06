// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Goto an action by Id.
    /// </summary>
    public class GotoAction : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.GotoAction";

        public GotoAction(string actionId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.ActionId = actionId;
        }

        /// <summary>
        /// Gets or sets the action Id to goto.
        /// </summary>
        /// <value>The action Id.</value>
        public string ActionId { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var actionScopeResult = new ActionScopeResult()
            {
                ActionScopeCommand = ActionScopeCommands.GotoAction,
                ActionId = this.ActionId ?? throw new ArgumentNullException(nameof(ActionId))
            };

            return await dc.EndDialogAsync(result: actionScopeResult, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
