using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.ComposableDialogs.Dialogs
{
    /// <summary>
    /// Execute set of actions in sequence
    /// </summary>
    /// <remarks>
    /// The behavior of executing multiple actions that try to manipulate the DialogStack is not defined.  
    /// </remarks>
    public class ActionSet : IAction
    {
        public ActionSet(List<IAction> actions = null)
        {
            if (actions != null)
                this.Actions = actions;
        }

        public List<IAction> Actions { get; set; } = new List<IAction>();

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
