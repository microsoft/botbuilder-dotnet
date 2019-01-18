using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Evaluate expression and execute actions based on the result
    /// </summary>
    /// <typeparam name="ValueT"></typeparam>
    public class IfElse : IDialogAction
    {
        public IfElse() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// Expression to evalute
        /// </summary>
        public IExpressionEval Condition { get; set; }

        /// <summary>
        /// Command to execute if true
        /// </summary>
        public IDialogAction True { get; set; }

        /// <summary>
        /// Commmand to execute if false
        /// </summary>
        public IDialogAction Else { get; set; }


        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;
            if (Condition == null)
            {
                throw new ArgumentNullException(nameof(Condition));
            }

            if (True == null)
            {
                throw new ArgumentNullException(nameof(True));
            }

            var conditionResult = (bool)await Condition.Evaluate(state);
            if (conditionResult == true)
            {
                return await this.True.Execute(dialogContext, cancellationToken);
            }
            else if (this.Else != null)
            {
                return await this.Else.Execute(dialogContext, cancellationToken);
            }

            // do nothing
            return null;
        }
    }
}
