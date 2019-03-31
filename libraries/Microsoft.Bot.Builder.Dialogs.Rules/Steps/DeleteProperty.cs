using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    /// <summary>
    /// Deletes a property from memory
    /// </summary>
    public class DeleteProperty : DialogCommand
    {
        public DeleteProperty() : base()
        { }

        public DeleteProperty(string property)
            : base()
        {
            if (!string.IsNullOrEmpty(property))
            {
                this.Property = property;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure planning context
            if (dc is PlanningContext planning)
            {
                dc.State.SetValue(Property, null);
                return await dc.EndDialogAsync();
            }
            else
            {
                throw new Exception("`ClearProperty` should only be used in the context of an adaptive dialog.");
            }
        }
    }
}
