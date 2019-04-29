using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    public class DebugBreak : DialogCommand
    {
        public DebugBreak([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
