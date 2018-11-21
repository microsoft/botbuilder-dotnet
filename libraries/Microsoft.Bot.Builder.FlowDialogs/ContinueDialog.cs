using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.FlowDialogs
{
    /// <summary>
    /// Continue the current dialog 
    /// </summary>
    public class ContinueDialog : IFlowCommand
    {
        public ContinueDialog() { }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            return await dialogContext.ContinueDialogAsync(cancellationToken);
        }
    }
}
