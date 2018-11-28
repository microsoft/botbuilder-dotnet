using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Continue the current dialog 
    /// </summary>
    public class ContinueDialog : IDialogCommand
    {
        public ContinueDialog() { }

        public Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            return Task.FromResult(new DialogTurnResult(DialogTurnStatus.Waiting));
        }
    }
}
