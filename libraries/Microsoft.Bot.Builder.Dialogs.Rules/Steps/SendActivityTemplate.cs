using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class SendActivityTemplate : DialogCommand
    {
        public ITemplate<IMessageActivity> Activity { get; set; }

        public string Speak { get; set; }

        //TODO: InputHint

        public string ResultProperty { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = await Activity.BindToData(dc.Context, dc.State).ConfigureAwait(false);
            var response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return "CancelDialog()";
        }
    }
}
