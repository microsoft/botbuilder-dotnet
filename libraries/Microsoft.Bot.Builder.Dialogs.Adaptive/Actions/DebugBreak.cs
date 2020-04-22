// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class DebugBreak : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.DebugBreak";

        public DebugBreak([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            DebugDump(dc);

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private void DebugDump(DialogContext dc)
        {
            // Best effort
            try
            {
                // Compute path
                var path = string.Empty;
                var connector = string.Empty;

                var current = dc.Parent;
                while (current != null)
                {
                    path = current.ActiveDialog?.Id ?? string.Empty + connector + path;
                    connector = "/";
                    current = current.Parent;
                }

                // Get list of actions
                var actions = dc.Parent is ActionContext ac ? ac.Actions : new List<ActionState>();
                var actionsIds = actions.Select(s => s.DialogId);

                Debug.WriteLine($"{path}: {actionsIds.Count()} actions remaining.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to collect full debug dump. Error: {ex.ToString()}");
            }
        }
    }
}
