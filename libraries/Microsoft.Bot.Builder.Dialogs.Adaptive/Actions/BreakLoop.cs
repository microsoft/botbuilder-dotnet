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
    /// Break out of a loop.
    /// </summary>
    public class BreakLoop : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.BreakLoop";

        public BreakLoop([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var actionScopeResult = new ActionScopeResult()
            {
                ActionScopeCommand = ActionScopeCommands.BreakLoop
            };

            return await dc.EndDialogAsync(result: actionScopeResult, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
