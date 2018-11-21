using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.FlowDialogs
{
    /// <summary>
    /// Clear a variable as an action
    /// </summary>
    public class ClearVar : IFlowAction
    {
        public ClearVar() { }

        public string Name { get; set; }
        
        public Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;
            state["DialogTurnResult"] = result;
            state.Remove(Name);
            return Task.FromResult(result);
        }
    }
}
