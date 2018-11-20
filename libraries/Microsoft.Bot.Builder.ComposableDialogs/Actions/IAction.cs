using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    public interface IAction
    {
        /// <summary>
        /// Execute an action 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>DialogTurnResult</returns>
        Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken);
    }

}
