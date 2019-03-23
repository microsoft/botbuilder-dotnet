using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class ClearProperty : DialogCommand
    {
        public ClearProperty() : base()
        { }

        public ClearProperty(string property)
            : base()
        {
            if (!string.IsNullOrEmpty(property))
            {
                this.property = property;
            }
        }

        public string property { get; set; }


        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            dc.State.SetValue(property, null);
            return await dc.EndDialogAsync();
        }
    }
}
