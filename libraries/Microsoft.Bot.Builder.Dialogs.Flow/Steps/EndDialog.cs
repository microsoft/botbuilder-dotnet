using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// end the current dialog 
    /// </summary>
    public class EndDialog : IDialogStep
    {
        public EndDialog() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// Expression to evaluate to get the result for the dialog
        /// </summary>
        public IExpressionEval DialogResult { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            if (DialogResult != null)
            {
                var state = dialogContext.ActiveDialog.State;
                var expressionResult = await DialogResult.Evaluate(state);
                return await dialogContext.EndDialogAsync(expressionResult, cancellationToken);
            }
            return await dialogContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
