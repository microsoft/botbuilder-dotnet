using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class DebugBreak : Dialog
    {
        public DebugBreak([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
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
                var stepState = dc is SequenceContext sc ? sc.Actions : new List<ActionState>();
                var actionsIds = stepState.Select(s => s.DialogId);

                Debug.WriteLine($"{path}: {actionsIds.Count()} actions remaining.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to collect full debug dump. Error: {ex.ToString()}");
            }
        }
    }
}
