using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Defines a flow command 
    /// </summary>
    public interface IFlowCommand
    {
        /// <summary>
        /// Execute an commmand 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>DialogTurnResult</returns>
        Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken);
    }

}
