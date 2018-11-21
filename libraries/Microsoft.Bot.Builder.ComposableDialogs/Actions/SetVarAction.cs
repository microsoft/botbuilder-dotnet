using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.ComposableDialogs.Dialogs
{
    /// <summary>
    /// Set State variable as an action
    /// </summary>
    public class SetVarAction : IAction
    {
        public SetVarAction() { }

        public string Name { get; set; }
        
        public IExpressionEval Value { get; set; }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;
            state["DialogTurnResult"] = result;
            state[Name] = await Value.Evaluate(state);
            return result;
        }
    }
}
