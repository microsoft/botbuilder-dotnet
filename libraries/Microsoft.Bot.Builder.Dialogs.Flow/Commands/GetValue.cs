using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// GetValue - evaluate expression and return the result as the Dialog Result to caller
    /// </summary>
    public class GetValue : IDialogCommand
    {
        public GetValue() { }

        public IExpressionEval Expression { get; set; }

        public async Task<DialogTurnResult> Execute(DialogContext dialogContext, object options, DialogTurnResult result, CancellationToken cancellationToken)
        {
            if (result.Status == DialogTurnStatus.Complete && Expression != null)
            {
                var state = dialogContext.ActiveDialog.State;
                var expressionResult = await Expression.Evaluate(state);
                return new DialogTurnResult(DialogTurnStatus.Complete, expressionResult);
            }
            return result;
        }
    }
}
