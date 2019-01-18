using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Composition.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// Set State variable as an action
    /// </summary>
    public class SetVar : IDialogAction
    {
        public SetVar() { }

        /// <summary>
        /// (OPTIONAL) Id of the command
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        public string Name { get; set; }

        public IExpressionEval Value { get; set; }

        public async Task<object> Execute(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            if (Value == null)
            {
                throw new ArgumentNullException(nameof(Value));
            }
            var state = dialogContext.ActiveDialog.State;
            state[Name.Trim()] = await Value.Evaluate(state);
            return null;
        }
    }
}
