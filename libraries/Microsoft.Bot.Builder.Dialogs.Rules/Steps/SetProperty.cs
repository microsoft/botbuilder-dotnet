using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class SetProperty : DialogCommand
    {
        public SetProperty() : base()
        { }

        /// <summary>
        /// Value expression
        /// </summary>
        public Expression Value { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                // Simply evaluate the expression, for example user.name = 'Carlos'
                // Consider renaming this to EvaluateExpression rather than SetProperty
                // Otherwise we should have property and value properties
                var value = Value.TryEvaluate(dc.State);
                return await planning.EndDialogAsync(value, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`SetProperty` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            return $"SetProperty[${this.Property.ToString() ?? string.Empty}]";
        }
    }
}
