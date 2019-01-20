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
    public class IfElseStep : IStep
    {
        public IfElseStep() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Expression to evalute
        /// </summary>
        public IExpressionEval Condition { get; set; }

        /// <summary>
        /// Command to execute if true
        /// </summary>
        public IStep IfTrue { get; set; }

        /// <summary>
        /// Commmand to execute if false
        /// </summary>
        public IStep IfFalse { get; set; }


        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var state = dialogContext.ActiveDialog.State;
            if (Condition == null)
            {
                throw new ArgumentNullException(nameof(Condition));
            }

            if (IfTrue == null)
            {
                throw new ArgumentNullException(nameof(IfTrue));
            }

            var conditionResult = (bool)await Condition.Evaluate(state);
            if (conditionResult == true)
            {
                return await this.IfTrue.Execute(dialogContext, cancellationToken);
            }
            else if (this.IfFalse != null)
            {
                return await this.IfFalse.Execute(dialogContext, cancellationToken);
            }

            // do nothing
            return null;
        }
    }
}
