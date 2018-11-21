using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.FlowDialogs
{
    /// <summary>
    /// Execute set of actions in sequence
    /// </summary>
    /// <remarks>
    /// The behavior of executing multiple actions that try to manipulate the DialogStack is not defined.  
    /// </remarks>
    public class FlowActionSet : IFlowAction
    {
        public FlowActionSet(List<IFlowAction> actions = null)
        {
            if (actions != null)
                this.Actions = actions;
        }

        public List<IFlowAction> Actions { get; set; } = new List<IFlowAction>();

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            foreach (var action in Actions)
            {
                result = await action.Execute(dialogContext, options, result, cancellationToken);
            }
            return result;
        }
    }
}
