using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class SaveEntity : DialogCommand
    {
        public SaveEntity() : base()
        { }

        public SaveEntity(string entityName, string property)
            : base()
        {
            if (!string.IsNullOrEmpty(entityName))
            {
                this.entityName = entityName;
            }

            if (!string.IsNullOrEmpty(property))
            {
                this.property = property;
            }
        }

        public string entityName;

        public string property;


        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO Get the entities from turn context and set to the user state.
            return await dc.EndDialogAsync();
        }
    }
}
